using System.Security.Cryptography;
using Enver.API.Configuration;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisConnectionsTests(AuthenticationDependenciesFixture dependencies)
{
    [Fact]
    public async Task Concurrent_callers_reuse_one_real_multiplexer_and_synthetic_operations()
    {
        await using var connections = new RedisConnections(Options());

        var multiplexers = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => connections.GetMultiplexerAsync()));
        var multiplexer = multiplexers[0];
        Assert.All(multiplexers, current => Assert.Same(multiplexer, current));

        var database = multiplexer.GetDatabase();
        var key = Key();
        try
        {
            Assert.True(await database.StringSetAsync(key, "synthetic"));
            Assert.Equal("synthetic", await database.StringGetAsync(key));
            Assert.True(await database.KeyDeleteAsync(key));
        }
        finally
        {
            await database.KeyDeleteAsync(key);
        }
    }

    [Fact]
    public async Task Namespaces_are_isolated_without_global_deletion()
    {
        await using var connections = new RedisConnections(Options());
        var database = (await connections.GetMultiplexerAsync()).GetDatabase();
        var first = RedisKeyBuilder.BuildRevocationKey($"t016a_{dependencies.RunId}", RedisKeyBuilder.Issuer, Jti());
        var second = RedisKeyBuilder.BuildRevocationKey($"t016b_{dependencies.RunId}", RedisKeyBuilder.Issuer, Jti());

        try
        {
            await database.StringSetAsync(first, "first");
            await database.StringSetAsync(second, "second");
            await database.KeyDeleteAsync(first);

            Assert.False(await database.KeyExistsAsync(first));
            Assert.Equal("second", await database.StringGetAsync(second));
        }
        finally
        {
            await database.KeyDeleteAsync(first);
            await database.KeyDeleteAsync(second);
        }
    }

    [Fact]
    public async Task Cancelled_caller_does_not_dispose_the_reusable_multiplexer()
    {
        await using var connections = new RedisConnections(Options());
        var multiplexer = await connections.GetMultiplexerAsync();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connections.GetMultiplexerAsync(cancellation.Token));

        var reused = await connections.GetMultiplexerAsync();
        Assert.Same(multiplexer, reused);
        Assert.True(await reused.GetDatabase().PingAsync() >= TimeSpan.Zero);
    }

    [Fact]
    public async Task Disconnect_fails_without_backlog_during_a_controlled_outage()
    {
        await using var connections = new RedisConnections(Options());
        var multiplexer = await connections.GetMultiplexerAsync();
        var database = multiplexer.GetDatabase();

        await dependencies.StopRedisAsync();
        try
        {
            using var boundedWait = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var failure = await Record.ExceptionAsync(async () => await database.StringGetAsync(Key()).WaitAsync(boundedWait.Token));
            Assert.NotNull(failure);
        }
        finally
        {
            await dependencies.StartRedisAsync();
        }
    }

    [Fact]
    public async Task Disposing_with_synthetic_reads_in_flight_never_returns_another_keys_value()
    {
        var connections = new RedisConnections(Options());
        var multiplexer = await connections.GetMultiplexerAsync();
        var database = multiplexer.GetDatabase();
        var first = Key();
        var second = Key();
        const string expected = "expected-value";
        const string other = "other-value";

        await database.StringSetAsync(first, expected);
        await database.StringSetAsync(second, other);
        var reads = Enumerable.Range(0, 128).Select(_ => ReadAsync(database, first));
        await connections.DisposeAsync();
        var results = await Task.WhenAll(reads);

        Assert.DoesNotContain(results, result => result.Completed && result.Value == other);
        Assert.All(results.Where(result => result.Completed), result => Assert.Equal(expected, result.Value));

        using var cleanup = await ConnectionMultiplexer.ConnectAsync(dependencies.RedisConnectionString);
        var cleanupDatabase = cleanup.GetDatabase();
        await cleanupDatabase.KeyDeleteAsync(first);
        await cleanupDatabase.KeyDeleteAsync(second);
    }

    private RedisOptions Options() => new()
    {
        ConnectionString = dependencies.RedisConnectionString,
        ConnectionTimeoutMilliseconds = 250,
        OperationTimeoutMilliseconds = 250
    };

    private string Key() => RedisKeyBuilder.BuildRevocationKey($"t016_{dependencies.RunId}", RedisKeyBuilder.Issuer, Jti());

    private static async Task<(bool Completed, string? Value)> ReadAsync(IDatabase database, RedisKey key)
    {
        try
        {
            return (true, await database.StringGetAsync(key));
        }
        catch (Exception exception) when (exception is RedisException or ObjectDisposedException)
        {
            return (false, null);
        }
    }

    private static string Jti() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
