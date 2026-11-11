using System.Reflection;
using System.Runtime.ExceptionServices;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.Clusters;

namespace Enver.API.UnitTests;

public sealed class MongoUserStoreTests
{
    [Fact]
    public void Results_require_consistent_payloads_and_do_not_print_sensitive_data()
    {
        var user = Mongo024Harness.User();
        var found = new UserReadResult(UserReadStatus.Found, user);
        Assert.Same(user, found.User);
        Assert.False(found.ToString().Contains(user.NormalizedEmail, StringComparison.Ordinal));
        Assert.False(found.ToString().Contains(user.PasswordHash, StringComparison.Ordinal));
        Assert.Throws<ArgumentException>(() => new UserReadResult(UserReadStatus.Found));
        Assert.Throws<ArgumentException>(() => new UserReadResult(UserReadStatus.NotFound, user));
        Assert.Throws<ArgumentException>(() => new UserReadResult((UserReadStatus)99));
        Assert.Throws<ArgumentException>(() => new UserInsertResult((UserInsertStatus)99));
        Assert.Throws<ArgumentException>(() => new UserInsertResult(UserInsertStatus.Confirmed, BudgetCancellationCause.Client));
        Assert.False(new UserInsertResult(UserInsertStatus.Confirmed).ResultUncertain);
        Assert.True(new UserInsertResult(UserInsertStatus.Uncertain).ResultUncertain);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Lookup_uses_backend_equality_simple_collation_and_does_not_hide_disabled(bool byId)
    {
        using var h = new Mongo024Harness();
        var original = Mongo024Harness.User(UserStatus.Disabled);
        h.Find = _ => Task.FromResult<IAsyncCursor<UserDocument>>(new Mongo024Cursor<UserDocument>([UserDocument.FromUser(original)]));
        using var budget = new RequestBudget(8000, default);
        var result = byId ? await h.Store.FindByIdAsync(original.Id, budget)
            : await h.Store.FindByNormalizedEmailAsync(original.NormalizedEmail, budget);
        Assert.Equal(UserReadStatus.Found, result.Status);
        Assert.Equal(original.Id, result.User!.Id);
        Assert.Equal(UserStatus.Disabled, result.User.Status);
        var filter = h.LastFilter!.Render(new RenderArgs<UserDocument>(BsonSerializer.LookupSerializer<UserDocument>(), BsonSerializer.SerializerRegistry));
        Assert.Single(filter);
        Assert.True(filter.Contains(byId ? "_id" : "normalizedEmail"));
        Assert.True(byId ? filter["_id"].AsBsonBinaryData.ToGuid(GuidRepresentation.Standard) == original.Id
            : filter["normalizedEmail"].AsString == original.NormalizedEmail);
        Assert.Equal(Collation.Simple, h.LastFindOptions!.Collation);
        Assert.Equal(1, h.FindCalls);
    }

    [Fact]
    public async Task Only_completed_empty_query_is_not_found_and_multiple_matches_are_unavailable()
    {
        using var h = new Mongo024Harness();
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(UserReadStatus.NotFound, (await h.Store.FindByIdAsync(Guid.NewGuid(), budget)).Status);
        h.Find = _ => Task.FromResult<IAsyncCursor<UserDocument>>(new Mongo024Cursor<UserDocument>(
            [UserDocument.FromUser(Mongo024Harness.User()), UserDocument.FromUser(Mongo024Harness.User())]));
        Assert.Equal(UserReadStatus.Unavailable, (await h.Store.FindByNormalizedEmailAsync("synthetic@example.invalid", budget)).Status);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Read_failure_in_find_or_cursor_is_never_not_found(bool cursor)
    {
        using var h = new Mongo024Harness();
        h.Find = _ => cursor
            ? Task.FromResult<IAsyncCursor<UserDocument>>(new Mongo024Cursor<UserDocument>([], fail: true))
            : Task.FromException<IAsyncCursor<UserDocument>>(new TimeoutException("synthetic-private-diagnostic"));
        using var budget = new RequestBudget(8000, default);
        var result = await h.Store.FindByIdAsync(Guid.NewGuid(), budget);
        Assert.Equal(UserReadStatus.Unavailable, result.Status);
        Assert.Null(result.User);
        Assert.DoesNotContain("synthetic-private-diagnostic", result.ToString());
        Assert.Equal(1, h.FindCalls);
    }

    [Fact]
    public async Task Confirmed_insert_preserves_identity_and_explicit_concerns_without_retry()
    {
        using var h = new Mongo024Harness();
        using var budget = new RequestBudget(8000, default);
        var user = Mongo024Harness.User();
        Assert.Equal(UserInsertStatus.Confirmed, (await h.Store.InsertAsync(user, budget)).Status);
        var command = h.LastCommand!;
        Assert.Equal("users", command["insert"].AsString);
        Assert.True(command["documents"][0].AsBsonDocument.Equals(UserDocument.FromUser(user).ToBsonDocument()));
        Assert.True(command["writeConcern"].AsBsonDocument.Equals(new BsonDocument { { "w", "majority" }, { "wtimeout", 2000 }, { "j", true } }));
        Assert.False(command.Contains("txnNumber"));
        Assert.Equal(1, h.InsertCalls);
        Assert.Equal(0, h.FindCalls);
    }

    [Theory]
    [InlineData("email", UserInsertStatus.Conflict)]
    [InlineData("id", UserInsertStatus.Unavailable)]
    [InlineData("missing", UserInsertStatus.Unavailable)]
    [InlineData("compound", UserInsertStatus.Unavailable)]
    [InlineData("other-code", UserInsertStatus.Unavailable)]
    [InlineData("wrong-index", UserInsertStatus.Unavailable)]
    [InlineData("write-concern", UserInsertStatus.Uncertain)]
    [InlineData("no-ack", UserInsertStatus.Uncertain)]
    [InlineData("malformed", UserInsertStatus.Uncertain)]
    public async Task Duplicate_classification_requires_structured_evidence_and_no_ambiguous_ack(string scenario, UserInsertStatus expected)
    {
        using var h = new Mongo024Harness();
        var error = new BsonDocument { { "index", 0 }, { "code", 11000 }, { "errmsg", "synthetic-private-diagnostic" } };
        if (scenario != "missing") error["keyPattern"] = scenario switch
        {
            "id" => new BsonDocument("_id", 1),
            "compound" => new BsonDocument { { "normalizedEmail", 1 }, { "status", 1 } },
            _ => new BsonDocument("normalizedEmail", 1)
        };
        if (scenario == "other-code") error["code"] = 121;
        if (scenario == "wrong-index") error["index"] = 1;
        var response = new BsonDocument { { "ok", 1 }, { "n", 0 }, { "writeErrors", new BsonArray { error } } };
        if (scenario == "write-concern") response["writeConcernError"] = new BsonDocument("code", 64);
        if (scenario == "no-ack") response.Remove("ok");
        if (scenario == "malformed") response["writeErrors"] = "invalid";
        h.Insert = _ => Task.FromResult(response);
        using var budget = new RequestBudget(8000, default);
        var result = await h.Store.InsertAsync(Mongo024Harness.User(), budget);
        Assert.Equal(expected, result.Status);
        Assert.DoesNotContain("synthetic-private-diagnostic", result.ToString());
        Assert.Equal(1, h.InsertCalls);
        Assert.Equal(0, h.FindCalls);
    }

    [Fact]
    public async Task Conclusive_command_rejection_is_unavailable_not_email_conflict()
    {
        using var h = new Mongo024Harness();
        h.Insert = _ => Task.FromException<BsonDocument>(new MongoCommandException(
            new ConnectionId(new ServerId(new ClusterId(), new System.Net.DnsEndPoint("localhost", 1))),
            "synthetic-private-diagnostic", new BsonDocument("insert", "users"), new BsonDocument { { "ok", 0 }, { "code", 13 } }));
        using var budget = new RequestBudget(8000, default);
        Assert.Equal(UserInsertStatus.Unavailable, (await h.Store.InsertAsync(Mongo024Harness.User(), budget)).Status);
        Assert.Equal(1, h.InsertCalls);
    }

    [Fact]
    public async Task Client_cancellation_before_dispatch_is_unavailable_without_operations()
    {
        using var h = new Mongo024Harness();
        using var client = new CancellationTokenSource();
        client.Cancel();
        using var budget = new RequestBudget(8000, client.Token);
        var write = await h.Store.InsertAsync(Mongo024Harness.User(), budget);
        var read = await h.Store.FindByIdAsync(Guid.NewGuid(), budget);
        Assert.Equal(UserInsertStatus.Unavailable, write.Status);
        Assert.Equal(UserReadStatus.Unavailable, read.Status);
        Assert.Equal(BudgetCancellationCause.Client, write.CancellationCause);
        Assert.Equal(BudgetCancellationCause.Client, read.CancellationCause);
        Assert.Equal(0, h.InsertCalls + h.FindCalls);
    }

    [Fact]
    public async Task Expired_request_budget_is_not_restarted_for_another_operation()
    {
        using var h = new Mongo024Harness();
        var time = new Mongo024Clock();
        using var budget = new RequestBudget(8000, default, time);
        Assert.Equal(UserReadStatus.NotFound, (await h.Store.FindByIdAsync(Guid.NewGuid(), budget)).Status);
        time.Advance(8001);
        var result = await h.Store.InsertAsync(Mongo024Harness.User(), budget);
        Assert.Equal(UserInsertStatus.Unavailable, result.Status);
        Assert.Equal(BudgetCancellationCause.RequestBudget, result.CancellationCause);
        Assert.Equal(0, h.InsertCalls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_dispatch_cancellation_or_lost_response_is_uncertain_without_readback(bool cancel)
    {
        using var h = new Mongo024Harness();
        using var client = new CancellationTokenSource();
        h.Insert = token =>
        {
            if (cancel) client.Cancel();
            return Task.FromException<BsonDocument>(cancel ? new OperationCanceledException(token) : new TimeoutException("synthetic-private-diagnostic"));
        };
        using var budget = new RequestBudget(8000, client.Token);
        var result = await h.Store.InsertAsync(Mongo024Harness.User(), budget);
        Assert.Equal(UserInsertStatus.Uncertain, result.Status);
        Assert.Equal(cancel ? BudgetCancellationCause.Client : BudgetCancellationCause.None, result.CancellationCause);
        Assert.Equal(1, h.InsertCalls);
        Assert.Equal(0, h.FindCalls);
    }

    [Fact]
    public async Task Dependency_timeout_bounds_a_pending_write_and_is_uncertain()
    {
        using var h = new Mongo024Harness(100);
        h.Insert = async token => { await Task.Delay(Timeout.Infinite, token); return new BsonDocument(); };
        using var budget = new RequestBudget(8000, default);
        var result = await h.Store.InsertAsync(Mongo024Harness.User(), budget);
        Assert.Equal(UserInsertStatus.Uncertain, result.Status);
        Assert.Equal(BudgetCancellationCause.DependencyTimeout, result.CancellationCause);
        Assert.Equal(1, h.InsertCalls);
    }
}

// Dobles exclusivos de UT: ninguna operación de datos llega a la conexión sintética.
public class Mongo024Proxy : DispatchProxy
{
    public Func<MethodInfo, object?[], object?> Handler { get; set; } = null!;
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Handler(targetMethod!, args ?? []);
    public static T Create<T>(Func<MethodInfo, object?[], object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, Mongo024Proxy>();
        ((Mongo024Proxy)(object)proxy).Handler = handler;
        return proxy;
    }

    public static object? Forward(MethodInfo method, object target, object?[] args)
    {
        try { return method.Invoke(target, args); }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}

internal sealed class Mongo024Cursor<T>(IReadOnlyList<T> values, bool fail = false) : IAsyncCursor<T>
{
    private bool moved;
    public IEnumerable<T> Current => values;
    public bool MoveNext(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (fail) throw new TimeoutException("synthetic-private-diagnostic");
        var next = !moved;
        moved = true;
        return next;
    }
    public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default) => Task.FromResult(MoveNext(cancellationToken));
    public void Dispose() { }
}

internal sealed class Mongo024Clock : TimeProvider
{
    private long ticks;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public override long GetTimestamp() => ticks;
    public void Advance(int milliseconds) => ticks += TimeSpan.FromMilliseconds(milliseconds).Ticks;
}

internal sealed class Mongo024Harness : IDisposable
{
    private readonly MongoClientFactory owner;
    public Mongo024Harness(int timeout = 2000)
    {
        Options = new MongoDbOptions
        {
            ConnectionString = "mongodb://127.0.0.1:1/?directConnection=true", DatabaseName = "unit_t024", UsersCollectionName = "users",
            OperationTimeoutMilliseconds = timeout, ConnectionTimeoutMilliseconds = 100,
            WriteConcernCandidate = "majority", ReadConcernCandidate = "majority", ReadPreference = "primary"
        };
        owner = new MongoClientFactory(Options);
        var database = owner.Client.GetDatabase(Options.DatabaseName);
        var collection = database.GetCollection<UserDocument>("users");
        var indexes = Mongo024Proxy.Create<IMongoIndexManager<UserDocument>>((method, args) => method.Name switch
        {
            "ListAsync" => List((CancellationToken)args[^1]!),
            "CreateOneAsync" => Create((CancellationToken)args[^1]!),
            _ => throw new InvalidOperationException("Operación de índice no autorizada en UT.")
        });
        var users = Mongo024Proxy.Create<IMongoCollection<UserDocument>>((method, args) =>
        {
            if (method.Name == "get_Indexes") return indexes;
            if (method.Name == "FindAsync")
            {
                FindCalls++;
                LastFilter = (FilterDefinition<UserDocument>)args[0]!;
                LastFindOptions = (FindOptions<UserDocument>)args[1]!;
                return Find((CancellationToken)args[^1]!);
            }
            return Mongo024Proxy.Forward(method, collection, args);
        });
        Database = Mongo024Proxy.Create<IMongoDatabase>((method, args) =>
        {
            if (method.Name == "GetCollection") return users;
            if (method.Name == "RunCommandAsync")
            {
                InsertCalls++;
                LastCommand = ((Command<BsonDocument>)args[0]!).Render(BsonSerializer.SerializerRegistry).Document;
                return Insert((CancellationToken)args[^1]!);
            }
            return Mongo024Proxy.Forward(method, database, args);
        });
        Store = new MongoUserStore(Database, Options);
    }

    public MongoDbOptions Options { get; }
    public IMongoDatabase Database { get; }
    public MongoUserStore Store { get; }
    public int FindCalls { get; private set; }
    public int InsertCalls { get; private set; }
    public BsonDocument? LastCommand { get; private set; }
    public FilterDefinition<UserDocument>? LastFilter { get; private set; }
    public FindOptions<UserDocument>? LastFindOptions { get; private set; }
    public Func<CancellationToken, Task<IAsyncCursor<UserDocument>>> Find { get; set; } = _ =>
        Task.FromResult<IAsyncCursor<UserDocument>>(new Mongo024Cursor<UserDocument>([]));
    public Func<CancellationToken, Task<BsonDocument>> Insert { get; set; } = _ => Task.FromResult(new BsonDocument { { "ok", 1 }, { "n", 1 } });
    public Func<CancellationToken, Task<IAsyncCursor<BsonDocument>>> List { get; set; } = _ =>
        Task.FromResult<IAsyncCursor<BsonDocument>>(new Mongo024Cursor<BsonDocument>([]));
    public Func<CancellationToken, Task<string>> Create { get; set; } = _ => Task.FromResult(MongoUsersInitializer.IndexName);
    public static User User(UserStatus status = UserStatus.Active) => new(Guid.NewGuid(), "Synthetic", "synthetic@example.invalid",
        "synthetic-not-a-hash", status, DateTime.UtcNow, DateTime.UtcNow);
    public void Dispose() => owner.Dispose();
}
