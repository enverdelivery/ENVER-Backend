namespace Enver.QuotaControl.Bootstrap;

internal sealed record BootstrapRequest(
    string Endpoint,
    string Generation,
    string AuthorityTable,
    string Region,
    int TimeoutSeconds);

internal enum BootstrapOutcome
{
    Passed,
    Stopped
}

internal sealed record BootstrapResult(BootstrapOutcome Outcome, string Code, IReadOnlyList<string> Detail)
{
    public static BootstrapResult Passed() => new(BootstrapOutcome.Passed, "passed", []);
    public static BootstrapResult Stopped(string code, params string[] detail) => new(BootstrapOutcome.Stopped, code, detail);
}

internal sealed record PreflightResult(
    bool ProbeSucceeded,
    string MetadataType,
    string RegistryType,
    bool LocalReady,
    bool GlobalReady,
    long KeyspaceTotal = 0)
{
    public static PreflightResult Unavailable() => new(false, "unknown", "unknown", false, false);
}

internal sealed record InitializeOutcome(string Status, string Category, bool IsSemantic);

internal sealed record ReadbackResult(bool IsMatch, IReadOnlyList<string> Mismatches)
{
    public static ReadbackResult Success() => new(true, []);
    public static ReadbackResult Failure(params string[] mismatches) => new(false, mismatches);
}

internal interface IBootstrapBackend : IAsyncDisposable
{
    ValueTask<PreflightResult> PreflightAsync(CancellationToken cancellationToken);
    ValueTask<InitializeOutcome> InitializeAsync(CancellationToken cancellationToken);
    ValueTask<ReadbackResult> ReadbackAsync(CancellationToken cancellationToken);
}

internal sealed class BootstrapService
{
    public async ValueTask<BootstrapResult> ExecuteAsync(
        BootstrapRequest request,
        IBootstrapBackend backend,
        CancellationToken cancellationToken)
    {
        BootstrapContract.ValidateGeneration(request.Generation);

        var preflight = await backend.PreflightAsync(cancellationToken).ConfigureAwait(false);
        if (!preflight.ProbeSucceeded) return BootstrapResult.Stopped("preflight-unavailable");
        if (!preflight.LocalReady) return BootstrapResult.Stopped("guard-closed");
        if (!preflight.GlobalReady) return BootstrapResult.Stopped("authority-closed");
        if (!string.Equals(preflight.MetadataType, "none", StringComparison.Ordinal))
        {
            return BootstrapResult.Stopped("metadata-present");
        }

        if (!string.Equals(preflight.RegistryType, "none", StringComparison.Ordinal))
        {
            return BootstrapResult.Stopped("registry-present");
        }

        if (preflight.KeyspaceTotal != 0)
        {
            return BootstrapResult.Stopped("keyspace-not-empty");
        }

        var initialized = await backend.InitializeAsync(cancellationToken).ConfigureAwait(false);
        if (string.Equals(initialized.Status, "uncertain", StringComparison.Ordinal))
        {
            return BootstrapResult.Stopped("initialize-uncertain");
        }

        if (!string.Equals(initialized.Status, "initialized", StringComparison.Ordinal) ||
            !string.Equals(initialized.Category, "ok", StringComparison.Ordinal) ||
            !initialized.IsSemantic)
        {
            return BootstrapResult.Stopped("initialize-failed");
        }

        var readback = await backend.ReadbackAsync(cancellationToken).ConfigureAwait(false);
        if (!readback.IsMatch)
        {
            return BootstrapResult.Stopped("readback-mismatch", readback.Mismatches.ToArray());
        }

        return BootstrapResult.Passed();
    }
}
