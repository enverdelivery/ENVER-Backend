namespace Enver.RuntimeConfig.Materializer;

internal interface IDeliveryRuntime : IDisposable
{
    IRuntimeFileStore Files { get; }
    Task<ISecretReader> ResolveCredentials(CancellationToken cancellationToken);
    void ValidateRuntime(DeliveryTrace trace);
}

internal sealed class ProductionDeliveryRuntime : IDeliveryRuntime
{
    private Amazon.SecretsManager.AmazonSecretsManagerClient? client;
    public IRuntimeFileStore Files { get; } = new LinuxRuntimeFileStore();
    public async Task<ISecretReader> ResolveCredentials(CancellationToken cancellationToken)
    {
        await InstanceIdentity.Require(cancellationToken);
        client = await InstanceIdentity.CreateClient(cancellationToken);
        return new PinnedSecretReader(client);
    }
    public void ValidateRuntime(DeliveryTrace trace) => BindingDiagnostic.FileBinding(AtomicRuntimeFile.Path, ValidationTarget.FinalFile, trace);
    public void Dispose() => client?.Dispose();
}

internal static class Program
{
    internal static Task<int> Main(string[] args)
    {
        return WithBudget(args);
    }

    private static async Task<int> WithBudget(string[] args)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        return await Run(args, new ProductionDeliveryRuntime(), Console.Out, new DeliveryTrace(), timeout.Token);
    }

    // Tests inject in-memory dependencies only here, never via argv/environment or a production switch.
    internal static async Task<int> Run(string[] args, IDeliveryRuntime runtime, TextWriter output,
        DeliveryTrace trace, CancellationToken cancellationToken = default)
    {
        try
        {
            using var lifetime = runtime;
            trace.Enter(DeliveryPhase.Arguments);
            if (args.Length != 1 || args[0] is not ("--materialize" or "--validate-runtime"))
                throw new DeliveryFailure("usage-invalid");
            if (args[0] == "--validate-runtime")
            {
                trace.EnterValidation(DeliveryPhase.FinalConfigParseValidation, ValidationTarget.FinalFile);
                runtime.ValidateRuntime(trace);
                output.WriteLine("{\"status\":\"PASS\",\"binding\":true,\"jsonParse\":true,\"requiredSections\":true,\"externalConnections\":false,\"apiStarted\":false}");
            }
            else
            {
                trace.Enter(DeliveryPhase.TmpfsCheck);
                runtime.Files.Prepare();
                trace.Enter(DeliveryPhase.AwsCredentials);
                var reader = await runtime.ResolveCredentials(cancellationToken);
                var config = await MapAndValidate(reader, cancellationToken, trace);
                try { AtomicRuntimeFile.Write(config, trace, runtime.Files); }
                finally { config.Clear(); }
                output.WriteLine("{\"status\":\"PASS\",\"instanceRole\":true,\"pins\":true,\"redisMapping\":true,\"mongoMapping\":true,\"hmacMapping\":true,\"jwtRingMapping\":true,\"nonsecretConfig\":true,\"binding\":true,\"atomicWrite\":true,\"permissions\":\"0600\",\"owner\":\"1654:1654\",\"apiStarted\":false}");
            }
            return 0;
        }
        catch (Exception error)
        {
            trace.WriteFailure(output, error, cancellationToken);
            return trace.Current == DeliveryPhase.Arguments ? 2 : 1;
        }
    }

    internal static async Task<System.Text.Json.Nodes.JsonObject> MapAndValidate(ISecretReader reader, CancellationToken cancellationToken, DeliveryTrace? trace = null)
    {
        trace ??= new();
        var payloads = new Dictionary<string, string>();
        try
        {
            foreach (var pin in Pins.All)
            {
                trace.Enter(pin.Name switch
                {
                    "redis" => DeliveryPhase.RedisSecretRead,
                    "mongodb" => DeliveryPhase.MongodbSecretRead,
                    "hmac" => DeliveryPhase.HmacSecretRead,
                    "jwt" => DeliveryPhase.JwtSecretRead,
                    _ => throw new DeliveryFailure("schema-invalid")
                });
                payloads.Add(pin.Name, await reader.Read(pin, cancellationToken));
            }
            var runtime = RuntimeMapping.Map(payloads, trace);
            BindingDiagnostic.InMemory(runtime, trace);
            return runtime;
        }
        catch (Exception error) { throw DeliveryFailure.Wrap(error); }
        finally { payloads.Clear(); } // Managed strings cannot be guaranteed zeroized; no persistence/logging.
    }

}
