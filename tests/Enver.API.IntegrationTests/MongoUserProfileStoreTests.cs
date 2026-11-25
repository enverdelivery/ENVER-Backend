using Enver.API.Features.Users.Application;
using Enver.API.Features.Users.Core;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.Testing;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class MongoUserProfileStoreTests(AuthenticationDependenciesFixture fixture)
{
    [Fact]
    public Task Concurrent_first_profile_writes_preserve_fields_under_one_uuid() => Mongo024IntegrationScope.SafelyAsync(async () =>
    {
        await using var scope = new Mongo024IntegrationScope(fixture);
        using var peer = new Mongo026Actor(scope);
        await Mongo026Evidence.WarmAsync(scope.Client);
        await Mongo026Evidence.WarmAsync(peer.Client);
        var store = new MongoUserProfileStore(scope.Database, scope.Options);
        var other = new MongoUserProfileStore(peer.Database, scope.Options);
        var id = Guid.NewGuid();
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(ProfileStoreStatus.Missing, (await store.ReadAsync(id, budget)).Status);
        using (var collections = await scope.Database.ListCollectionNamesAsync())
            Assert.DoesNotContain(MongoUserProfileStore.CollectionName, await collections.ToListAsync());

        using var a = new RequestBudget(8000, default);
        using var b = new RequestBudget(8000, default);
        var writes = await Task.WhenAll(store.UpdateAsync(id, new("Visible", true, "+527731234567"), a),
            other.UpdateAsync(id, new(null, false, null, ActiveRole.Rescatista), b));
        Assert.All(writes, value => Assert.Equal(ProfileStoreStatus.Confirmed, value.Status));
        using var check = new RequestBudget(8000, default);
        Assert.Equal(new UserProfile("Visible", "+527731234567", ActiveRole.Rescatista), (await other.ReadAsync(id, check)).Profile);
        var raw = Assert.Single(await scope.Database.GetCollection<BsonDocument>(MongoUserProfileStore.CollectionName)
            .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync());
        Assert.Equal(BsonBinarySubType.UuidStandard, raw["_id"].AsBsonBinaryData.SubType);
        Assert.Equal(new[] { "_id", "activeRole", "fullName", "phone" }, raw.Names.Order());
    });
}
