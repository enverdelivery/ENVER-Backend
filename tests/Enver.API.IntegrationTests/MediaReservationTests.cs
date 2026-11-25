using Enver.API.Features.Media;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.Testing;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class MediaReservationTests(AuthenticationDependenciesFixture fixture)
{
    [Fact]
    public Task Mongo_enforces_fixed_window_across_two_clients_and_stores_only_pending_metadata() => Mongo024IntegrationScope.SafelyAsync(async () =>
    {
        await using var scope = new Mongo024IntegrationScope(fixture);
        using var peer = new Mongo026Actor(scope);
        await Mongo026Evidence.WarmAsync(scope.Client);
        await Mongo026Evidence.WarmAsync(peer.Client);
        var a = new MongoMediaReservationStore(scope.Database, scope.Options);
        var b = new MongoMediaReservationStore(peer.Database, scope.Options);
        var now = DateTime.UtcNow;
        var start = now.Date.AddMinutes(Math.Floor(now.TimeOfDay.TotalMinutes / 10) * 10);
        var owner = Guid.NewGuid();
        async Task<QuotaOutcome> Charge(MongoMediaReservationStore store, Guid id, DateTime window)
        {
            using var budget = new RequestBudget(8000, default);
            return await store.ChargeAsync(id, window, window.AddMinutes(10), 10, budget);
        }
        foreach (var store in new[] { a, b }) Assert.Equal(QuotaOutcome.Granted, await Charge(store, Guid.NewGuid(), start));
        for (var i = 0; i < 10; i++) Assert.Equal(QuotaOutcome.Granted, await Charge(a, owner, start));
        Assert.Equal(QuotaOutcome.Exceeded, await Charge(b, owner, start));
        Assert.Equal(QuotaOutcome.Granted, await Charge(a, owner, start.AddMinutes(10)));

        var concurrent = Guid.NewGuid();
        var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(i => Charge(i % 2 == 0 ? a : b, concurrent, start)));
        Assert.Equal(10, results.Count(x => x == QuotaOutcome.Granted));
        Assert.Equal(2, results.Count(x => x == QuotaOutcome.Exceeded));
        var quota = scope.Database.GetCollection<BsonDocument>(MongoMediaReservationStore.Quotas);
        using var indexes = await quota.Indexes.ListAsync();
        Assert.Contains(await indexes.ToListAsync(), x => x["name"].AsString == "quota_expiry" && x["expireAfterSeconds"].ToInt64() == 0);

        using var reservationBudget = new RequestBudget(8000, default);
        var id = Guid.NewGuid();
        var key = $"staging/orders/{owner:D}/{id:D}.png";
        Assert.True(await a.ReserveAsync(new(id, owner, key, "image/png", MediaStatus.Pending, now, now.AddSeconds(300)), reservationBudget));
        var reservations = peer.Database.GetCollection<BsonDocument>(MongoMediaReservationStore.Reservations);
        var raw = await reservations.Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();
        Assert.Equal(id, raw["_id"].AsBsonBinaryData.ToGuid());
        Assert.Equal(owner, raw["ownerId"].AsBsonBinaryData.ToGuid());
        Assert.Equal(key, raw["objectKey"].AsString);
        Assert.Equal("pending", raw["status"].AsString);
        Assert.Equal(300000, raw["uploadExpiresAt"].AsBsonDateTime.MillisecondsSinceEpoch - raw["createdAt"].AsBsonDateTime.MillisecondsSinceEpoch);
        Assert.Equal(9, raw.ElementCount);
    });
}
