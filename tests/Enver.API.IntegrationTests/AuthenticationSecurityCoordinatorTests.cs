using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.API.Infrastructure.Security;
using Enver.Testing;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class AuthenticationSecurityCoordinatorTests(AuthenticationDependenciesFixture dependencies)
{
    [Fact]
    public async Task Two_local_consumers_share_closed_authority_through_a_real_Redis_restart_and_only_explicit_quarantine_reopen()
    {
        var time = new ManualTimeProvider(new DateTimeOffset(2026, 11, 6, 12, 0, 0, TimeSpan.Zero));
        var authority = new AuthenticationSecurityAuthorityFake(Open());
        var first = Coordinator(authority, time);
        var second = Coordinator(authority, time);
        await using var connections = new RedisConnections(Options());
        var database = (await connections.GetMultiplexerAsync()).GetDatabase();
        Assert.True((await database.PingAsync()).TotalMilliseconds >= 0);
        Assert.True((await first.EvaluateAsync(Budget())).CanContinue);
        var emission = await first.BeginEmissionAsync(Budget());
        Assert.NotNull(emission);

        var close = await first.CloseAsync(Budget());
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, close.Status);
        Assert.False((await second.EvaluateAsync(Budget())).CanContinue);

        await dependencies.StopRedisAsync();
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            Assert.NotNull(await Record.ExceptionAsync(async () => await database.StringGetAsync(AbsentKey()).WaitAsync(timeout.Token)));
        }
        finally
        {
            await dependencies.StartRedisAsync();
        }

        await using var restoredConnections = new RedisConnections(Options());
        var restoredDatabase = (await restoredConnections.GetMultiplexerAsync()).GetDatabase();
        Assert.True((await restoredDatabase.PingAsync()).TotalMilliseconds >= 0);
        Assert.True((await restoredDatabase.StringGetAsync(AbsentKey())).IsNull);
        Assert.False((await second.EvaluateAsync(Budget())).CanContinue);

        time.Advance(TimeSpan.FromSeconds(990));
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Unavailable, (await first.TryReopenAfterQuarantineAsync(Budget())).Status);
        first.CompleteEmission(emission!);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, (await first.TryReopenAfterQuarantineAsync(Budget())).Status);
        Assert.True((await second.EvaluateAsync(Budget())).CanContinue);
    }

    [Fact]
    public async Task Shared_authority_unavailable_never_admits_a_second_consumer()
    {
        var authority = new AuthenticationSecurityAuthorityFake(Open());
        authority.EnqueueReadOutcome(AuthenticationSecurityAuthorityResultStatus.Unavailable);
        var second = Coordinator(authority, new ManualTimeProvider(new DateTimeOffset(2026, 11, 6, 12, 0, 0, TimeSpan.Zero)));

        var decision = await second.EvaluateAsync(Budget());

        Assert.False(decision.CanContinue);
        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, decision.Admission);
    }

    private RedisOptions Options() => new()
    {
        ConnectionString = dependencies.RedisConnectionString,
        ConnectionTimeoutMilliseconds = 250,
        OperationTimeoutMilliseconds = 250,
        RevocationOperationalMarginSeconds = 60
    };

    private static AuthenticationSecurityCoordinator Coordinator(AuthenticationSecurityAuthorityFake authority, TimeProvider time) =>
        new(authority, new JwtOptions { LifetimeSeconds = 900, ClockSkewSeconds = 30 }, new RedisOptions { RevocationOperationalMarginSeconds = 60 }, time);

    private static AuthenticationSecurityAuthorityState Open() =>
        new(AuthenticationSecurityGlobalState.Open, 0, 0, null, null, null, null, null);

    private static RequestBudget Budget() => new(10_000_000, CancellationToken.None);
    private string AbsentKey() => $"{dependencies.RedisNamespace}:t020:absent";

    private sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset current = utcNow;
        private long timestamp;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => timestamp;
        public override DateTimeOffset GetUtcNow() => current;
        public void Advance(TimeSpan duration) { current = current.Add(duration); timestamp += duration.Ticks; }
    }
}
