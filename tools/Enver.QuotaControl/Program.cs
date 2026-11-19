using System.Text.Json;
using System.IO.Pipes;
using System.Globalization;

namespace Enver.QuotaControl;

public sealed record CommandLine(string Command, string DescriptorPath, string? Generation, long? ServerTime,
    RemoteOptions? Remote = null, string? CredentialPipeHandle = null)
{
    public static CommandLine Parse(string[] args)
    {
        if (args.Length == 0 || args[0] is not ("precheck" or "inventory" or "plan-closed" or "apply-closed" or "readback"))
            throw new ControlException(Failure.InvalidArguments);
        var options = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 1; i < args.Length; i += 2)
        {
            if (i + 1 >= args.Length || args[i] is not ("--descriptor" or "--generation" or "--server-time-us" or
                "--transport" or "--tunnel-port" or "--credential-pipe") ||
                args[i + 1].StartsWith("--", StringComparison.Ordinal) || !options.TryAdd(args[i], args[i + 1]) || args[i + 1].Length > 4096)
                throw new ControlException(Failure.InvalidArguments);
        }
        if (!options.TryGetValue("--descriptor", out var descriptor) || string.IsNullOrWhiteSpace(descriptor))
            throw new ControlException(Failure.InvalidArguments);
        options.TryGetValue("--generation", out var generation);
        var needsGeneration = args[0] is "plan-closed" or "apply-closed" or "readback";
        if (needsGeneration) Contract.ValidateGeneration(generation);
        else if (generation is not null) throw new ControlException(Failure.InvalidArguments);
        long? time = null;
        if (options.TryGetValue("--server-time-us", out var suppliedTime))
        {
            if (args[0] != "plan-closed") throw new ControlException(Failure.InvalidArguments);
            time = Contract.ParseUnsigned(suppliedTime);
            Contract.ValidateTime(time.Value);
        }
        if (args[0] == "plan-closed" && time is null) throw new ControlException(Failure.InvalidArguments);
        RemoteOptions? remote = null;
        options.TryGetValue("--credential-pipe", out var pipe);
        options.TryGetValue("--transport", out var transport);
        options.TryGetValue("--tunnel-port", out var port);
        if (transport is not null || pipe is not null || port is not null)
        {
            if (args[0] != "inventory" || transport is not ("direct" or "ssm-tunnel") || pipe is null ||
                !long.TryParse(pipe, NumberStyles.None, CultureInfo.InvariantCulture, out var handle) || handle <= 0 ||
                handle.ToString(CultureInfo.InvariantCulture) != pipe)
                throw new ControlException(Failure.PolicyFailure);
            int? tunnelPort = null;
            if (port is not null)
            {
                if (!int.TryParse(port, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
                    throw new ControlException(Failure.PolicyFailure);
                tunnelPort = parsed;
            }
            remote = new RemoteOptions(transport == "direct" ? ConnectionRoute.Direct : ConnectionRoute.SsmTunnel, tunnelPort);
        }
        return new(args[0], descriptor, generation, time, remote, pipe);
    }
}

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var cancel = new CancellationTokenSource();
        ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; cancel.Cancel(); };
        Console.CancelKeyPress += handler;
        try { return await RunAsync(args, Console.Out, cancel.Token).ConfigureAwait(false); }
        finally { Console.CancelKeyPress -= handler; }
    }

    public static async Task<int> RunAsync(string[] args, TextWriter output, CancellationToken token = default)
    {
        try
        {
            var command = CommandLine.Parse(args);
            // Solo descriptor no secreto, estricto; nunca configuracion generica/ENV/connection strings.
            var file = new FileInfo(command.DescriptorPath);
            if (!file.Exists || file.Length > 16384) throw new ControlException(Failure.InvalidDescriptor);
            var descriptor = ApprovedDescriptor.ParseJson(await File.ReadAllTextAsync(file.FullName).ConfigureAwait(false));
            if (command.Command == "apply-closed") throw new ControlException(Failure.ApplyDisabled);
            if (command.Command == "inventory" && command.Remote is not null)
            {
                // Handle no secreto de un pipe anonimo heredado; no stdin, archivos o configuracion ENV.
                AnonymousPipeClientStream pipe;
                try { pipe = new AnonymousPipeClientStream(PipeDirection.In, command.CredentialPipeHandle!); }
                catch { throw new ControlException(Failure.CredentialFailure); }
                await using (pipe.ConfigureAwait(false))
                    return await InventoryExecution.RunAsync(command.Remote, descriptor, pipe, new TlsSessionFactory(), output, token).ConfigureAwait(false);
            }
            if (command.Command == "plan-closed")
            {
                var plan = ClosedPlan.Build(descriptor, command.Generation!, command.ServerTime!.Value);
                await Write(output, new { schema = "enver.quota-control/1", mode = "local-only", command = command.Command,
                    status = "planned_not_applied", baseGenerationStatus = "UNKNOWN", operationallyApproved = false,
                    serverTimeVerified = false, admissionOpened = false, fields = plan.Fields }).ConfigureAwait(false);
                return 0;
            }
            var service = new ControlService(new DisabledRedisClient());
            using var budget = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            object result = command.Command switch
            {
                "inventory" => (await service.InventoryAsync(descriptor, budget.Token).ConfigureAwait(false)).Sanitize(),
                "precheck" => (await service.PrecheckAsync(descriptor, budget.Token).ConfigureAwait(false)).Sanitize(),
                "readback" => (await service.ReadbackAsync(descriptor, command.Generation!, budget.Token).ConfigureAwait(false)).Fields,
                _ => throw new ControlException(Failure.InvalidArguments)
            };
            await Write(output, new { schema = "enver.quota-control/1", mode = "local-only", command = command.Command,
                status = "validated", operationallyApproved = false, admissionOpened = false, result }).ConfigureAwait(false);
            return 0;
        }
        catch (ControlException exception)
        {
            await Write(output, new { schema = "enver.quota-control/1", mode = "local-only", status = "blocked",
                code = exception.Failure.ToString(), admissionOpened = false }).ConfigureAwait(false);
            return args.Length > 0 && args[0] == "inventory" ? InventoryExit.For(exception.Failure)
                : exception.Failure == Failure.ResultUncertain ? 3 : 2;
        }
        catch
        {
            await Write(output, new { schema = "enver.quota-control/1", mode = "local-only", status = "blocked",
                code = "LocalOperationFailed", admissionOpened = false }).ConfigureAwait(false);
            return args.Length > 0 && args[0] == "inventory" ? InventoryExit.Local : 2;
        }
    }

    private static Task Write(TextWriter output, object value) => output.WriteLineAsync(JsonSerializer.Serialize(value));
}
