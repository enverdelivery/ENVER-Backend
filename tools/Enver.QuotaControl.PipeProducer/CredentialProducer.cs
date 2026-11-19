using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;

namespace Enver.QuotaControl.PipeProducer;

public enum ProducerFailure { InvalidInput, UnsupportedPlatform, ArtifactMismatch, ChildStart, ChildFailure, InvalidOutput, Cancelled, Deadline, Cleanup }

public sealed class ProducerException(ProducerFailure failure) : Exception(failure.ToString())
{
    public ProducerFailure Failure { get; } = failure;
}

public sealed record ProducerReceipt(int ProcessId, int ExitCode, long ElapsedMilliseconds, InventoryEvidence? Evidence = null);

// Biblioteca, no CLI/broker AWS. El creador conserva y borra su memoria de entrada.
// No genera ni lee passwords, archivos de credenciales, ENV, consola o clipboard.
public static class CredentialProducer
{
    public const string ControlDllSha256 = "6b32414eb3a66cf9339aa066af2049da11f21339c095427204d146d42622823f";
    public const string DescriptorSha256 = "52f6cc3519295c3e11ce4d8a07f64b4c896e3ac8bf502bfe0b684b15a0142ca8";
    public static TimeSpan MaximumLifetime => TimeSpan.FromSeconds(45);

    // Solo inventory y ruta aprobada. Invocarla operativamente requiere otra autorización.
    public static Task<ProducerReceipt> RunInventoryAsync(string workspace, ReadOnlyMemory<byte> password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var root = Path.GetFullPath(workspace);
            var dll = Path.Combine(root, "tools", "Enver.QuotaControl", "bin", "Release", "net10.0", "Enver.QuotaControl.dll");
            var descriptor = Path.Combine(root, "tools", "Enver.QuotaControl", "descriptor.approved.json");
            VerifyFile(dll, ControlDllSha256);
            VerifyFile(descriptor, DescriptorSha256);
            var args = new[] { dll, "inventory", "--descriptor", descriptor, "--transport", "ssm-tunnel", "--tunnel-port", "16379" };
            return RunCoreAsync(new(DotnetPath, args), password, MaximumLifetime, cancellationToken, captureInventory: true);
        }
        catch (ProducerException) { throw; }
        catch { throw new ProducerException(ProducerFailure.ArtifactMismatch); }
    }

    internal static string DotnetPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe");

    private static void VerifyFile(string file, string hash)
    {
        for (FileSystemInfo? item = new FileInfo(file); item is not null;
             item = item is FileInfo f ? f.Directory : ((DirectoryInfo)item).Parent)
            if ((item.Attributes & FileAttributes.ReparsePoint) != 0) throw new ProducerException(ProducerFailure.ArtifactMismatch);
        using var input = File.OpenRead(file);
        if (!CryptographicOperations.FixedTimeEquals(SHA256.HashData(input), Convert.FromHexString(hash)))
            throw new ProducerException(ProducerFailure.ArtifactMismatch);
    }

    internal static async Task<ProducerReceipt> RunCoreAsync(ChildLaunch launch, ReadOnlyMemory<byte> password,
        TimeSpan lifetime, CancellationToken cancellationToken, bool captureInventory = false)
    {
        if (!OperatingSystem.IsWindows() || !Environment.Is64BitProcess)
            throw new ProducerException(ProducerFailure.UnsupportedPlatform);
        Validate(password.Span);
        if (lifetime <= TimeSpan.Zero || lifetime > MaximumLifetime) throw new ProducerException(ProducerFailure.InvalidInput);
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(lifetime);
        NativeChild? child = null;
        AnonymousPipeServerStream? output = null;
        Task<byte[]>? readOutput = null;
        byte[]? frame = null;
        var clock = Stopwatch.StartNew();
        try
        {
            budget.Token.ThrowIfCancellationRequested();
            // El pipe original NO es heredable. Solo se duplica el extremo lector para CreateProcess.
            using var pipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.None);
            using var reader = WindowsProcess.DuplicateForChild(pipe.ClientSafePipeHandle);
            output = captureInventory ? new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.None) : null;
            using var outputWriter = output is null ? null : WindowsProcess.DuplicateForChild(output.ClientSafePipeHandle);
            var args = launch.Arguments.Concat(new[] { "--credential-pipe", reader.DangerousGetHandle().ToInt64().ToString(System.Globalization.CultureInfo.InvariantCulture) }).ToArray();
            NoExposure(password.Span, args.Append(launch.Executable).Concat(WindowsProcess.EnvironmentEntries));
            child = WindowsProcess.Start(launch.Executable, args, new[] { reader.DangerousGetHandle() }, () =>
            {
                reader.Dispose();
                pipe.DisposeLocalCopyOfClientHandle();
                outputWriter?.Dispose();
                output?.DisposeLocalCopyOfClientHandle();
            }, outputWriter?.DangerousGetHandle());
            if (output is not null) readOutput = Task.Run(() => InventoryEvidence.ReadBounded(output));
            frame = new byte[4 + password.Length];
            BinaryPrimitives.WriteUInt32BigEndian(frame, (uint)password.Length);
            password.Span.CopyTo(frame.AsSpan(4));
            // <=132 bytes: inferior al buffer del pipe; no se espera flush del consumidor.
            await pipe.WriteAsync(frame, budget.Token).ConfigureAwait(false);
            CryptographicOperations.ZeroMemory(frame);
            pipe.Dispose(); // EOF y cierre del escritor inmediatamente tras el frame.
            var exit = await child.WaitAsync(budget.Token).ConfigureAwait(false);
            InventoryEvidence? evidence = null;
            if (readOutput is not null)
            {
                var bytes = await readOutput.WaitAsync(budget.Token).ConfigureAwait(false);
                try { evidence = InventoryEvidence.Parse(bytes, exit, password.Span); }
                finally { CryptographicOperations.ZeroMemory(bytes); }
            }
            return new(child.Id, exit, clock.ElapsedMilliseconds, evidence);
        }
        catch (OperationCanceledException)
        {
            throw new ProducerException(cancellationToken.IsCancellationRequested ? ProducerFailure.Cancelled : ProducerFailure.Deadline);
        }
        catch (ProducerException) { throw; }
        catch { throw new ProducerException(ProducerFailure.ChildFailure); }
        finally
        {
            if (frame is not null) CryptographicOperations.ZeroMemory(frame);
            try { child?.Dispose(); } // Kill-on-job-close; solo el árbol hijo propio.
            finally
            {
                output?.Dispose();
                if (readOutput is not null)
                {
                    try
                    {
                        var pending = await readOutput.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                        CryptographicOperations.ZeroMemory(pending);
                    }
                    catch (TimeoutException) { throw new ProducerException(ProducerFailure.Cleanup); }
                    catch { /* El fallo original permanece saneado; nunca exportar stdout crudo. */ }
                }
            }
        }
    }

    internal static void Validate(ReadOnlySpan<byte> password)
    {
        if (password.Length is < 16 or > 128) throw new ProducerException(ProducerFailure.InvalidInput);
        foreach (var b in password)
            if (b < 33 || b > 126 || b is (byte)',' or (byte)'"' or (byte)'/' or (byte)'@')
                throw new ProducerException(ProducerFailure.InvalidInput);
    }

    internal static void NoExposure(ReadOnlySpan<byte> secret, IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            var encoded = Encoding.UTF8.GetBytes(value);
            try { if (encoded.AsSpan().IndexOf(secret) >= 0) throw new ProducerException(ProducerFailure.InvalidInput); }
            finally { CryptographicOperations.ZeroMemory(encoded); }
        }
    }
}

internal sealed record ChildLaunch(string Executable, IReadOnlyList<string> Arguments);
