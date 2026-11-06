using System.Security.Cryptography;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.API.Infrastructure.Security;
using Enver.Testing;
using StackExchange.Redis;

namespace Enver.API.SecurityTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class AuthenticationSecurityCoordinatorSecurityTests(AuthenticationDependenciesFixture dependencies)
{
    [Fact]
    public async Task Ping_and_absent_revocation_key_do_not_bypass_a_closed_authority()
    {
        var authority = new AuthenticationSecurityAuthorityFake(Closed(T0));
        var coordinator = Coordinator(authority, new ManualTimeProvider(T0));
        await using var connections = new RedisConnections(Options());
        var database = (await connections.GetMultiplexerAsync()).GetDatabase();
        var key = $"{dependencies.RedisNamespace}:t020:missing:{Guid.NewGuid():N}";
        Assert.True((await database.PingAsync()).TotalMilliseconds >= 0);
        Assert.True((await database.StringGetAsync(key)).IsNull);
        var store = new RedisTokenRevocationStore(connections, coordinator, Options());
        using var budget = Budget();

        var result = await store.CheckAsync(new TokenRevocationReadRequest("enver-api", Jti()), budget);

        Assert.Equal(TokenRevocationReadStatus.Unavailable, result.Status);
    }

    [Fact]
    public async Task Redis_restart_and_empty_state_do_not_reopen_closed_authority()
    {
        var authority = new AuthenticationSecurityAuthorityFake(Closed(T0));
        var coordinator = Coordinator(authority, new ManualTimeProvider(T0));
        await using var connections = new RedisConnections(Options());
        var database = (await connections.GetMultiplexerAsync()).GetDatabase();

        await dependencies.StopRedisAsync();
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            Assert.NotNull(await Record.ExceptionAsync(async () => await database.StringGetAsync($"{dependencies.RedisNamespace}:t020:restart").WaitAsync(timeout.Token)));
        }
        finally
        {
            await dependencies.StartRedisAsync();
        }

        await using var restoredConnections = new RedisConnections(Options());
        var restoredDatabase = (await restoredConnections.GetMultiplexerAsync()).GetDatabase();
        Assert.True((await restoredDatabase.StringGetAsync($"{dependencies.RedisNamespace}:t020:restart")).IsNull);
        using var budget = Budget();
        Assert.False((await coordinator.EvaluateAsync(budget)).CanContinue);
    }

    [Fact]
    public async Task Missing_or_unavailable_authority_never_opens_admission()
    {
        var missing = Coordinator(new AuthenticationSecurityAuthorityFake(), new ManualTimeProvider(T0));
        var unavailableAuthority = new AuthenticationSecurityAuthorityFake(Open());
        unavailableAuthority.EnqueueReadOutcome(AuthenticationSecurityAuthorityResultStatus.Unavailable);
        var unavailable = Coordinator(unavailableAuthority, new ManualTimeProvider(T0));

        using var missingBudget = Budget();
        using var unavailableBudget = Budget();
        Assert.False((await missing.EvaluateAsync(missingBudget)).CanContinue);
        Assert.False((await unavailable.EvaluateAsync(unavailableBudget)).CanContinue);
    }

    [Fact]
    public async Task Stale_generation_and_incomplete_quarantine_cannot_reopen()
    {
        var authority = new AuthenticationSecurityAuthorityFake(Closed(T0, generation: 2, revision: 7));
        using var staleBudget = Budget();
        var stale = await authority.TryReopenAsync(new TryReopenAuthenticationSecurityRequest(1, 7, T0.AddHours(2)), staleBudget);
        var coordinator = Coordinator(authority, new ManualTimeProvider(T0));

        using var controlBudget = Budget();
        var incomplete = await coordinator.TryReopenAfterQuarantineAsync(controlBudget);

        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Conflict, stale.Status);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Unavailable, incomplete.Status);
    }

    private RedisOptions Options() => new()
    {
        ConnectionString = dependencies.RedisConnectionString,
        NamespacePrefix = dependencies.RedisNamespace,
        OperationTimeoutMilliseconds = 250,
        ConnectionTimeoutMilliseconds = 250,
        RevocationOperationalMarginSeconds = 60
    };

    private static AuthenticationSecurityCoordinator Coordinator(AuthenticationSecurityAuthorityFake authority, TimeProvider time) =>
        new(authority, new JwtOptions { LifetimeSeconds = 900, ClockSkewSeconds = 30 }, new RedisOptions { RevocationOperationalMarginSeconds = 60 }, time);

    private static AuthenticationSecurityAuthorityState Open() =>
        new(AuthenticationSecurityGlobalState.Open, 0, 0, null, null, null, null, null);

    private static AuthenticationSecurityAuthorityState Closed(DateTimeOffset lastPossible, long generation = 1, long revision = 1) =>
        new(AuthenticationSecurityGlobalState.Closed, generation, revision, lastPossible, lastPossible.AddSeconds(990), 900, 30, 60);

    private static RequestBudget Budget() => new(10_000_000, CancellationToken.None);
    private static string Jti() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static readonly DateTimeOffset T0 = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);

    private sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private readonly DateTimeOffset current = utcNow;
        public override DateTimeOffset GetUtcNow() => current;
    }
}
