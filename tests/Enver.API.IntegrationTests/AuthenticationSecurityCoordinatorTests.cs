using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Aws;
using Enver.API.Infrastructure.Redis;
using Enver.API.Infrastructure.Security;
using Enver.Testing;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class AuthenticationSecurityCoordinatorTests(AuthenticationDependenciesFixture dependencies)
{
    [Fact]
    public async Task Two_coordinators_share_DynamoDb_Local_and_real_Redis_restart_never_bypasses_closed_authority()
    {
        var time = new ManualTimeProvider(new DateTimeOffset(2026, 11, 6, 12, 0, 0, TimeSpan.Zero));
        using var firstClient = dependencies.CreateDynamoDbClient();
        using var secondClient = dependencies.CreateDynamoDbClient();
        await dependencies.DeleteAuthenticationSecurityAuthorityAsync(firstClient);
        await dependencies.BootstrapAuthenticationSecurityAuthorityAsync(firstClient);
        var first = Coordinator(Authority(firstClient), time);
        var second = Coordinator(Authority(secondClient), time);
        await using var connections = new RedisConnections(Options());
        var database = (await connections.GetMultiplexerAsync()).GetDatabase();
        Assert.True((await database.PingAsync()).TotalMilliseconds >= 0);
        Assert.True((await first.EvaluateAsync(Budget())).CanContinue);
        var emission = await first.BeginEmissionAsync(Budget(time, 1_000));
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

        time.Advance(TimeSpan.FromSeconds(991));
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Unavailable, (await first.TryReopenAfterQuarantineAsync(Budget(time))).Status);
        first.CompleteEmission(emission!);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, (await second.TryReopenAfterQuarantineAsync(Budget(time))).Status);
        Assert.True((await second.EvaluateAsync(Budget(time))).CanContinue);
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

    private DynamoDbAuthenticationSecurityAuthority Authority(Amazon.DynamoDBv2.IAmazonDynamoDB client) => new(client, new AuthenticationSecurityAuthorityOptions
    {
        TableName = dependencies.DynamoDbTableName,
        OperationTimeoutMilliseconds = 500
    });

    private static AuthenticationSecurityCoordinator Coordinator(IAuthenticationSecurityAuthority authority, TimeProvider time) =>
        new(authority, new JwtOptions { LifetimeSeconds = 900, ClockSkewSeconds = 30 }, new RedisOptions { RevocationOperationalMarginSeconds = 60 }, time);

    private static AuthenticationSecurityAuthorityState Open() =>
        new(AuthenticationSecurityGlobalState.Open, 0, 0, null, null, null, null, null);

    private static RequestBudget Budget() => new(10_000_000, CancellationToken.None);
    private static RequestBudget Budget(TimeProvider clock, int milliseconds = 10_000) => new(milliseconds, CancellationToken.None, clock);
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
