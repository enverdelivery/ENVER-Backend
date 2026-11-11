using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisAuthenticationQuotaCapacityTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public async Task Direct_5000_boundary_existing_only_and_no_partial_capacity()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        await s.FillRegisterAsync(2499);
        Assert.Equal(4998, await s.CountAsync());
        var oneNew = s.Request(3000, identities: new Dictionary<AuthenticationQuotaScopeType, int> { [AuthenticationQuotaScopeType.Origin] = 11 });
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(oneNew)).Status);
        Assert.Equal(4999, await s.CountAsync());
        var twoNew = s.Request(4000);
        var insufficient = await s.SendAsync(twoNew);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, insufficient.Status);
        Assert.Null(insufficient.RetryAfter);
        foreach (var scope in twoNew.Scopes) Assert.False(await s.Database.KeyExistsAsync(s.Key(twoNew, scope)));
        var last = s.Request(3001, identities: new Dictionary<AuthenticationQuotaScopeType, int> { [AuthenticationQuotaScopeType.Origin] = 11 });
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(last)).Status);
        Assert.Equal(5000, await s.CountAsync());
        Assert.Equal(5000, await s.Database.SortedSetLengthAsync(s.Configuration.Keys.Registry));
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(s.Request(2), s.SecondStore)).Status);
        var extra = s.Request(3002, identities: new Dictionary<AuthenticationQuotaScopeType, int> { [AuthenticationQuotaScopeType.Origin] = 11 });
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(extra)).Status);
        Assert.False(await s.Database.KeyExistsAsync(s.Key(extra, extra.Scopes.Single(x => x.ScopeType == AuthenticationQuotaScopeType.Email))));
        Assert.Equal(5000, await s.CountAsync());
    }

    [Fact]
    public async Task Exactly_2500_real_admissions_create_5000_and_consumers_race_without_exceeding_limit()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        using var diagnostic = new RedisCapacityDiagnostic(s, output);
        await diagnostic.SnapshotAsync("before");
        try
        {
        // Mismo llenado secuencial, requests, store real y aserción; sin override/retry.
        for (var i = 1; i <= 2499; i++)
        {
            var request = s.Request(i);
            var result = await diagnostic.SendAsync(request, s.Store);
            if (result.Status != AuthenticationQuotaStoreStatus.Admitted)
                await diagnostic.ObserveFailureAsync(request, i);
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, result.Status);
            diagnostic.RecordAdmission();
        }
        var requests = Enumerable.Range(3000, 8).Select(i => s.Request(i)).ToArray();
        var results = await Task.WhenAll(requests.Select((r, i) => diagnostic.SendAsync(r, i % 2 == 0 ? s.Store : s.SecondStore)));
        diagnostic.RecordRace(results);
        Assert.Single(results, r => r.Status == AuthenticationQuotaStoreStatus.Admitted);
        Assert.Equal(7, results.Count(r => r.Status == AuthenticationQuotaStoreStatus.Unavailable));
        Assert.Equal(5000, await s.CountAsync());
        Assert.Equal(5000, await s.Database.SortedSetLengthAsync(s.Configuration.Keys.Registry));
        for (var i = 0; i < requests.Length; i++)
            foreach (var scope in requests[i].Scopes)
                Assert.Equal(results[i].Status == AuthenticationQuotaStoreStatus.Admitted, await s.Database.KeyExistsAsync(s.Key(requests[i], scope)));
        }
        finally
        {
            await diagnostic.SnapshotAsync("after");
            diagnostic.Flush();
        }
    }
}

// Diagnóstico test-only: reloj/timers System delegados y excepciones de primera oportunidad.
// No intercepta/reemplaza comandos, no lee valores sensibles ni modifica budgets/aserciones.
internal sealed class RedisCapacityDiagnostic : IDisposable
{
    private readonly RedisAuthenticationQuotaTestScope scope;
    private readonly ITestOutputHelper output;
    private readonly ConcurrentQueue<string> events = new();
    private readonly AsyncLocal<Operation?> current = new();
    private readonly Stopwatch elapsed = Stopwatch.StartNew();
    private int admitted;
    private int calls;
    private double maximumOperationMs;

    public RedisCapacityDiagnostic(RedisAuthenticationQuotaTestScope scope, ITestOutputHelper output)
    {
        this.scope = scope;
        this.output = output;
        scope.Connections.ConnectivityChanged += Signal;
        scope.SecondConnections.ConnectivityChanged += Signal;
        AppDomain.CurrentDomain.FirstChanceException += ExceptionObserved;
        Mark($"REQUEST_BUDGET_MS=10000 REDIS_DEPENDENCY_MS={scope.Options.OperationTimeoutMilliseconds} CAPACITY=5000 PRODUCT_OVERRIDES=false RETRY=false");
    }

    private void Mark(string text) => events.Enqueue($"REDIS_CAPACITY_DIAG AT_MS={elapsed.Elapsed.TotalMilliseconds:F3} {text}");
    private void Signal(object? sender, RedisConnectionSignal signal) => Mark($"CONNECTION_SIGNAL={signal}");
    private void ExceptionObserved(object? sender, FirstChanceExceptionEventArgs args)
    {
        var operation = current.Value;
        if (operation is null) return;
        var error = args.Exception;
        if (error is RedisException or OperationCanceledException or ObjectDisposedException)
            operation.Events.Enqueue($"EXCEPTION_TYPE={error.GetType().Name} REDIS_ERROR_CODE={SafeCode(error)} AT_OPERATION_MS={operation.Elapsed.TotalMilliseconds:F3}");
    }

    private static string SafeCode(Exception error)
    {
        if (error is not RedisServerException) return "none";
        foreach (var code in new[] { "OOM", "ERR", "NOSCRIPT", "LOADING", "READONLY", "BUSY", "WRONGTYPE", "CLUSTERDOWN", "MOVED" })
            if (error.Message.StartsWith(code + " ", StringComparison.Ordinal)) return code;
        return "other-redacted";
    }

    public async Task<AuthenticationQuotaConsumeResult> SendAsync(AuthenticationQuotaConsumeRequest request, RedisAuthenticationQuotaStore store)
    {
        var operation = new Operation();
        current.Value = operation;
        var ordinal = Interlocked.Increment(ref calls);
        // Equivalente al Budget() de este scope: 10000 ms; reloj real, no virtual.
        using var budget = new RequestBudget(10000, CancellationToken.None, operation);
        try
        {
            var result = await store.EvaluateAndConsumeAsync(request, budget);
            var duration = operation.Elapsed.TotalMilliseconds;
            lock (events) maximumOperationMs = Math.Max(maximumOperationMs, duration);
            if (result.Status != AuthenticationQuotaStoreStatus.Admitted || ordinal == 2499)
            {
                Mark($"CALL={ordinal} STATUS={result.Status} RESULT_CANCELLATION_CAUSE={result.CancellationCause} OPERATION_MS={duration:F3} REQUEST_REMAINING_MS={budget.Remaining.TotalMilliseconds:F3} REQUEST_TOKEN_CANCELLED={budget.Token.IsCancellationRequested} REQUEST_CAUSE={budget.Cause}");
                foreach (var entry in operation.Events) Mark($"CALL={ordinal} {entry}");
            }
            return result;
        }
        finally { current.Value = null; }
    }

    public void RecordAdmission() => admitted++;
    public void RecordRace(AuthenticationQuotaConsumeResult[] results)
    {
        var winners = results.Count(result => result.Status == AuthenticationQuotaStoreStatus.Admitted);
        admitted += winners;
        Mark($"RACE_ADMITTED={winners} RACE_UNAVAILABLE={results.Count(result => result.Status == AuthenticationQuotaStoreStatus.Unavailable)} ADMISSIONS_COMPLETED={admitted}");
    }

    public async Task ObserveFailureAsync(AuthenticationQuotaConsumeRequest request, int attempt)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var keysPresent = 0;
        foreach (var item in request.Scopes)
            if (await scope.Database.KeyExistsAsync(scope.Key(request, item)).WaitAsync(timeout.Token)) keysPresent++;
        Mark($"FILL_FAILURE_ATTEMPT={attempt} ADMISSIONS_COMPLETED={admitted} EXPECTED_PARTITIONS_FROM_CONFIRMED={admitted * 2} FAILED_REQUEST_BUCKETS_PRESENT={keysPresent} REQUIRED_BUCKETS={request.Scopes.Count} DIAGNOSTIC_READ_ONLY=true");
    }

    public async Task SnapshotAsync(string phase)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            var ping = await scope.Database.PingAsync(CommandFlags.DemandMaster).WaitAsync(timeout.Token);
            var info = (await scope.Database.ExecuteAsync("INFO", "ALL").WaitAsync(timeout.Token)).ToString();
            var allowed = new HashSet<string>(StringComparer.Ordinal)
            {
                "role", "connected_slaves", "cluster_enabled", "maxmemory", "maxmemory_policy", "used_memory", "used_memory_peak",
                "connected_clients", "blocked_clients", "rejected_connections", "evicted_keys", "total_error_replies"
            };
            var safe = info.Split('\n').Select(line => line.Trim()).Where(line =>
                allowed.Contains(line.Split(':')[0]) || line.StartsWith("errorstat_OOM:", StringComparison.Ordinal));
            var multiplexer = await scope.Connections.GetMultiplexerAsync(timeout.Token);
            var server = multiplexer.GetServer(multiplexer.GetEndPoints()[0]);
            long namespaceKeys = 0;
            await foreach (var _ in server.KeysAsync(scope.Database.Database, scope.Configuration.Keys.Prefix + "*").WithCancellation(timeout.Token)) namespaceKeys++;
            var count = await scope.CountAsync().WaitAsync(timeout.Token);
            var registry = await scope.Database.SortedSetLengthAsync(scope.Configuration.Keys.Registry).WaitAsync(timeout.Token);
            Mark($"SNAPSHOT={phase} AVAILABLE=true PING_MS={ping.TotalMilliseconds:F3} ADMISSIONS_COMPLETED={admitted} METADATA_PARTITIONS={count} REGISTRY_PARTITIONS={registry} NAMESPACE_KEY_COUNT={namespaceKeys} MAX_OPERATION_MS={maximumOperationMs:F3} {string.Join(" ", safe)}");
        }
        catch (Exception error) { Mark($"SNAPSHOT={phase} OBSERVATION_FAILED=true EXCEPTION_TYPE={error.GetType().Name} REDIS_ERROR_CODE={SafeCode(error)}"); }
    }

    public void Flush()
    {
        Mark($"TOTAL_DIAGNOSTIC_MS={elapsed.Elapsed.TotalMilliseconds:F3} CALLS={calls} ADMISSIONS_COMPLETED={admitted} NO_SENSITIVE_VALUES=true");
        foreach (var entry in events) output.WriteLine(entry);
    }
    public void Dispose()
    {
        AppDomain.CurrentDomain.FirstChanceException -= ExceptionObserved;
        scope.Connections.ConnectivityChanged -= Signal;
        scope.SecondConnections.ConnectivityChanged -= Signal;
    }

    private sealed class Operation : TimeProvider
    {
        private readonly long started = Stopwatch.GetTimestamp();
        private int timers;
        public ConcurrentQueue<string> Events { get; } = new();
        public TimeSpan Elapsed => Stopwatch.GetElapsedTime(started);
        public override long TimestampFrequency => TimeProvider.System.TimestampFrequency;
        public override long GetTimestamp() => TimeProvider.System.GetTimestamp();
        public override DateTimeOffset GetUtcNow() => TimeProvider.System.GetUtcNow();
        public override TimeZoneInfo LocalTimeZone => TimeProvider.System.LocalTimeZone;
        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var ordinal = Interlocked.Increment(ref timers);
            Events.Enqueue($"TIMER={ordinal} ASSIGNED_MS={dueTime.TotalMilliseconds:F3} AT_OPERATION_MS={Elapsed.TotalMilliseconds:F3}");
            return TimeProvider.System.CreateTimer(value =>
            {
                Events.Enqueue($"TIMER={ordinal} CALLBACK_BEGIN_MS={Elapsed.TotalMilliseconds:F3}");
                try { callback(value); }
                finally { Events.Enqueue($"TIMER={ordinal} CALLBACK_END_MS={Elapsed.TotalMilliseconds:F3}"); }
            }, state, dueTime, period);
        }
    }
}
