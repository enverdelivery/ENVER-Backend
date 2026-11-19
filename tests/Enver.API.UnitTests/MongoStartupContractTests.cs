using Enver.API.Configuration;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;

namespace Enver.API.UnitTests;

/// <summary>Proxies cerrados sin MongoClient ni sockets: cualquier operación no prevista falla.</summary>
public sealed class MongoStartupContractTests
{
    [Theory]
    [InlineData("compatible")]
    [InlineData("missing")]
    [InlineData("namespace-missing")]
    [InlineData("incompatible")]
    [InlineData("duplicate-data")]
    [InlineData("lost-ack")]
    [InlineData("not-visible-after-create")]
    [InlineData("inspection-failed")]
    public async Task Initializer_only_lists_and_conditionally_creates_one_index_then_reinspects(string scenario)
    {
        var calls = new List<string>();
        var lists = 0;
        var creates = 0;
        var indexes = Mongo024Proxy.Create<IMongoIndexManager<UserDocument>>((method, args) =>
        {
            calls.Add(method.Name);
            switch (method.Name)
            {
                case "ListAsync":
                    lists++;
                    if (scenario == "inspection-failed") throw new TimeoutException("LOCAL-CANARY");
                    if (scenario == "namespace-missing" && lists == 1) throw Error(26);
                    var definition = MongoUsersInitializerTests.ValidIndex();
                    if (scenario == "incompatible") definition["unique"] = false;
                    var present = scenario is "compatible" or "incompatible" || lists > 1 && scenario != "not-visible-after-create";
                    return Task.FromResult<IAsyncCursor<BsonDocument>>(new Mongo024Cursor<BsonDocument>(present ? [definition] : []));
                case "CreateOneAsync":
                    creates++;
                    var model = Assert.IsType<CreateIndexModel<UserDocument>>(args[0]);
                    Assert.True(model.Options.Unique); Assert.Equal("normalizedEmail_1", model.Options.Name);
                    if (scenario == "duplicate-data") throw Error(11000);
                    if (scenario == "lost-ack") throw new TimeoutException("LOCAL-CANARY");
                    return Task.FromResult(MongoUsersInitializer.IndexName);
                default: throw new Xunit.Sdk.XunitException("Unexpected index operation.");
            }
        });
        var options = new MongoDbOptions
        {
            DatabaseName = "enver_local", UsersCollectionName = "users", OperationTimeoutMilliseconds = 2000,
            ConnectionTimeoutMilliseconds = 2000, WriteConcernCandidate = "majority", ReadConcernCandidate = "majority", ReadPreference = "primary"
        };
        var client = Mongo024Proxy.Create<IMongoClient>((method, _) => method.Name == "get_Settings"
            ? new MongoClientSettings { RetryWrites = false } : throw new Xunit.Sdk.XunitException("No client operations permitted."));
        var collection = Mongo024Proxy.Create<IMongoCollection<UserDocument>>((method, _) => method.Name == "get_Indexes"
            ? indexes : throw new Xunit.Sdk.XunitException("No document operations permitted."));
        var database = Mongo024Proxy.Create<IMongoDatabase>((method, args) => method.Name switch
        {
            "get_DatabaseNamespace" => new DatabaseNamespace(options.DatabaseName),
            "get_Settings" => new MongoDatabaseSettings { ReadConcern = ReadConcern.Majority, ReadPreference = ReadPreference.Primary,
                WriteConcern = new WriteConcern("majority", wTimeout: TimeSpan.FromMilliseconds(2000), journal: true) },
            "get_Client" => client,
            "GetCollection" => (string)args[0]! == "users" ? collection : throw new Xunit.Sdk.XunitException("Unexpected collection."),
            _ => throw new Xunit.Sdk.XunitException("No database commands or explicit collection creation permitted.")
        });
        var initializer = new MongoUsersInitializer(database, options);
        using var budget = new RequestBudget(8000, default);
        var result = await initializer.InitializeAsync(budget);
        var expected = scenario switch
        {
            "incompatible" or "duplicate-data" => MongoUsersInitializationStatus.Incompatible,
            "not-visible-after-create" or "inspection-failed" => MongoUsersInitializationStatus.Unavailable,
            _ => MongoUsersInitializationStatus.Ready
        };
        Assert.Equal(expected, result);
        Assert.Equal(scenario is "compatible" or "incompatible" or "inspection-failed" ? 0 : 1, creates);
        Assert.Equal(creates == 0 ? 1 : 2, lists);
        Assert.All(calls, call => Assert.Contains(call, new[] { "ListAsync", "CreateOneAsync" }));
        if (result == MongoUsersInitializationStatus.Ready)
        {
            // Segundo inicio lógico: índice compatible ya visible, cero creates adicionales.
            using var secondBudget = new RequestBudget(8000, default);
            var before = creates;
            Assert.Equal(MongoUsersInitializationStatus.Ready, await initializer.InitializeAsync(secondBudget));
            Assert.Equal(before, creates);
        }
    }

    private static MongoCommandException Error(int code) => new(
        new ConnectionId(new ServerId(new ClusterId(), new System.Net.DnsEndPoint("localhost", 1))), "LOCAL-CANARY",
        new BsonDocument("createIndexes", "users"), new BsonDocument { { "ok", 0 }, { "code", code }, { "errmsg", "LOCAL-CANARY" } });
}
