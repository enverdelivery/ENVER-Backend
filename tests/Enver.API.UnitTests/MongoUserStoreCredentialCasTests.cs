using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;

namespace Enver.API.UnitTests;

public sealed class MongoUserStoreCredentialCasTests
{
    private static readonly DateTime Instant = new(2026, 11, 10, 13, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Contract_has_exact_signature_and_safe_results()
    {
        var method = typeof(IUserStore).GetMethod(nameof(IUserStore.CompareExchangePasswordHashAsync))!;
        Assert.Equal(typeof(ValueTask<UserCredentialUpdateResult>), method.ReturnType);
        Assert.Equal(new[] { typeof(Guid), typeof(string), typeof(UserStatus), typeof(string), typeof(DateTime), typeof(RequestBudget) },
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Throws<ArgumentException>(() => new UserCredentialUpdateResult((UserCredentialUpdateStatus)99));
        Assert.Throws<ArgumentException>(() => new UserCredentialUpdateResult(UserCredentialUpdateStatus.Updated, BudgetCancellationCause.Client));
        Assert.Throws<ArgumentException>(() => new UserCredentialUpdateResult(UserCredentialUpdateStatus.Conflict, BudgetCancellationCause.RequestBudget));
        Assert.Throws<ArgumentException>(() => new UserCredentialUpdateResult(UserCredentialUpdateStatus.Uncertain, (BudgetCancellationCause)99));
        Assert.True(new UserCredentialUpdateResult(UserCredentialUpdateStatus.Uncertain).ResultUncertain);
        Assert.False(new UserCredentialUpdateResult(UserCredentialUpdateStatus.Updated).ResultUncertain);
    }

    [Fact]
    public async Task Exact_three_condition_filter_two_field_update_and_binary_no_upsert()
    {
        using var h = new CasHarness();
        using var budget = new RequestBudget(8000, default);
        var id = Guid.NewGuid();
        var result = await h.Store.CompareExchangePasswordHashAsync(id, "synthetic-old", UserStatus.Active,
            "synthetic-new", Instant.AddTicks(12345), budget);
        Assert.Equal(UserCredentialUpdateStatus.Updated, result.Status);
        var render = new RenderArgs<UserDocument>(BsonSerializer.LookupSerializer<UserDocument>(), BsonSerializer.SerializerRegistry);
        var filter = h.Filter!.Render(render);
        Assert.Equal(3, filter.ElementCount);
        Assert.True(filter["_id"].AsBsonBinaryData.ToGuid(GuidRepresentation.Standard) == id);
        Assert.True(filter["passwordHash"].AsString == "synthetic-old");
        Assert.True(filter["status"].AsString == "Active");
        var update = h.UpdateDefinition!.Render(render).AsBsonDocument;
        Assert.Single(update);
        var fields = update["$set"].AsBsonDocument;
        Assert.Equal(new[] { "passwordHash", "updatedAt" }, fields.Names.Order());
        Assert.True(fields["passwordHash"].AsString == "synthetic-new");
        Assert.Equal(Instant.AddMilliseconds(1), fields["updatedAt"].ToUniversalTime());
        Assert.False(h.Options!.IsUpsert);
        Assert.Equal(Collation.Simple, h.Options.Collation);
        Assert.Equal(1, h.Calls);
        Assert.Equal(0, h.Reads);
        Assert.False(result.ToString().Contains("synthetic-", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(1, 1, UserCredentialUpdateStatus.Updated)]
    [InlineData(0, 0, UserCredentialUpdateStatus.Conflict)]
    [InlineData(1, 0, UserCredentialUpdateStatus.Unavailable)]
    [InlineData(0, 1, UserCredentialUpdateStatus.Uncertain)]
    [InlineData(2, 1, UserCredentialUpdateStatus.Uncertain)]
    public async Task Acknowledged_counts_are_classified_without_readback(long matched, long modified, UserCredentialUpdateStatus expected)
    {
        using var h = new CasHarness();
        h.Update = _ => Task.FromResult<UpdateResult>(new UpdateResult.Acknowledged(matched, modified, null));
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(expected, (await Apply(h, budget)).Status);
        Assert.Equal(1, h.Calls);
        Assert.Equal(0, h.Reads);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Missing_ack_or_modified_count_is_uncertain(bool missingCount)
    {
        using var h = new CasHarness();
        h.Update = _ => Task.FromResult<UpdateResult>(missingCount
            ? new UpdateResult.Acknowledged(1, null, null) : UpdateResult.Unacknowledged.Instance);
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(UserCredentialUpdateStatus.Uncertain, (await Apply(h, budget)).Status);
    }

    [Theory]
    [InlineData("lost-ack")]
    [InlineData("write-concern")]
    [InlineData("rejection")]
    [InlineData("write-rejection")]
    public async Task Exceptions_distinguish_conclusive_rejection_from_uncertainty(string scenario)
    {
        using var h = new CasHarness();
        var connection = new ConnectionId(new ServerId(new ClusterId(), new System.Net.DnsEndPoint("localhost", 1)));
        Exception error = scenario switch
        {
            "write-concern" => new MongoWriteConcernException(connection, "synthetic-private", new WriteConcernResult(
                new BsonDocument { { "ok", 1 }, { "n", 1 }, { "writeConcernError", new BsonDocument { { "code", 64 }, { "errmsg", "synthetic-private" } } } })),
            "rejection" => new MongoCommandException(connection, "synthetic-private", new BsonDocument("update", "users"),
                new BsonDocument { { "ok", 0 }, { "code", 13 } }),
            "write-rejection" => new MongoWriteException(connection,
                (WriteError)Activator.CreateInstance(typeof(WriteError), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                    null, new object[] { ServerErrorCategory.Uncategorized, 121, "synthetic-private", new BsonDocument() }, null)!, null, null),
            _ => new TimeoutException("synthetic-private")
        };
        h.Update = _ => Task.FromException<UpdateResult>(error);
        using var budget = new RequestBudget(8000, default);
        var result = await Apply(h, budget);
        Assert.Equal(scenario.Contains("rejection", StringComparison.Ordinal)
            ? UserCredentialUpdateStatus.Unavailable : UserCredentialUpdateStatus.Uncertain, result.Status);
        Assert.DoesNotContain("synthetic-private", result.ToString());
        Assert.Equal(1, h.Calls);
        Assert.Equal(0, h.Reads);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Client_cancellation_before_and_after_possible_dispatch(bool after)
    {
        using var h = new CasHarness();
        using var client = new CancellationTokenSource();
        if (!after) client.Cancel();
        h.Update = _ => { client.Cancel(); return Task.FromResult<UpdateResult>(new UpdateResult.Acknowledged(1, 1, null)); };
        using var budget = new RequestBudget(8000, client.Token);
        var result = await Apply(h, budget);
        Assert.Equal(after ? UserCredentialUpdateStatus.Uncertain : UserCredentialUpdateStatus.Unavailable, result.Status);
        Assert.Equal(BudgetCancellationCause.Client, result.CancellationCause);
        Assert.Equal(after ? 1 : 0, h.Calls);
    }

    [Fact]
    public async Task Expired_budget_is_not_restarted()
    {
        using var h = new CasHarness();
        var clock = new Mongo024Clock();
        using var budget = new RequestBudget(8000, default, clock);
        clock.Advance(8001);
        var result = await Apply(h, budget);
        Assert.Equal(UserCredentialUpdateStatus.Unavailable, result.Status);
        Assert.Equal(BudgetCancellationCause.RequestBudget, result.CancellationCause);
        Assert.Equal(0, h.Calls);
    }

    [Fact]
    public async Task Dependency_deadline_bounds_pending_update()
    {
        using var h = new CasHarness(100);
        h.Update = async token => { await Task.Delay(Timeout.Infinite, token); return new UpdateResult.Acknowledged(1, 1, null); };
        using var budget = new RequestBudget(8000, default);
        var result = await Apply(h, budget);
        Assert.Equal(UserCredentialUpdateStatus.Uncertain, result.Status);
        Assert.Equal(BudgetCancellationCause.DependencyTimeout, result.CancellationCause);
        Assert.Equal(1, h.Calls);
        Assert.Equal(0, h.Reads);
    }

    [Theory]
    [InlineData("utc")]
    [InlineData("status")]
    [InlineData("id")]
    [InlineData("old")]
    [InlineData("new")]
    public async Task Invalid_internal_arguments_do_not_dispatch(string scenario)
    {
        using var h = new CasHarness();
        using var budget = new RequestBudget(8000, default);
        var result = await h.Store.CompareExchangePasswordHashAsync(scenario == "id" ? Guid.Empty : Guid.NewGuid(),
            scenario == "old" ? null! : "synthetic-old", scenario == "status" ? (UserStatus)99 : UserStatus.Active,
            scenario == "new" ? null! : "synthetic-new", scenario == "utc" ? DateTime.SpecifyKind(Instant, DateTimeKind.Unspecified) : Instant, budget);
        Assert.Equal(UserCredentialUpdateStatus.Unavailable, result.Status);
        Assert.Equal(0, h.Calls);
    }

    private static ValueTask<UserCredentialUpdateResult> Apply(CasHarness h, RequestBudget budget) =>
        h.Store.CompareExchangePasswordHashAsync(Guid.NewGuid(), "synthetic-old", UserStatus.Active, "synthetic-new", Instant, budget);

    private sealed class CasHarness : IDisposable
    {
        private readonly Mongo024Harness owner;
        public CasHarness(int timeout = 2000)
        {
            owner = new Mongo024Harness(timeout);
            var collection = owner.Database.GetCollection<UserDocument>("users");
            var proxy = Mongo024Proxy.Create<IMongoCollection<UserDocument>>((method, args) =>
            {
                if (method.Name == "UpdateOneAsync")
                {
                    Calls++;
                    Filter = (FilterDefinition<UserDocument>)args[0]!;
                    UpdateDefinition = (UpdateDefinition<UserDocument>)args[1]!;
                    Options = (UpdateOptions)args[2]!;
                    return Update((CancellationToken)args[^1]!);
                }
                if (method.Name == "FindAsync") Reads++;
                return Mongo024Proxy.Forward(method, collection, args);
            });
            var database = Mongo024Proxy.Create<IMongoDatabase>((method, args) => method.Name == "GetCollection"
                ? proxy : Mongo024Proxy.Forward(method, owner.Database, args));
            Store = new MongoUserStore(database, owner.Options);
        }
        public MongoUserStore Store { get; }
        public int Calls { get; private set; }
        public int Reads { get; private set; }
        public FilterDefinition<UserDocument>? Filter { get; private set; }
        public UpdateDefinition<UserDocument>? UpdateDefinition { get; private set; }
        public UpdateOptions? Options { get; private set; }
        public Func<CancellationToken, Task<UpdateResult>> Update { get; set; } = _ =>
            Task.FromResult<UpdateResult>(new UpdateResult.Acknowledged(1, 1, null));
        public void Dispose() => owner.Dispose();
    }
}
