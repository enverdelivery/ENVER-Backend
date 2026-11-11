using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisAuthenticationQuotaStoreTests(AuthenticationDependenciesFixture fixture)
{
    [Fact]
    public async Task Each_bucket_admits_one_event_and_multi_scope_uses_same_attempt_and_TIME()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var request = s.Request(operation: AuthenticationOperation.Login);
        var previousLast = (long)await s.Database.HashGetAsync(s.Configuration.Keys.Metadata, "last");
        var result = await s.RawAsync(request);
        Assert.Equal("admitted", result.Status);
        Assert.True(result.NowMicroseconds >= previousLast);
        Assert.Equal(result.NowMicroseconds, (long)await s.Database.HashGetAsync(s.Configuration.Keys.Metadata, "last"));
        foreach (var scope in request.Scopes)
        {
            var events = await s.Database.SortedSetRangeByRankWithScoresAsync(s.Key(request, scope));
            Assert.Single(events);
            Assert.Equal(request.InternalAttemptId, events[0].Element.ToString());
            Assert.Equal((double)result.NowMicroseconds, events[0].Score);
            Assert.True(await s.Database.KeyTimeToLiveAsync(s.Key(request, scope)) > TimeSpan.Zero);
        }
        Assert.Equal(3, await s.CountAsync());
    }

    [Fact]
    public async Task One_blocker_has_no_partial_events_or_new_partitions_and_rejection_does_not_extend_TTL()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        for (var i = 0; i < 3; i++) Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(s.Request())).Status);
        var request = s.Request(identities: new Dictionary<AuthenticationQuotaScopeType, int> { [AuthenticationQuotaScopeType.Email] = 10, [AuthenticationQuotaScopeType.Origin] = 999 });
        var email = request.Scopes.Single(x => x.ScopeType == AuthenticationQuotaScopeType.Email);
        var origin = request.Scopes.Single(x => x.ScopeType == AuthenticationQuotaScopeType.Origin);
        var expiry = await s.Database.KeyExpireTimeAsync(s.Key(request, email));
        var result = await s.SendAsync(request, s.SecondStore);
        Assert.Equal(AuthenticationQuotaStoreStatus.Rejected, result.Status);
        Assert.True(result.RetryAfter > TimeSpan.Zero);
        Assert.False(await s.Database.KeyExistsAsync(s.Key(request, origin)));
        Assert.Equal(3, await s.Database.SortedSetLengthAsync(s.Key(request, email)));
        Assert.Equal(expiry, await s.Database.KeyExpireTimeAsync(s.Key(request, email)));
        Assert.Equal(2, await s.CountAsync());
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public async Task RetryAfter_uses_n_minus_limit_event_and_maximum_across_scopes(int count)
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var request = s.Request();
        var now = await s.NowAsync();
        var email = request.Scopes.Single(x => x.ScopeType == AuthenticationQuotaScopeType.Email);
        var origin = request.Scopes.Single(x => x.ScopeType == AuthenticationQuotaScopeType.Origin);
        var emailTimes = Enumerable.Range(0, count).Select(i => now - 800000000 + i * 10000000L).ToArray();
        var originTimes = Enumerable.Range(0, 20).Select(i => now - 500000000 + i * 1000000L).ToArray();
        await s.SeedAsync(request, email, emailTimes);
        await s.SeedAsync(request, origin, originTimes);
        var previousLast = (long)await s.Database.HashGetAsync(s.Configuration.Keys.Metadata, "last");
        var result = await s.RawAsync(request);
        Assert.Equal("rejected", result.Status);
        var expected = Math.Max(emailTimes[count - 3] + 900000000 - result.NowMicroseconds,
            originTimes[0] + 900000000 - result.NowMicroseconds);
        var logicalRetry = (expected + 999999) / 1000000;
        if (result.NowMicroseconds > previousLast) Assert.Equal(logicalRetry, result.RetryAfterSeconds);
        else Assert.True(result.RetryAfterSeconds >= logicalRetry); // Conversión conservadora a espera de pared en rollback.
        Assert.Null(await s.Database.SortedSetScoreAsync(s.Key(request, email), request.InternalAttemptId));
    }

    [Fact]
    public async Task Window_both_sides_and_expiry_release_capacity_with_real_TIME()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture, TimeSpan.FromSeconds(1));
        var initial = s.Request();
        for (var i = 0; i < 3; i++) Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(s.Request())).Status);
        var blocked = await s.RawAsync(s.Request());
        Assert.Equal("rejected", blocked.Status);
        var last = (await s.Database.SortedSetRangeByRankWithScoresAsync(s.Key(initial, initial.Scopes[0]), -1, -1))[0].Score;
        Assert.True(last > blocked.NowMicroseconds - 1000000);
        await s.WaitUntilAsync((long)last + 1000000);
        var admitted = await s.RawAsync(s.Request());
        Assert.Equal("admitted", admitted.Status);
        Assert.True(last <= admitted.NowMicroseconds - 1000000);
        Assert.Equal(2, await s.CountAsync());
        Assert.Equal(1, await s.Database.SortedSetLengthAsync(s.Key(initial, initial.Scopes[0])));
    }

    [Fact]
    public async Task Attempt_collision_never_updates_scores_or_other_scopes()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var request = s.Request();
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(request)).Status);
        var score = await s.Database.SortedSetScoreAsync(s.Key(request, request.Scopes[0]), request.InternalAttemptId);
        Assert.Equal("collision", (await s.RawAsync(request)).Category);
        Assert.Equal(score, await s.Database.SortedSetScoreAsync(s.Key(request, request.Scopes[0]), request.InternalAttemptId));
        Assert.All(await Task.WhenAll(request.Scopes.Select(scope => s.Database.SortedSetLengthAsync(s.Key(request, scope)))), n => Assert.Equal(1, n));
    }

    [Fact]
    public async Task Revision_and_snapshot_mismatch_do_not_mutate()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(s.Request(revision: "forged"))).Status);
        Assert.Equal(0, await s.CountAsync());
        var request = s.Request();
        var before = await s.Database.HashGetAllAsync(s.Configuration.Keys.Metadata);
        var result = await s.RawAsync(request, invocation => invocation.Arguments[4] = "divergent-config");
        Assert.Equal("unavailable", result.Status);
        Assert.Equal(before.OrderBy(x => x.Name.ToString()), (await s.Database.HashGetAllAsync(s.Configuration.Keys.Metadata)).OrderBy(x => x.Name.ToString()));
    }

    [Theory]
    [InlineData(1, "2")]
    [InlineData(7, "6000")]
    [InlineData(11, "invalid-attempt")]
    [InlineData(13, "1")]
    [InlineData(16, "9007199254740992")]
    public async Task Invalid_script_arguments_do_not_mutate(int index, string value)
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var request = s.Request();
        var before = await s.Database.HashGetAllAsync(s.Configuration.Keys.Metadata);
        Assert.Equal("unavailable", (await s.RawAsync(request, invocation => invocation.Arguments[index] = value)).Status);
        Assert.Equal(before.OrderBy(x => x.Name.ToString()), (await s.Database.HashGetAllAsync(s.Configuration.Keys.Metadata)).OrderBy(x => x.Name.ToString()));
        foreach (var scope in request.Scopes) Assert.False(await s.Database.KeyExistsAsync(s.Key(request, scope)));
    }

    [Fact]
    public async Task Real_post_dispatch_timeout_is_uncertain_without_a_retry()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var calls = 0;
        var options = new Enver.API.Configuration.RedisOptions { OperationTimeoutMilliseconds = 50 };
        var store = new RedisAuthenticationQuotaStore(s.Connections, options, s.Configuration, RedisAuthenticationQuotaTestScope.Open, RedisAuthenticationQuotaTestScope.Open,
            async (dependency, invocation) =>
            {
                calls++;
                // Pausa finita en el contenedor exclusivo de pruebas; ningún endpoint productivo.
                await s.Database.ExecuteAsync("CLIENT", "PAUSE", "300", "ALL");
                return await s.Database.ScriptEvaluateAsync(RedisAuthenticationQuotaScriptProtocol.ScriptText, invocation.Keys, invocation.Arguments,
                    CommandFlags.DemandMaster).WaitAsync(dependency.Token);
            }, () => ValueTask.FromResult(Array.Empty<string>()));
        var request = s.Request();
        using var budget = RedisAuthenticationQuotaTestScope.Budget();
        Assert.Equal(AuthenticationQuotaStoreStatus.Uncertain, (await store.EvaluateAndConsumeAsync(request, budget)).Status);
        Assert.Equal(1, calls);
        // Solo diagnóstico IT posterior, nunca read-back del adapter para transformar el resultado incierto.
        await s.NowAsync();
        foreach (var scope in request.Scopes) Assert.Equal(1, await s.Database.SortedSetLengthAsync(s.Key(request, scope)));
    }
}
