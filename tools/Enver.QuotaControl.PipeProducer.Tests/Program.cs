using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Enver.QuotaControl;
using Enver.QuotaControl.PipeProducer;

namespace Enver.QuotaControl.PipeProducer.Tests;

internal static class Program
{
    // Fixture no operativa, nunca se genera/usa una credencial real.
    private static byte[] Fixture(int n = 64) => Enumerable.Range(0, n).Select(i => (byte)('A' + i % 26)).ToArray();
    private static string Assembly => typeof(Program).Assembly.Location;
    private static ChildLaunch Child(string mode, params string[] extra) => new(CredentialProducer.DotnetPath,
        new[] { Assembly, "child", mode }.Concat(extra).ToArray());

    public static async Task<int> Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "child")
        {
            try { return await ChildMain(args); }
            catch { return 99; } // Jamás raw excepción/payload.
        }
        var tests = new List<(string Name, Func<Task> Run)>
        {
            ("exact-frame-eof-and-no-secret-in-argv-env", async () => await Success("receive")),
            ("compatible-with-existing-consumer", async () => await Success("consumer")),
            ("minimum-length", async () => await Success("receive", 16)),
            ("maximum-length", async () => await Success("receive", 128)),
            ("child-secret-stdout-stderr-discarded", async () => await Success("echo")),
            ("reader-handle-closed-explicitly", async () => await Success("close")),
            ("only-explicit-nonsecret-environment", async () => await Success("environment")),
            ("bounded-typed-inventory-output", async () =>
            {
                var r = await CredentialProducer.RunCoreAsync(Child("report"), Fixture(), TimeSpan.FromSeconds(5), default, true);
                Check(r.ExitCode == 0 && r.Evidence is { Pages: 1, Completed: true, MetadataPresent: false });
            }),
            ("secret-in-output-fails-without-export", async () => await Fails(ProducerFailure.InvalidOutput,
                () => CredentialProducer.RunCoreAsync(Child("echo"), Fixture(), TimeSpan.FromSeconds(5), default, true))),
            ("oversized-output-fails-closed", async () => await Fails(ProducerFailure.InvalidOutput,
                () => CredentialProducer.RunCoreAsync(Child("oversize"), Fixture(), TimeSpan.FromSeconds(5), default, true))),
            ("encoded-secret-output-rejected", () =>
            {
                var encoded = string.Concat(Fixture().Select(b => "\\u" + ((int)b).ToString("x4")));
                try { InventoryEvidence.Parse(Encoding.UTF8.GetBytes("{\"leak\":\"" + encoded + "\"}"), 0, Fixture()); }
                catch (ProducerException e) { Check(e.Failure == ProducerFailure.InvalidOutput); return Task.CompletedTask; }
                throw new InvalidOperationException();
            }),
            ("malformed-output-rejected", () =>
            {
                try { InventoryEvidence.Parse(Encoding.UTF8.GetBytes("not-json"), 0, Fixture()); }
                catch (ProducerException e) { Check(e.Failure == ProducerFailure.InvalidOutput); return Task.CompletedTask; }
                throw new InvalidOperationException();
            }),
            ("artifact-mismatch-before-any-child", async () => await Fails(ProducerFailure.ArtifactMismatch,
                () => CredentialProducer.RunInventoryAsync(Path.Combine(Path.GetTempPath(), "missing-enver-control-workspace"), Fixture()))),
            ("foreign-inheritable-handle-excluded", async () =>
            {
                using var foreign = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
                var receipt = await CredentialProducer.RunCoreAsync(Child("foreign", foreign.GetClientHandleAsString()), Fixture(), TimeSpan.FromSeconds(5), default);
                Check(receipt.ExitCode == 0);
            }),
            ("sibling-does-not-inherit-credential-handle", Sibling),
            ("cancel-before-spawn", async () =>
            {
                using var c = new CancellationTokenSource(); c.Cancel();
                await Fails(ProducerFailure.Cancelled, () => CredentialProducer.RunCoreAsync(Child("receive"), Fixture(), TimeSpan.FromSeconds(5), c.Token));
            }),
            ("cancel-running-child-kills-owned-job", async () =>
            {
                using var c = new CancellationTokenSource(350);
                await Fails(ProducerFailure.Cancelled, () => CredentialProducer.RunCoreAsync(Child("hang"), Fixture(), TimeSpan.FromSeconds(5), c.Token));
            }),
            ("deadline-kills-owned-child", async () => await Fails(ProducerFailure.Deadline,
                () => CredentialProducer.RunCoreAsync(Child("hang"), Fixture(), TimeSpan.FromMilliseconds(350), default))),
            ("child-crash-is-nonzero-and-sanitized", async () =>
            {
                try { var r = await CredentialProducer.RunCoreAsync(Child("crash"), Fixture(), TimeSpan.FromSeconds(5), default); Check(r.ExitCode == 91); }
                catch (ProducerException e) { Check(e.Failure == ProducerFailure.ChildFailure); }
            }),
            ("missing-executable-cleanup", async () => await Fails(ProducerFailure.ChildStart,
                () => CredentialProducer.RunCoreAsync(new(Path.Combine(Path.GetTempPath(), "no-enver-executable.exe"), Array.Empty<string>()), Fixture(), TimeSpan.FromSeconds(5), default))),
            ("short-input-rejected", async () => await Fails(ProducerFailure.InvalidInput, () => CredentialProducer.RunCoreAsync(Child("receive"), Fixture(15), TimeSpan.FromSeconds(5), default))),
            ("long-input-rejected", async () => await Fails(ProducerFailure.InvalidInput, () => CredentialProducer.RunCoreAsync(Child("receive"), Fixture(129), TimeSpan.FromSeconds(5), default))),
            ("invalid-character-rejected", async () => await Fails(ProducerFailure.InvalidInput, () => CredentialProducer.RunCoreAsync(Child("receive"), Enumerable.Repeat((byte)'@', 64).ToArray(), TimeSpan.FromSeconds(5), default))),
            ("unbounded-lifetime-rejected", async () => await Fails(ProducerFailure.InvalidInput, () => CredentialProducer.RunCoreAsync(Child("receive"), Fixture(), TimeSpan.FromMinutes(1), default))),
            ("secret-in-arguments-blocked-before-spawn", async () => await Fails(ProducerFailure.InvalidInput, () => CredentialProducer.RunCoreAsync(Child("receive", Encoding.ASCII.GetString(Fixture())), Fixture(), TimeSpan.FromSeconds(5), default))),
            ("nul-in-argument-rejected", () => { Throws(() => WindowsProcess.Quote("x\0y")); return Task.CompletedTask; }),
            ("quote-spaces-slashes-and-quotes", () => { Check(WindowsProcess.Quote("a b") == "\"a b\""); Check(WindowsProcess.Quote("a\\") == "\"a\\\\\""); Check(WindowsProcess.Quote("a\"b") == "\"a\\\"b\""); return Task.CompletedTask; }),
            ("caller-buffer-not-mutated", async () => { var b = Fixture(); await CredentialProducer.RunCoreAsync(Child("receive"), b, TimeSpan.FromSeconds(5), default); Check(b.SequenceEqual(Fixture())); CryptographicOperations.ZeroMemory(b); }),
            ("repeated-transfers-no-handle-leak", async () =>
            {
                using var process = Process.GetCurrentProcess(); var before = process.HandleCount;
                for (var i = 0; i < 6; i++) await Success("receive");
                Check(process.HandleCount <= before + 4);
            })
        };
        var failed = 0;
        foreach (var (name, run) in tests)
        {
            try { await run(); Console.WriteLine(JsonSerializer.Serialize(new { test = name, pass = true })); }
            catch { failed++; Console.WriteLine(JsonSerializer.Serialize(new { test = name, pass = false, error = "SANITIZED_TEST_FAILURE" })); }
        }
        Console.WriteLine(JsonSerializer.Serialize(new { suite = "quota-control-pipe-producer-synthetic", total = tests.Count, passed = tests.Count - failed, failed, network = false, realCredential = false }));
        return failed == 0 ? 0 : 1;
    }

    private static async Task<int> ChildMain(string[] args)
    {
        var mode = args[1];
        if (mode == "crash") return 91;
        if (mode == "hang") { await Task.Delay(Timeout.Infinite); return 92; }
        if (mode is "foreign" or "sibling")
        {
            var forbidden = new IntPtr(long.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture));
            return GetHandleInformation(forbidden, out _) ? 93 : 0;
        }
        var index = Array.IndexOf(args, "--credential-pipe");
        if (index < 0) return 94;
        var handle = args[index + 1];
        using var pipe = new AnonymousPipeClientStream(PipeDirection.In, handle);
        if (mode == "consumer")
        {
            using var c = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var credential = await PipeCredential.ReadAsync(pipe, c.Token);
            return credential.ToString() == "[REDACTED]" ? 0 : 95;
        }
        var prefix = new byte[4];
        await pipe.ReadExactlyAsync(prefix);
        var n = BinaryPrimitives.ReadUInt32BigEndian(prefix);
        if (n is < 16 or > 128) return 96;
        var data = new byte[n];
        try
        {
            await pipe.ReadExactlyAsync(data);
            var extra = new byte[1];
            if (await pipe.ReadAsync(extra) != 0 || !data.SequenceEqual(Fixture((int)n))) return 97;
            var value = Encoding.ASCII.GetString(data); // Solo este hijo sintético de tests; no productor operativo.
            if (Environment.CommandLine.Contains(value, StringComparison.Ordinal) ||
                Environment.GetEnvironmentVariables().Values.Cast<object>().Any(x => x.ToString()!.Contains(value, StringComparison.Ordinal))) return 98;
            if (mode == "echo") { Console.Out.Write(value); Console.Error.Write(value); }
            if (mode == "environment")
            {
                var allowed = new HashSet<string>(new[] { "SystemRoot", "WINDIR", "DOTNET_EnableDiagnostics", "DOTNET_CLI_TELEMETRY_OPTOUT" }, StringComparer.OrdinalIgnoreCase);
                if (Environment.GetEnvironmentVariables().Keys.Cast<string>().Any(key => !allowed.Contains(key))) return 89;
            }
            if (mode == "oversize") Console.Out.Write(new string('x', 65537));
            if (mode == "close")
            {
                var owned = pipe.SafePipeHandle.DangerousGetHandle(); pipe.Dispose();
                if (GetHandleInformation(owned, out _)) return 90;
            }
            if (mode == "report") Console.Out.Write(JsonSerializer.Serialize(new
            {
                schema = "enver.quota-control/2", mode = "inventory-remote", command = "inventory", exitCode = 0,
                logicalEndpoint = "master.enver-dev-valkey.kb2qe0.use1.cache.amazonaws.com", database = 0,
                operationallyApproved = false, admissionOpened = false, exclusionAccreditedByTool = false, sessionCoherent = true,
                connection = new { logicalEndpoint = "master.enver-dev-valkey.kb2qe0.use1.cache.amazonaws.com", resolvedTarget = "127.0.0.1:16379",
                    tlsProtocol = "Tls12", cipher = "TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384", topologyScope = "SINGLE_PINNED_TLS_CONNECTION_NOT_INDEPENDENT_PRIMARY_ATTESTATION" },
                execution = new { runId = "control-20261117T0000000000000Z-1", startedUtc = DateTimeOffset.UtcNow,
                    finishedUtc = DateTimeOffset.UtcNow, pages = 1, uniqueKeys = 0, duplicateKeys = 0,
                    initialCursor = "0", finalCursor = "0", completed = true, serverTimeMicroseconds = 1,
                    scanStartedUtc = DateTimeOffset.UtcNow, scanFinishedUtc = DateTimeOffset.UtcNow },
                inventory = new { metadataPresent = false, registryPresent = false, bucketCount = 0, discrepancies = 0,
                    registryMemberCount = 0, iterationCompleted = true }
            }));
            return 0;
        }
        finally { CryptographicOperations.ZeroMemory(data); CryptographicOperations.ZeroMemory(prefix); }
    }

    private static async Task Sibling()
    {
        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.None);
        using var inheritable = WindowsProcess.DuplicateForChild(pipe.ClientSafePipeHandle);
        var id = inheritable.DangerousGetHandle().ToInt64().ToString(System.Globalization.CultureInfo.InvariantCulture);
        using var child = WindowsProcess.Start(CredentialProducer.DotnetPath, new[] { Assembly, "child", "sibling", id }, Array.Empty<IntPtr>(), () => { });
        using var c = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        Check(await child.WaitAsync(c.Token) == 0);
    }
    private static async Task Success(string mode, int size = 64)
    {
        var password = Fixture(size);
        try { var r = await CredentialProducer.RunCoreAsync(Child(mode), password, TimeSpan.FromSeconds(5), default); Check(r.ExitCode == 0); }
        finally { CryptographicOperations.ZeroMemory(password); }
    }
    private static async Task Fails(ProducerFailure expected, Func<Task<ProducerReceipt>> action)
    {
        try { await action(); }
        catch (ProducerException e) { Check(e.Failure == expected && e.InnerException is null && e.Message == expected.ToString()); return; }
        throw new InvalidOperationException();
    }
    private static void Check(bool ok) { if (!ok) throw new InvalidOperationException(); }
    private static void Throws(Action action) { try { action(); } catch (ProducerException) { return; } throw new InvalidOperationException(); }
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetHandleInformation(IntPtr handle, out uint flags);
}
