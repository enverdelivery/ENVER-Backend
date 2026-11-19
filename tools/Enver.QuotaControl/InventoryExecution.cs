using System.Diagnostics;
using System.Text.Json;

namespace Enver.QuotaControl;

public sealed class InventoryTrace
{
    private static long sequence;
    private readonly long clock = Stopwatch.GetTimestamp();
    public string RunId { get; } = "control-" + DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssfffffffZ") + "-" + Interlocked.Increment(ref sequence);
    public DateTimeOffset StartedUtc { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedUtc { get; private set; }
    public DateTimeOffset? ScanStartedUtc { get; internal set; }
    public DateTimeOffset? ScanFinishedUtc { get; internal set; }
    public string InitialCursor => "0";
    public string? FinalCursor { get; internal set; }
    public int Pages { get; internal set; }
    public int UniqueKeys { get; internal set; }
    public int DuplicateKeys { get; internal set; }
    public bool Completed { get; internal set; }
    public long? ServerTimeMicroseconds { get; internal set; }
    public double DurationMilliseconds { get; private set; }
    public void Finish()
    {
        FinishedUtc = DateTimeOffset.UtcNow;
        DurationMilliseconds = Stopwatch.GetElapsedTime(clock).TotalMilliseconds;
    }
}

public static class InventoryExit
{
    public const int Pass = 0, Residue = 10, Incomplete = 20, AuthTls = 30, Protocol = 40, Policy = 50, Credential = 60, Local = 70;
    public static int For(Failure failure) => failure switch
    {
        Failure.ResidueFound or Failure.MetadataExists or Failure.RegistryExists => Residue,
        Failure.InventoryIncomplete or Failure.RemoteTransportDisabled or Failure.ResultUncertain => Incomplete,
        Failure.AuthTlsFailure => AuthTls,
        Failure.UnexpectedResponse or Failure.ProtocolFailure or Failure.InvalidTime => Protocol,
        Failure.PolicyFailure or Failure.InvalidArguments or Failure.InvalidDescriptor or Failure.InvalidGeneration or Failure.ApplyDisabled => Policy,
        Failure.CredentialFailure => Credential,
        _ => Local
    };

    public static int Evaluate(InventoryResult result, InventoryTrace trace, bool sessionCoherent)
    {
        if (!trace.Completed || trace.FinalCursor != "0" || trace.Pages < 1 || trace.DuplicateKeys != 0 || !sessionCoherent)
            return Incomplete;
        if (result.Metadata.Type != "none" || result.Registry.Type != "none" || result.BucketCount != 0) return Residue;
        return result.IsEmpty ? Pass : Incomplete;
    }
}

public static class InventoryExecution
{
    public static async Task<int> RunAsync(RemoteOptions options, ApprovedDescriptor descriptor, Stream credentialPipe,
        IRemoteSessionFactory factory, TextWriter output, CancellationToken token = default)
    {
        var trace = new InventoryTrace();
        ConnectionEvidence? connection = null;
        InventorySummary? summary = null;
        Failure? failure = null;
        var exit = InventoryExit.Incomplete;
        var coherent = false;
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(token);
        budget.CancelAfter(RemoteOptions.OperationTimeout);
        try
        {
            descriptor.Validate();
            using var credential = await PipeCredential.ReadAsync(credentialPipe, budget.Token).WaitAsync(budget.Token).ConfigureAwait(false);
            // OpenAsync es inyectado. Los tests usan solo MemoryStream, jamas TlsSessionFactory.
            await using var session = await factory.OpenAsync(options, budget.Token).WaitAsync(budget.Token).ConfigureAwait(false);
            connection = session.Evidence;
            var client = new RemoteTransport(session);
            await client.AuthenticateAsync(credential, budget.Token).ConfigureAwait(false);
            credential.Dispose();
            var result = await new InventoryReader(client, trace).ReadAsync(budget.Token).ConfigureAwait(false);
            coherent = client.IsCoherent;
            summary = result.Sanitize();
            exit = InventoryExit.Evaluate(result, trace, coherent);
        }
        catch (ControlException exception) { failure = exception.Failure; exit = InventoryExit.For(exception.Failure); }
        catch (OperationCanceledException) { failure = Failure.InventoryIncomplete; exit = InventoryExit.Incomplete; }
        catch { failure = Failure.LocalOperationFailed; exit = InventoryExit.Local; }
        trace.Finish();
        await WriteAsync(output, new
        {
            schema = "enver.quota-control/2", mode = "inventory-remote", command = "inventory", exitCode = exit,
            logicalEndpoint = RemoteOptions.Endpoint, logicalPort = 6379, database = 0,
            route = options.Route.ToString(), controlUser = RemoteOptions.ControlUser,
            status = exit switch { 0 => "PASS", 10 => "RESIDUE_FOUND", 20 => "INCOMPLETE", 30 => "AUTH_TLS_FAILURE",
                40 => "PROTOCOL_FAILURE", 50 => "POLICY_ALLOWLIST_FAILURE", 60 => "CREDENTIAL_INPUT_FAILURE", _ => "LOCAL_FAILURE" },
            conclusion = exit == 0 ? "CURRENT INVENTORY EMPTY" : null,
            exclusionAccreditedByTool = false, operationallyApproved = false, admissionOpened = false,
            code = failure?.ToString(), connection, sessionCoherent = coherent, execution = trace, inventory = summary
        }).ConfigureAwait(false);
        return exit;
    }

    internal static Task WriteAsync(TextWriter output, object value) => output.WriteLineAsync(JsonSerializer.Serialize(value,
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
}
