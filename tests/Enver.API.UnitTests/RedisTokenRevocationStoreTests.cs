using System.Security.Cryptography;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using StackExchange.Redis;

namespace Enver.API.UnitTests;

public sealed class RedisTokenRevocationStoreTests
{
    [Fact]
    public async Task Valid_request_calculates_a_conservative_absolute_retention_and_confirms_once()
    {
        var time = new TestTimeProvider(DateTimeOffset.Parse("2026-11-06T12:00:00.0005000+00:00"));
        long observedRetention = 0;
        var calls = 0;
        await using var connections = new RedisConnections(Options());
        var store = Store(connections, time, revoke: (_, _, _, retainUntil) =>
        {
            calls++;
            observedRetention = retainUntil;
            return ValueTask.FromResult(Confirmed());
        });
        using var requestBudget = new RequestBudget(1000, CancellationToken.None, time);

        var result = await store.RevokeAsync(WriteRequest(time.UtcDateTime.AddMinutes(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60)), requestBudget);

        Assert.Equal(TokenRevocationWriteStatus.Confirmed, result.Status);
        Assert.False(result.ResultUncertain);
        Assert.Equal(1, calls);
        Assert.Equal(DateTimeOffset.Parse("2026-11-06T12:02:30.0010000+00:00").ToUnixTimeMilliseconds(), observedRetention);
    }

    [Fact]
    public async Task Expired_retention_is_a_no_operation_without_a_Redis_command()
    {
        var time = new TestTimeProvider(DateTimeOffset.Parse("2026-11-06T12:00:00+00:00"));
        var calls = 0;
        await using var connections = new RedisConnections(Options());
        var store = Store(connections, time, revoke: (_, _, _, _) =>
        {
            calls++;
            return ValueTask.FromResult(Confirmed());
        });
        using var requestBudget = new RequestBudget(1000, CancellationToken.None, time);

        var result = await store.RevokeAsync(WriteRequest(time.GetUtcNow().AddMilliseconds(-1), TimeSpan.Zero, TimeSpan.Zero), requestBudget);

        Assert.Equal(TokenRevocationWriteStatus.NoOperation, result.Status);
        Assert.False(result.ResultUncertain);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Negative_skew_or_margin_is_unavailable_without_a_Redis_command()
    {
        var time = new TestTimeProvider(DateTimeOffset.Parse("2026-11-06T12:00:00+00:00"));
        var calls = 0;
        await using var connections = new RedisConnections(Options());
        var store = Store(connections, time, revoke: (_, _, _, _) =>
        {
            calls++;
            return ValueTask.FromResult(Confirmed());
        });
        using var firstBudget = new RequestBudget(1000, CancellationToken.None, time);
        using var secondBudget = new RequestBudget(1000, CancellationToken.None, time);

        var negativeSkew = await store.RevokeAsync(WriteRequest(time.GetUtcNow().AddMinutes(1), TimeSpan.FromTicks(-1), TimeSpan.Zero), firstBudget);
        var negativeMargin = await store.RevokeAsync(WriteRequest(time.GetUtcNow().AddMinutes(1), TimeSpan.Zero, TimeSpan.FromTicks(-1)), secondBudget);

        Assert.Equal(TokenRevocationWriteStatus.Unavailable, negativeSkew.Status);
        Assert.Equal(TokenRevocationWriteStatus.Unavailable, negativeMargin.Status);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Overflowing_retention_is_unavailable_without_a_Redis_command()
    {
        var time = new TestTimeProvider(DateTimeOffset.Parse("2026-11-06T12:00:00+00:00"));
        var calls = 0;
        await using var connections = new RedisConnections(Options());
        var store = Store(connections, time, revoke: (_, _, _, _) =>
        {
            calls++;
            return ValueTask.FromResult(Confirmed());
        });
        using var requestBudget = new RequestBudget(1000, CancellationToken.None, time);

        var result = await store.RevokeAsync(WriteRequest(DateTimeOffset.MaxValue, TimeSpan.FromTicks(1), TimeSpan.Zero), requestBudget);

        Assert.Equal(TokenRevocationWriteStatus.Unavailable, result.Status);
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(AuthenticationSecurityAdmission.Unavailable, TokenRevocationWriteStatus.Unavailable)]
    [InlineData(AuthenticationSecurityAdmission.Uncertain, TokenRevocationWriteStatus.Uncertain)]
    public async Task Unverifiable_security_state_does_not_send_a_write(
        AuthenticationSecurityAdmission admission,
        TokenRevocationWriteStatus expected)
    {
        var time = new TestTimeProvider(DateTimeOffset.Parse("2026-11-06T12:00:00+00:00"));
        var calls = 0;
        await using var connections = new RedisConnections(Options());
        var store = Store(
            connections,
            time,
            state: new State(admission, BudgetCancellationCause.DependencyTimeout),
            revoke: (_, _, _, _) =>
            {
                calls++;
                return ValueTask.FromResult(Confirmed());
            });
        using var requestBudget = new RequestBudget(1000, CancellationToken.None, time);

        var result = await store.RevokeAsync(WriteRequest(time.GetUtcNow().AddMinutes(1), TimeSpan.Zero, TimeSpan.Zero), requestBudget);

        Assert.Equal(expected, result.Status);
        Assert.Equal(BudgetCancellationCause.DependencyTimeout, result.CancellationCause);
        Assert.Equal(admission == AuthenticationSecurityAdmission.Uncertain, result.ResultUncertain);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Timeout_before_dispatch_is_unavailable_and_not_uncertain()
    {
        var time = new TestTimeProvider(DateTimeOffset.Parse("2026-11-06T12:00:00+00:00"));
        var calls = 0;
        await using var connections = new RedisConnections(Options());
        var store = Store(
            connections,
            time,
            state: new State(AuthenticationSecurityAdmission.Unavailable, BudgetCancellationCause.DependencyTimeout),
            revoke: (_, _, _, _) =>
            {
                calls++;
                return ValueTask.FromResult(Confirmed());
            });
        using var requestBudget = new RequestBudget(1000, CancellationToken.None, time);

        var result = await store.RevokeAsync(WriteRequest(time.GetUtcNow().AddMinutes(1), TimeSpan.Zero, TimeSpan.Zero), requestBudget);

        Assert.Equal(TokenRevocationWriteStatus.Unavailable, result.Status);
        Assert.False(result.ResultUncertain);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Timeout_after_dispatch_is_uncertain()
    {
        var time = new TestTimeProvider(DateTimeOffset.Parse("2026-11-06T12:00:00+00:00"));
        await using var connections = new RedisConnections(Options());
        var store = Store(connections, time, revoke: (budget, _, _, _) =>
        {
            time.Advance(TimeSpan.FromMilliseconds(50));
            budget.ThrowIfCancellationRequested();
            return ValueTask.FromResult(Confirmed());
        });
        using var requestBudget = new RequestBudget(1000, CancellationToken.None, time);

        var result = await store.RevokeAsync(WriteRequest(time.GetUtcNow().AddMinutes(1), TimeSpan.Zero, TimeSpan.Zero), requestBudget);

        Assert.Equal(TokenRevocationWriteStatus.Uncertain, result.Status);
        Assert.True(result.ResultUncertain);
        Assert.Equal(BudgetCancellationCause.DependencyTimeout, result.CancellationCause);
    }

    [Fact]
    public async Task A_post_dispatch_failure_is_not_retried()
    {
        var time = new TestTimeProvider(DateTimeOffset.Parse("2026-11-06T12:00:00+00:00"));
        var calls = 0;
        await using var connections = new RedisConnections(Options());
        var store = Store(connections, time, revoke: (_, _, _, _) =>
        {
            calls++;
            throw new InvalidOperationException("synthetic failure");
        });
        using var requestBudget = new RequestBudget(1000, CancellationToken.None, time);

        var result = await store.RevokeAsync(WriteRequest(time.GetUtcNow().AddMinutes(1), TimeSpan.Zero, TimeSpan.Zero), requestBudget);

        Assert.Equal(TokenRevocationWriteStatus.Uncertain, result.Status);
        Assert.True(result.ResultUncertain);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Store_uses_the_fixed_revocation_key_and_minimal_requests()
    {
        var time = new TestTimeProvider(DateTimeOffset.Parse("2026-11-06T12:00:00+00:00"));
        RedisKey observedKey = default;
        await using var connections = new RedisConnections(Options());
        var store = Store(connections, time, revoke: (_, key, _, _) =>
        {
            observedKey = key;
            return ValueTask.FromResult(Confirmed());
        });
        using var requestBudget = new RequestBudget(1000, CancellationToken.None, time);
        var jti = Jti();

        _ = await store.RevokeAsync(new TokenRevocationWriteRequest("enver-api", jti, time.GetUtcNow().AddMinutes(1), TimeSpan.Zero, TimeSpan.Zero), requestBudget);

        Assert.Equal($"enver:testing:auth:revoked:enver-api:{jti}", observedKey.ToString());
        Assert.Equal(["EffectiveClockSkew", "ExpiresAtUtc", "Issuer", "Jti", "OperationalMargin"], typeof(TokenRevocationWriteRequest).GetProperties().Select(property => property.Name).Order());
        Assert.Equal(["Issuer", "Jti"], typeof(TokenRevocationReadRequest).GetProperties().Select(property => property.Name).Order());
    }

    [Fact]
    public async Task Read_maps_present_and_absent_values_only_after_verifiable_admission()
    {
        var time = new TestTimeProvider(DateTimeOffset.Parse("2026-11-06T12:00:00+00:00"));
        await using var connections = new RedisConnections(Options());
        var values = new Queue<RedisValue>(["1", RedisValue.Null]);
        var store = Store(connections, time, check: (_, _) => ValueTask.FromResult(values.Dequeue()));
        using var firstBudget = new RequestBudget(1000, CancellationToken.None, time);
        using var secondBudget = new RequestBudget(1000, CancellationToken.None, time);
        var request = new TokenRevocationReadRequest("enver-api", Jti());

        var revoked = await store.CheckAsync(request, firstBudget);
        var notRevoked = await store.CheckAsync(request, secondBudget);

        Assert.Equal(TokenRevocationReadStatus.Revoked, revoked.Status);
        Assert.Equal(TokenRevocationReadStatus.NotRevoked, notRevoked.Status);
    }

    private static RedisTokenRevocationStore Store(
        RedisConnections connections,
        TimeProvider time,
        IAuthenticationSecurityState? state = null,
        Func<DependencyBudget, RedisKey, string, long, ValueTask<RedisResult>>? revoke = null,
        Func<DependencyBudget, RedisKey, ValueTask<RedisValue>>? check = null) =>
        new(connections, state ?? new State(AuthenticationSecurityAdmission.Verifiable), Options(), time, revoke, check);

    private static RedisOptions Options() => new()
    {
        ConnectionString = "127.0.0.1:1",
        NamespacePrefix = "enver:testing",
        OperationTimeoutMilliseconds = 50,
        ConnectionTimeoutMilliseconds = 50
    };

    private static TokenRevocationWriteRequest WriteRequest(DateTimeOffset expiresAtUtc, TimeSpan skew, TimeSpan margin) =>
        new("enver-api", Jti(), expiresAtUtc, skew, margin);

    private static string Jti() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static RedisResult Confirmed() => RedisResult.Create((RedisValue)"1");

    private sealed class State(AuthenticationSecurityAdmission admission, BudgetCancellationCause cause = BudgetCancellationCause.None) : IAuthenticationSecurityState
    {
        public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget requestBudget) =>
            ValueTask.FromResult(new AuthenticationSecurityDecision(admission, cause));
    }

    private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private long timestamp;
        public DateTimeOffset UtcDateTime => utcNow;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => timestamp;
        public override DateTimeOffset GetUtcNow() => utcNow;
        public void Advance(TimeSpan duration) => timestamp += duration.Ticks;
    }
}
