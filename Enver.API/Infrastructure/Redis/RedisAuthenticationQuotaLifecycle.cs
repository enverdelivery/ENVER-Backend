using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using StackExchange.Redis;

namespace Enver.API.Infrastructure.Redis;

/// <summary>
/// Control explícito aislable, no invocado por el store ni por reconnect. No reabre la autoridad 020.
/// Inicialización exclusivamente para un ambiente nuevo cuya ausencia de historia acredita el operador;
/// Redis vacío NO acredita esa precondición. Sin integración ni autorización productiva implícita.
/// </summary>
public sealed class RedisAuthenticationQuotaLifecycle
{
    private readonly RedisConnections connections;
    private readonly RedisOptions options;
    private readonly RedisAuthenticationQuotaConfiguration configuration;
    private readonly IAuthenticationSecurityState local;
    private readonly IAuthenticationSecurityState global;
    private readonly Func<DependencyBudget, RedisAuthenticationQuotaInvocation, ValueTask<RedisResult>>? execute;

    public RedisAuthenticationQuotaLifecycle(RedisConnections connections, RedisOptions options, RedisAuthenticationQuotaConfiguration configuration,
        IAuthenticationSecurityState localSecurityState, IAuthenticationSecurityState globalSecurityState,
        Func<DependencyBudget, RedisAuthenticationQuotaInvocation, ValueTask<RedisResult>>? executeOverride = null)
    {
        this.connections = connections ?? throw new ArgumentNullException(nameof(connections));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.OperationTimeoutMilliseconds);
        this.options = new() { OperationTimeoutMilliseconds = options.OperationTimeoutMilliseconds };
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        local = localSecurityState ?? throw new ArgumentNullException(nameof(localSecurityState));
        global = globalSecurityState ?? throw new ArgumentNullException(nameof(globalSecurityState));
        execute = executeOverride;
    }

    public ValueTask<RedisAuthenticationQuotaReply> InitializeVerifiedNewEnvironmentAsync(RequestBudget budget) => RunAsync("initialize", budget);
    public ValueTask<RedisAuthenticationQuotaReply> FenceAsync(string nextGeneration, RequestBudget budget) => RunAsync("fence", budget, nextGeneration: nextGeneration);
    public ValueTask<RedisAuthenticationQuotaReply> ReconcileBatchAsync(IEnumerable<string> candidates, RequestBudget budget) => RunAsync("maintain", budget, candidates);
    public async ValueTask<RedisAuthenticationQuotaReply> ActivateAfterDrainAsync(RequestBudget budget)
    {
        // Inventario de CONTROL cerrado: también localiza buckets sin registro/TTL tras fallo parcial.
        // SCAN no participa en requests ni decide admisión; cada lote se revalida en el Lua (máximo 3).
        // El seam unitario sustituye únicamente E/S; no es evidencia de este inventario real.
        if (execute is null)
        {
            try
            {
                var check = await RunAsync("maintain", budget).ConfigureAwait(false);
                if (check.Status != "maintained") return check;
                using var dependency = budget.BeginDependency(options.OperationTimeoutMilliseconds);
                var multiplexer = await connections.GetMultiplexerAsync(dependency.Token).ConfigureAwait(false);
                var database = multiplexer.GetDatabase();
                var releasedCandidates = 0;
                while (true)
                {
                    var candidates = await database.SortedSetRangeByRankAsync(configuration.Keys.Registry, 0, 2,
                        Order.Ascending, RedisAuthenticationQuotaScriptProtocol.Flags).WaitAsync(dependency.Token).ConfigureAwait(false);
                    if (candidates.Length == 0) break;
                    releasedCandidates += candidates.Length;
                    if (releasedCandidates > 5000) return new("unavailable", "state", 0, 0, true);
                    var cleaned = await ReconcileBatchAsync(candidates.Select(v => v.ToString()), budget).ConfigureAwait(false);
                    if (cleaned.Status != "maintained") return cleaned;
                }
                var seen = new HashSet<string>(StringComparer.Ordinal);
                var batch = new List<string>(3);
                var inspectedPrimary = false;
                foreach (var endpoint in multiplexer.GetEndPoints())
                {
                    var server = multiplexer.GetServer(endpoint);
                    if (server.IsReplica) continue;
                    inspectedPrimary = true;
                    await foreach (var key in server.KeysAsync(database.Database, configuration.Keys.Prefix + "bucket:*", 3,
                        flags: RedisAuthenticationQuotaScriptProtocol.Flags).WithCancellation(dependency.Token).ConfigureAwait(false))
                    {
                        dependency.ThrowIfCancellationRequested();
                        var text = key.ToString();
                        if (!seen.Add(text)) continue;
                        if (seen.Count > 5000 || !text.StartsWith(configuration.Keys.Prefix + "bucket:", StringComparison.Ordinal))
                            return new("unavailable", "state", 0, 0, true);
                        var member = text[(configuration.Keys.Prefix.Length + "bucket:".Length)..];
                        if (configuration.Keys.CandidateBucket(member) != text) return new("unavailable", "state", 0, 0, true);
                        batch.Add(member);
                        if (batch.Count == 3)
                        {
                            var result = await ReconcileBatchAsync(batch, budget).ConfigureAwait(false);
                            if (result.Status != "maintained") return result;
                            batch.Clear();
                        }
                    }
                }
                if (batch.Count > 0)
                {
                    var result = await ReconcileBatchAsync(batch, budget).ConfigureAwait(false);
                    if (result.Status != "maintained") return result;
                }
                if (!inspectedPrimary) return new("unavailable", "state", 0, 0, true);
            }
            catch { return new("unavailable", "state", 0, 0, true, budget.Cause); }
        }
        return await RunAsync("activate", budget).ConfigureAwait(false);
    }

    private async ValueTask<RedisAuthenticationQuotaReply> RunAsync(string mode, RequestBudget budget,
        IEnumerable<string>? candidates = null, string? nextGeneration = null)
    {
        try
        {
            budget.ThrowIfCancellationRequested();
            var invocation = RedisAuthenticationQuotaScriptProtocol.Encode(configuration, mode, candidates: candidates, nextGeneration: nextGeneration);
            if (mode is "initialize" or "activate")
            {
                var failure = await RedisAuthenticationQuotaScriptProtocol.CheckSecurityAsync(local, global, budget).ConfigureAwait(false);
                if (failure is not null) return new(failure.ResultUncertain ? "uncertain" : "unavailable", "closed", 0, 0, !failure.ResultUncertain, failure.CancellationCause);
            }
            return await RedisAuthenticationQuotaScriptProtocol.ExecuteAsync(connections, options, invocation, budget, execute).ConfigureAwait(false);
        }
        catch { return new("unavailable", "invalid", 0, 0, true, budget.Cause); }
    }
    public override string ToString() => nameof(RedisAuthenticationQuotaLifecycle);
}
