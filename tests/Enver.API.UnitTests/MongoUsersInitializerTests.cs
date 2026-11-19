using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Enver.API.UnitTests;

public sealed class MongoUsersInitializerTests
{
    [Fact]
    public void Definition_has_exact_key_unique_simple_and_no_extra_options()
    {
        var definition = MongoUsersInitializer.CreateIndexDefinition();
        var keys = definition.Keys.Render(new RenderArgs<UserDocument>(BsonSerializer.LookupSerializer<UserDocument>(), BsonSerializer.SerializerRegistry));
        Assert.True(keys.Equals(new BsonDocument("normalizedEmail", 1)));
        Assert.Equal("normalizedEmail_1", definition.Options.Name);
        Assert.True(definition.Options.Unique);
        Assert.Equal(Collation.Simple, definition.Options.Collation);
        Assert.Null(definition.Options.Sparse);
        Assert.Null(definition.Options.Hidden);
        Assert.Null(definition.Options.ExpireAfter);
        Assert.Null(definition.Options.Background);
    }

    [Theory]
    [InlineData("other-name")]
    [InlineData("explicit-simple")]
    [InlineData("explicit-false")]
    public void Semantically_equivalent_indexes_are_accepted_independent_of_name(string scenario)
    {
        var definition = ValidIndex();
        definition["name"] = "different_name";
        if (scenario == "explicit-simple") definition["collation"] = new BsonDocument("locale", "simple");
        if (scenario == "explicit-false") { definition["sparse"] = false; definition["hidden"] = false; }
        Assert.True(MongoUsersInitializer.IsCompatible(definition));
    }

    [Theory]
    [InlineData("key")]
    [InlineData("descending")]
    [InlineData("compound")]
    [InlineData("unique")]
    [InlineData("sparse")]
    [InlineData("partial")]
    [InlineData("collation")]
    [InlineData("ttl")]
    [InlineData("hidden")]
    [InlineData("building")]
    public void Incompatible_definitions_never_pass(string scenario)
    {
        var definition = ValidIndex();
        switch (scenario)
        {
            case "key": definition["key"] = new BsonDocument("other", 1); break;
            case "descending": definition["key"] = new BsonDocument("normalizedEmail", -1); break;
            case "compound": definition["key"] = new BsonDocument { { "normalizedEmail", 1 }, { "status", 1 } }; break;
            case "unique": definition.Remove("unique"); break;
            case "sparse": definition["sparse"] = true; break;
            case "partial": definition["partialFilterExpression"] = new BsonDocument("status", "Active"); break;
            case "collation": definition["collation"] = new BsonDocument { { "locale", "en" }, { "strength", 2 } }; break;
            case "ttl": definition["expireAfterSeconds"] = 0; break;
            case "hidden": definition["hidden"] = true; break;
            case "building": definition["buildUUID"] = BsonNull.Value; break;
        }
        Assert.False(MongoUsersInitializer.IsCompatible(definition));
    }

    [Fact]
    public async Task Already_ready_is_idempotent_and_never_creates_or_repairs()
    {
        using var h = new Mongo024Harness();
        var lists = 0;
        h.List = _ => { lists++; return Cursor(ValidIndex("another_name")); };
        h.Create = _ => throw new InvalidOperationException("No crear si existe índice válido.");
        var initializer = new MongoUsersInitializer(h.Database, h.Options);
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(MongoUsersInitializationStatus.Ready, await initializer.InitializeAsync(budget));
        Assert.Equal(MongoUsersInitializationStatus.Ready, await initializer.InitializeAsync(budget));
        Assert.Equal(2, lists);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Creation_or_lost_ack_is_followed_by_effective_verification(bool lostAck)
    {
        using var h = new Mongo024Harness();
        var lists = 0;
        var creates = 0;
        h.List = _ => ++lists == 1 ? Cursor() : Cursor(ValidIndex());
        h.Create = _ => { creates++; return lostAck ? Task.FromException<string>(new TimeoutException("private")) : Task.FromResult("normalizedEmail_1"); };
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(MongoUsersInitializationStatus.Ready, await new MongoUsersInitializer(h.Database, h.Options).InitializeAsync(budget));
        Assert.Equal(2, lists);
        Assert.Equal(1, creates);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Missing_or_incompatible_state_cannot_be_ready_and_no_repairs_occur(bool incompatible)
    {
        using var h = new Mongo024Harness();
        var creates = 0;
        var bad = ValidIndex();
        bad["unique"] = false;
        h.List = _ => incompatible ? Cursor(bad) : Cursor();
        h.Create = _ => { creates++; return Task.FromException<string>(new TimeoutException("private")); };
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(incompatible ? MongoUsersInitializationStatus.Incompatible : MongoUsersInitializationStatus.Unavailable,
            await new MongoUsersInitializer(h.Database, h.Options).InitializeAsync(budget));
        Assert.Equal(incompatible ? 0 : 1, creates);
    }

    [Fact]
    public async Task Name_collision_with_wrong_field_fails_without_create()
    {
        using var h = new Mongo024Harness();
        var wrong = ValidIndex();
        wrong["key"] = new BsonDocument("fullName", 1);
        h.List = _ => Cursor(wrong);
        h.Create = _ => throw new InvalidOperationException("No reparar.");
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(MongoUsersInitializationStatus.Incompatible, await new MongoUsersInitializer(h.Database, h.Options).InitializeAsync(budget));
    }

    [Fact]
    public async Task Pre_cancelled_or_failed_inspection_is_unavailable()
    {
        using var h = new Mongo024Harness();
        h.List = _ => throw new TimeoutException("private");
        using var budget = new RequestBudget(8000, default);
        var initializer = new MongoUsersInitializer(h.Database, h.Options);
        Assert.Equal(MongoUsersInitializationStatus.Unavailable, await initializer.InitializeAsync(budget));
        using var client = new CancellationTokenSource();
        client.Cancel();
        h.List = _ => throw new InvalidOperationException("No inspeccionar tras cancelación.");
        using var cancelled = new RequestBudget(8000, client.Token);
        Assert.Equal(MongoUsersInitializationStatus.Unavailable, await initializer.InitializeAsync(cancelled));
    }

    internal static BsonDocument ValidIndex(string name = "normalizedEmail_1") => new()
    {
        { "v", 2 }, { "name", name }, { "key", new BsonDocument("normalizedEmail", 1) }, { "unique", true }
    };
    private static Task<IAsyncCursor<BsonDocument>> Cursor(params BsonDocument[] indexes) =>
        Task.FromResult<IAsyncCursor<BsonDocument>>(new Mongo024Cursor<BsonDocument>(indexes));
}
