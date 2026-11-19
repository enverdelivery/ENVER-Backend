namespace Enver.QuotaControl.Bootstrap;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var parsed = BootstrapCommandLine.Parse(args);
        if (!parsed.Success)
        {
            Console.Error.WriteLine("BOOTSTRAP ABORTED: " + parsed.Error);
            return 3;
        }

        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        try
        {
            return await RunValidatedAsync(parsed.Arguments!,
                new StdinSecretSource(Console.In, Console.IsInputRedirected),
                static (arguments, credential) => new RedisBootstrapBackend(arguments, credential),
                Console.Out, cancellation.Token).ConfigureAwait(false);
        }
        catch (BootstrapValidationException exception)
        {
            Console.Error.WriteLine("BOOTSTRAP ABORTED: " + exception.Code);
            return 4;
        }
        catch
        {
            // Nunca propagar mensajes de excepción que pudieran contener credenciales.
            Console.Error.WriteLine("BOOTSTRAP ABORTED: internal-error");
            return 5;
        }
    }

    // Punto de prueba del mismo despacho usado por Main; producción conserva la composición real anterior.
    internal static async Task<int> RunValidatedAsync(
        BootstrapArguments arguments,
        ISecretSource secretSource,
        Func<BootstrapArguments, RedisCredential, IBootstrapBackend> createBackend,
        TextWriter output,
        CancellationToken cancellationToken)
    {
        var credential = await secretSource.ReadAsync(cancellationToken).ConfigureAwait(false);
        await using var backend = createBackend(arguments, credential);
        if (arguments.PreflightOnly)
        {
            // Retorno terminal ANTES del flujo operacional. El runner no recibe acceso a InitializeAsync.
            return await PreflightOnlyRunner.RunAsync(backend.PreflightAsync, output, cancellationToken).ConfigureAwait(false);
        }

        var result = await new BootstrapService()
            .ExecuteAsync(arguments.ToRequest(), backend, cancellationToken)
            .ConfigureAwait(false);
        BootstrapReporter.Write(output, result);
        return result.Outcome == BootstrapOutcome.Passed ? 0 : 2;
    }
}

/// <summary>Solo lectura: un preflight, sin servicio operacional, initialize, readback ni retry.</summary>
internal static class PreflightOnlyRunner
{
    /// <summary>0: completado y admisible; 2: no disponible/no admisible. Main conserva 3/4/5.</summary>
    internal static async Task<int> RunAsync(
        Func<CancellationToken, ValueTask<PreflightResult>> preflight,
        TextWriter output,
        CancellationToken cancellationToken)
    {
        var result = await preflight(cancellationToken).ConfigureAwait(false);
        output.WriteLine(result.ProbeSucceeded ? "PREFLIGHT-ONLY RESULT: AVAILABLE" : "PREFLIGHT-ONLY RESULT: UNAVAILABLE");
        // Los tipos se proyectan a una lista cerrada; nunca se refleja un payload arbitrario en stdout.
        output.WriteLine($"ProbeSucceeded={result.ProbeSucceeded} LocalReady={result.LocalReady} GlobalReady={result.GlobalReady} " +
            $"MetadataType={SafeType(result.MetadataType)} RegistryType={SafeType(result.RegistryType)} " +
            $"KeyspaceTotal={result.KeyspaceTotal.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        var admissible = result.ProbeSucceeded && result.LocalReady && result.GlobalReady &&
            result.MetadataType == "none" && result.RegistryType == "none" && result.KeyspaceTotal == 0;
        return admissible ? 0 : 2;
    }

    private static string SafeType(string type) => type switch
    {
        "none" => "none", "hash" => "hash", "string" => "string", "list" => "list",
        "set" => "set", "zset" => "zset", "stream" => "stream", "vectorset" => "vectorset",
        _ => "unknown"
    };
}
