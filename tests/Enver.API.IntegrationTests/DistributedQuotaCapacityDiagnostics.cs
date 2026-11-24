using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Infrastructure.Redis;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

// Solo capacity. No EVAL alternativo, hints adicionales, SCAN, escrituras ni cambios de cleanup.
// Solo los tres EVAL de reasignación reciben sondeos de lectura en pipeline antes/después.
// EVAL conserva script, KEYS, ARGV, flags y respuesta. Las sondas pueden activar expiración
// diferida y perturbar timing; no acreditan reparación ni son un snapshot atómico.
internal sealed class DistributedQuotaCapacityDiagnostics
{
    private readonly string partition;
    private readonly List<object> rounds = new(3);
    private object? startState, beforeWait;
    private RedisResult? response;
    private int round, generatedRequests, seedRequests, hintCount;
    private long accountedBefore, missing, horizon, accounted;
    private object? eval;
    private long? redisTimeAtWaitCrossing;
    private Task<JsonElement>? transition;
    internal bool CaptureEval { get; private set; }

    internal DistributedQuotaCapacityDiagnostics(string metadataKey) =>
        partition = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(metadataKey)))[..16];

    internal async Task StartedAsync(DistributedQuotaTestScope scope) =>
        startState = await SnapshotAsync(scope, 0);

    internal void CreatedRequest() => Interlocked.Increment(ref generatedRequests);
    internal void BeginRound(int value)
    {
        round = value; response = null; eval = null; beforeWait = null;
        accounted = -1; CaptureEval = false;
        transition = null; redisTimeAtWaitCrossing = null;
    }
    internal void Prepared(long before, long missingCount, int seededRequests)
    { accountedBefore = before; missing = missingCount; seedRequests = seededRequests; }

    internal async Task BeforeWaitAsync(DistributedQuotaTestScope scope, long value)
    { horizon = value; beforeWait = await SnapshotAsync(scope, value); }

    internal void Arm() => CaptureEval = true;
    internal void WaitCrossed(long value) => redisTimeAtWaitCrossing = value;

    internal async Task<RedisResult> ExecuteAsync(IDatabase database,
        RedisAuthenticationQuotaConfiguration config, RedisAuthenticationQuotaInvocation invocation)
    {
        var requested = (int)invocation.Arguments[13];
        var prefix = config.Keys.Prefix + "bucket:";
        // Solo posiciones de KEYS salen en evidencia. Los members permanecen privados.
        var members = invocation.Keys.Skip(2).Select(key =>
        {
            var text = key.ToString();
            if (!text.StartsWith(prefix, StringComparison.Ordinal))
                throw new InvalidOperationException("Key fuera del namespace de diagnóstico capacity.");
            return (RedisValue)text[prefix.Length..];
        }).ToArray();
        var batch = database.CreateBatch();
        var before = QueueState(batch, config, members, horizon);
        var pending = batch.ScriptEvaluateAsync(RedisAuthenticationQuotaScriptProtocol.ScriptText,
            invocation.Keys, invocation.Arguments, RedisAuthenticationQuotaScriptProtocol.Flags);
        var after = QueueState(batch, config, members, horizon);
        batch.Execute();
        transition = CompleteTransitionAsync(before, after, pending, requested);
        // No esperar las sondas post-EVAL dentro del presupuesto funcional del adapter.
        // Su fallo no reemplaza la respuesta original ni se convierte en resultado quota.
        response = await pending;
        hintCount = members.Length - requested;
        return response;
    }

    private sealed record State(long RedisBeforeUs, long RedisAfterUs, long? Count, long? Horizon,
        long? Version, long? Dirty, long Cardinality, long ScoresThroughHorizon,
        long PExpireTime, long Pttl, double?[] MemberScores);

    private static Task<State> QueueState(IBatch batch, RedisAuthenticationQuotaConfiguration config,
        RedisValue[] members, long horizon)
    {
        var begin = batch.ExecuteAsync("TIME");
        var metadata = batch.HashGetAsync(config.Keys.Metadata, ["count", "horizon", "version", "dirty"]);
        var cardinality = batch.SortedSetLengthAsync(config.Keys.Registry);
        var residues = batch.SortedSetLengthAsync(config.Keys.Registry, double.NegativeInfinity, horizon);
        var expiry = batch.ExecuteAsync("PEXPIRETIME", config.Keys.Registry);
        var ttl = batch.ExecuteAsync("PTTL", config.Keys.Registry);
        var scores = Task.WhenAll(members.Select(member => batch.SortedSetScoreAsync(config.Keys.Registry, member)));
        var end = batch.ExecuteAsync("TIME");
        return ReadAsync();

        async Task<State> ReadAsync()
        {
            await Task.WhenAll(begin, metadata, cardinality, residues, expiry, ttl, scores, end);
            var values = await metadata;
            long? Number(int i) => long.TryParse(values[i].ToString(), out var value) ? value : null;
            long Time(RedisResult result)
            { var parts = (RedisResult[])result!; return checked((long)parts[0] * 1000000 + (long)parts[1]); }
            return new(Time(await begin), Time(await end), Number(0), Number(1), Number(2), Number(3),
                await cardinality, await residues, (long)await expiry, (long)await ttl, await scores);
        }
    }

    private static async Task<JsonElement> CompleteTransitionAsync(Task<State> before,
        Task<State> after, Task<RedisResult> pending, int requested)
    {
        try
        {
            await Task.WhenAll(before, after, pending);
            var pre = await before; var post = await after;
            var reply = RedisAuthenticationQuotaScriptProtocol.Parse(await pending);
            var hints = Enumerable.Range(requested, pre.MemberScores.Length - requested).ToArray();
            var newMembers = Enumerable.Range(0, requested).Count(i => pre.MemberScores[i] is null && post.MemberScores[i] is not null);
            var disappearedHints = hints.Count(i => pre.MemberScores[i] is not null && post.MemberScores[i] is null);
            return JsonSerializer.SerializeToElement(new { nonAtomic = true,
                expiryReadsMayTriggerLazyExpiration = true, before = pre, after = post,
                requestedScopes = requested, newRegistryMembersObserved = newMembers,
                staleHintMembersObserved = hints.Count(i => pre.MemberScores[i] is { } score && score <= reply.NowMicroseconds),
                hintMembersDisappeared = disappearedHints,
                removalAttribution = "MEMBERSHIP_DELTA_NOT_INTERNAL_COMMAND_TRACE",
                luaDecision = reply.Status, luaNowUs = reply.NowMicroseconds,
                admittedRequests = reply.Status == "admitted" ? 1 : 0,
                rejectedRequests = reply.Status == "rejected" ? 1 : 0,
                unavailableRequests = reply.Status == "unavailable" ? 1 : 0 });
        }
        catch { return JsonSerializer.SerializeToElement(new { unavailable = true }); }
    }

    internal async Task<JsonElement?> TransitionEvidenceAsync() => transition is null ? null : await transition;
    internal void Decision(AuthenticationQuotaConsumeResult result)
    {
        if (!CaptureEval) return;
        CaptureEval = false;
        var reply = response is null ? null : RedisAuthenticationQuotaScriptProtocol.Parse(response);
        eval = new { result = result.Status.ToString(), semantic = reply?.IsSemantic,
            status = reply?.Status, category = reply?.Category, luaNowUs = reply?.NowMicroseconds, hintCount,
            productionScript = true };
    }
    internal void Accounted(long value) => accounted = value;

    internal async Task RoundCompleteAsync(DistributedQuotaTestScope scope)
    {
        if (rounds.Count < 3) rounds.Add(Row(await TransitionEvidenceAsync()));
    }

    private object Row(object? countTransition) => new { round, accountedBefore, missing, seedRequests,
        seedScopeOccurrences = seedRequests * 2, waitHorizonUs = horizon, waitTargetUs = horizon + 1,
        redisTimeAtWaitCrossing, expectedAccounted = 2, actualAccounted = accounted, beforeWait, eval, countTransition };

    private static async Task<object> SnapshotAsync(DistributedQuotaTestScope scope, long cutoff)
    {
        var database = scope.Db;
        var batch = database.CreateBatch();
        var before = batch.ExecuteAsync("TIME");
        var metadata = batch.HashGetAsync(scope.Config.Keys.Metadata,
            ["count", "horizon", "version", "last", "dirty"]);
        var cardinality = batch.SortedSetLengthAsync(scope.Config.Keys.Registry);
        var expired = batch.SortedSetLengthAsync(scope.Config.Keys.Registry, double.NegativeInfinity, cutoff);
        var expiry = batch.ExecuteAsync("PEXPIRETIME", scope.Config.Keys.Registry);
        var ttl = batch.ExecuteAsync("PTTL", scope.Config.Keys.Registry);
        var after = batch.ExecuteAsync("TIME");
        batch.Execute();
        await Task.WhenAll(before, metadata, cardinality, expired, expiry, ttl, after);
        var fields = await metadata;
        long? Number(int index) => long.TryParse(fields[index].ToString(), out var value) ? value : null;
        long Time(RedisResult value)
        { var parts = (RedisResult[])value!; return checked((long)parts[0] * 1000000 + (long)parts[1]); }
        return new { nonAtomic = true, redisBeforeUs = Time(await before), redisAfterUs = Time(await after),
            metadataCount = Number(0), metadataHorizon = Number(1), metadataVersion = Number(2),
            metadataLast = Number(3), metadataDirty = Number(4), registryCardinality = await cardinality,
            registryScoresThroughCutoff = await expired, cutoffUs = cutoff,
            registryPExpireTime = (long)await expiry, registryPttl = (long)await ttl };
    }

    private object Context(DistributedQuotaTestScope scope) => new { partition,
        configuredMaxActivePartitions = scope.Config.Snapshot.Capacity.MaxActivePartitions,
        fixtureShared = true, collectionParallelizationDisabled = true, namespacePerScope = true,
        precedingTestCleanup = "NOT_OBSERVED_DIFFERENT_NAMESPACE",
        generatedRequests, trackedDistinctBucketIdentities = scope.TrackedBucketCount,
        scopeCounts = scope.TrackedScopeCounts(), physicalActiveBuckets = "NOT_MEASURED_NO_KEY_SCAN",
        startState };

    internal void Report(DistributedQuotaTestScope scope, ITestOutputHelper output)
    {
        foreach (var row in rounds)
            output.WriteLine("DISTRIBUTED_CAPACITY_DIAGNOSTIC " +
                JsonSerializer.Serialize(new { context = Context(scope), round = row }));
    }

    internal async Task<Exception> FailureAsync(DistributedQuotaTestScope scope, Exception original)
    {
        object? observedTransition;
        try
        {
            // Presupuesto exclusivamente de observación tras el fallo; no afecta el test ni EVAL.
            observedTransition = await TransitionEvidenceAsync().WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch { observedTransition = new { unavailable = true }; }
        var json = JsonSerializer.Serialize(new { context = Context(scope), failure = Row(observedTransition),
            completedRounds = rounds.Count });
        if (json.Length > 4400)
            json = JsonSerializer.Serialize(new { partition, round, expectedAccounted = 2, actualAccounted = accounted,
                waitHorizonUs = horizon, redisTimeAtWaitCrossing, countTransition = observedTransition,
                contextOmitted = true });
        return new InvalidOperationException("DISTRIBUTED_CAPACITY_DIAGNOSTIC " + json, original);
    }
}
