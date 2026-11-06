using System.Security.Cryptography;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisTokenRevocationStoreTests(AuthenticationDependenciesFixture dependencies)
{
    [Fact]
    public async Task First_revocation_executes_the_real_script_and_is_visible_to_a_second_consumer()
    {
        await using var firstConnections = new RedisConnections(Options());
        await using var secondConnections = new RedisConnections(Options());
        var first = Store(firstConnections);
        var second = Store(secondConnections);
        var request = WriteRequest(TimeSpan.FromMinutes(5));
        var key = Key(request.Jti);
        var database = (await firstConnections.GetMultiplexerAsync()).GetDatabase();
        using var writeBudget = new RequestBudget(1000, CancellationToken.None);
        using var readBudget = new RequestBudget(1000, CancellationToken.None);

        try
        {
            var write = await first.RevokeAsync(request, writeBudget);
            var check = await second.CheckAsync(new TokenRevocationReadRequest(request.Issuer, request.Jti), readBudget);
            var value = await database.StringGetAsync(key, CommandFlags.DemandMaster);
            var ttl = await database.KeyTimeToLiveAsync(key, CommandFlags.DemandMaster);

            Assert.Equal(TokenRevocationWriteStatus.Confirmed, write.Status);
            Assert.Equal(TokenRevocationReadStatus.Revoked, check.Status);
            Assert.Equal("1", value.ToString());
            Assert.NotNull(ttl);
            Assert.True(ttl > TimeSpan.Zero);
        }
        finally
        {
            await database.KeyDeleteAsync(key, CommandFlags.DemandMaster);
        }
    }

    [Fact]
    public async Task Repeated_revocations_are_idempotent_and_never_reduce_absolute_expiry()
    {
        await using var connections = new RedisConnections(Options());
        var store = Store(connections);
        var jti = Jti();
        var key = Key(jti);
        var database = (await connections.GetMultiplexerAsync()).GetDatabase();
        var baseExpiry = DateTimeOffset.UtcNow.AddMinutes(8);

        try
        {
            var first = await RevokeAsync(store, jti, baseExpiry);
            var afterFirst = await database.KeyExpireTimeAsync(key, CommandFlags.DemandMaster);
            var same = await RevokeAsync(store, jti, baseExpiry);
            var lower = await RevokeAsync(store, jti, baseExpiry.AddMinutes(-2));
            var afterLower = await database.KeyExpireTimeAsync(key, CommandFlags.DemandMaster);
            var higherExpiry = baseExpiry.AddMinutes(3);
            var higher = await RevokeAsync(store, jti, higherExpiry);
            var afterHigher = await database.KeyExpireTimeAsync(key, CommandFlags.DemandMaster);

            Assert.Equal(TokenRevocationWriteStatus.Confirmed, first.Status);
            Assert.Equal(TokenRevocationWriteStatus.Confirmed, same.Status);
            Assert.Equal(TokenRevocationWriteStatus.Confirmed, lower.Status);
            Assert.Equal(TokenRevocationWriteStatus.Confirmed, higher.Status);
            Assert.NotNull(afterFirst);
            Assert.NotNull(afterLower);
            Assert.NotNull(afterHigher);
            Assert.True(afterLower >= afterFirst);
            Assert.True(afterHigher >= higherExpiry.AddMilliseconds(-1));
        }
        finally
        {
            await database.KeyDeleteAsync(key, CommandFlags.DemandMaster);
        }
    }

    [Fact]
    public async Task Concurrent_revocations_preserve_the_greatest_requested_expiry()
    {
        await using var firstConnections = new RedisConnections(Options());
        await using var secondConnections = new RedisConnections(Options());
        var first = Store(firstConnections);
        var second = Store(secondConnections);
        var jti = Jti();
        var key = Key(jti);
        var database = (await firstConnections.GetMultiplexerAsync()).GetDatabase();
        var expiries = new[]
        {
            DateTimeOffset.UtcNow.AddMinutes(3),
            DateTimeOffset.UtcNow.AddMinutes(6),
            DateTimeOffset.UtcNow.AddMinutes(9),
            DateTimeOffset.UtcNow.AddMinutes(4)
        };

        try
        {
            var results = await Task.WhenAll(
                RevokeAsync(first, jti, expiries[0]),
                RevokeAsync(second, jti, expiries[1]),
                RevokeAsync(first, jti, expiries[2]),
                RevokeAsync(second, jti, expiries[3]));
            var expiry = await database.KeyExpireTimeAsync(key, CommandFlags.DemandMaster);

            Assert.All(results, result => Assert.Equal(TokenRevocationWriteStatus.Confirmed, result.Status));
            Assert.NotNull(expiry);
            Assert.True(expiry >= expiries.Max().AddMilliseconds(-1));
        }
        finally
        {
            await database.KeyDeleteAsync(key, CommandFlags.DemandMaster);
        }
    }

    [Fact]
    public async Task Absent_key_is_not_revoked_only_when_state_is_verifiable_and_namespaces_are_isolated()
    {
        await using var firstConnections = new RedisConnections(Options());
        await using var otherConnections = new RedisConnections(Options("other"));
        var first = Store(firstConnections);
        var other = Store(otherConnections, "other");
        var jti = Jti();
        var request = new TokenRevocationReadRequest("enver-api", jti);
        var firstKey = Key(jti);
        var otherKey = RedisKeyBuilder.BuildRevocationKey("other", "enver-api", jti);
        var firstDatabase = (await firstConnections.GetMultiplexerAsync()).GetDatabase();
        var otherDatabase = (await otherConnections.GetMultiplexerAsync()).GetDatabase();
        using var firstBudget = new RequestBudget(1000, CancellationToken.None);
        using var otherBudget = new RequestBudget(1000, CancellationToken.None);

        try
        {
            var absent = await first.CheckAsync(request, firstBudget);
            using var writeBudget = new RequestBudget(1000, CancellationToken.None);
            var write = await other.RevokeAsync(WriteRequest(TimeSpan.FromMinutes(5), jti), writeBudget);
            var firstCheck = await first.CheckAsync(request, new RequestBudget(1000, CancellationToken.None));
            var otherCheck = await other.CheckAsync(request, otherBudget);

            Assert.Equal(TokenRevocationReadStatus.NotRevoked, absent.Status);
            Assert.Equal(TokenRevocationWriteStatus.Confirmed, write.Status);
            Assert.Equal(TokenRevocationReadStatus.NotRevoked, firstCheck.Status);
            Assert.Equal(TokenRevocationReadStatus.Revoked, otherCheck.Status);
            Assert.False(await firstDatabase.KeyExistsAsync(firstKey, CommandFlags.DemandMaster));
            Assert.True(await otherDatabase.KeyExistsAsync(otherKey, CommandFlags.DemandMaster));
        }
        finally
        {
            await firstDatabase.KeyDeleteAsync(firstKey, CommandFlags.DemandMaster);
            await otherDatabase.KeyDeleteAsync(otherKey, CommandFlags.DemandMaster);
        }
    }

    [Fact]
    public async Task Closed_security_state_does_not_treat_an_existing_mark_as_not_revoked()
    {
        await using var connections = new RedisConnections(Options());
        var store = new RedisTokenRevocationStore(connections, new State(AuthenticationSecurityAdmission.Unavailable), Options());
        using var requestBudget = new RequestBudget(1000, CancellationToken.None);

        var result = await store.CheckAsync(new TokenRevocationReadRequest("enver-api", Jti()), requestBudget);

        Assert.Equal(TokenRevocationReadStatus.Unavailable, result.Status);
        Assert.False(result.ResultUncertain);
    }

    [Fact]
    public async Task Stopped_Redis_is_mapped_conservatively_without_a_permissive_read()
    {
        await using var connections = new RedisConnections(Options());
        var store = Store(connections);
        await dependencies.StopRedisAsync();
        try
        {
            using var requestBudget = new RequestBudget(1000, CancellationToken.None);
            var result = await store.CheckAsync(new TokenRevocationReadRequest("enver-api", Jti()), requestBudget);

            Assert.True(result.Status is TokenRevocationReadStatus.Unavailable or TokenRevocationReadStatus.Uncertain);
            Assert.NotEqual(TokenRevocationReadStatus.NotRevoked, result.Status);
        }
        finally
        {
            await dependencies.StartRedisAsync();
        }
    }

    [Fact]
    public async Task A_real_dispatched_timeout_is_uncertain()
    {
        await using var connections = new RedisConnections(Options(operationsTimeout: 100));
        var store = Store(connections, operationsTimeout: 100);
        _ = await connections.GetMultiplexerAsync();
        var controlOptions = ConfigurationOptions.Parse(dependencies.RedisConnectionString);
        controlOptions.AllowAdmin = true;
        using var control = await ConnectionMultiplexer.ConnectAsync(controlOptions);
        Assert.Equal("OK", (await control.GetDatabase().ExecuteAsync("CLIENT", "PAUSE", "1000", "ALL")).ToString());
        using var requestBudget = new RequestBudget(1000, CancellationToken.None);

        var result = await store.RevokeAsync(WriteRequest(TimeSpan.FromMinutes(5)), requestBudget);

        Assert.Equal(TokenRevocationWriteStatus.Uncertain, result.Status);
        Assert.True(result.ResultUncertain);
    }

    private RedisTokenRevocationStore Store(RedisConnections connections, string? environment = null, int operationsTimeout = 250) =>
        new(connections, new State(AuthenticationSecurityAdmission.Verifiable), Options(environment, operationsTimeout));

    private RedisOptions Options(string? environment = null, int operationsTimeout = 250) => new()
    {
        ConnectionString = dependencies.RedisConnectionString,
        NamespacePrefix = $"enver:{environment ?? dependencies.EnvironmentName.ToLowerInvariant()}",
        ConnectionTimeoutMilliseconds = 250,
        OperationTimeoutMilliseconds = operationsTimeout
    };

    private TokenRevocationWriteRequest WriteRequest(TimeSpan until, string? jti = null) =>
        new("enver-api", jti ?? Jti(), DateTimeOffset.UtcNow.Add(until), TimeSpan.Zero, TimeSpan.Zero);

    private async Task<TokenRevocationWriteResult> RevokeAsync(RedisTokenRevocationStore store, string jti, DateTimeOffset expiry)
    {
        using var requestBudget = new RequestBudget(1000, CancellationToken.None);
        return await store.RevokeAsync(new TokenRevocationWriteRequest("enver-api", jti, expiry, TimeSpan.Zero, TimeSpan.Zero), requestBudget);
    }

    private RedisKey Key(string jti) => RedisKeyBuilder.BuildRevocationKey(dependencies.EnvironmentName.ToLowerInvariant(), "enver-api", jti);

    private static string Jti() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private sealed class State(AuthenticationSecurityAdmission admission) : IAuthenticationSecurityState
    {
        public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget requestBudget) =>
            ValueTask.FromResult(new AuthenticationSecurityDecision(admission));
    }
}
