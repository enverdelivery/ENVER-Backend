using Enver.API.Features.Authentication.Application.Quotas;
using Enver.Testing;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisAuthenticationQuotaCapacityTests(AuthenticationDependenciesFixture fixture)
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
        await s.FillRegisterAsync(2499);
        var requests = Enumerable.Range(3000, 8).Select(i => s.Request(i)).ToArray();
        var results = await Task.WhenAll(requests.Select((r, i) => s.SendAsync(r, i % 2 == 0 ? s.Store : s.SecondStore)));
        Assert.Single(results, r => r.Status == AuthenticationQuotaStoreStatus.Admitted);
        Assert.Equal(7, results.Count(r => r.Status == AuthenticationQuotaStoreStatus.Unavailable));
        Assert.Equal(5000, await s.CountAsync());
        Assert.Equal(5000, await s.Database.SortedSetLengthAsync(s.Configuration.Keys.Registry));
        for (var i = 0; i < requests.Length; i++)
            foreach (var scope in requests[i].Scopes)
                Assert.Equal(results[i].Status == AuthenticationQuotaStoreStatus.Admitted, await s.Database.KeyExistsAsync(s.Key(requests[i], scope)));
    }
}
