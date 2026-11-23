using System.Collections.Concurrent;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.Testing;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Servers;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class MongoUserStoreCredentialCasIntegrationTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public Task Real_cas_updates_only_hash_and_millisecond_timestamp_with_effective_concerns() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture);
            await Mongo026Evidence.PrepareAsync(scope);
            var original = Mongo026Evidence.User();
            using var insertBudget = new RequestBudget(8000, default);
            Assert.Equal(UserInsertStatus.Confirmed, (await scope.Store.InsertAsync(original, insertBudget)).Status);
            using var actor = new Mongo026Actor(scope);
            using var budget = new RequestBudget(8000, default);
            var result = await actor.Store.CompareExchangePasswordHashAsync(original.Id, original.PasswordHash, UserStatus.Active,
                "synthetic-cas-next", Mongo026Evidence.Later.AddTicks(12345), budget);
            Assert.Equal(UserCredentialUpdateStatus.Updated, result.Status);
            var saved = await scope.Users.Find(user => user.Id == original.Id).SingleAsync();
            Mongo026Evidence.AssertPreserved(original, saved, "synthetic-cas-next", Mongo026Evidence.Later.AddMilliseconds(1));
            Assert.True(Assert.Single(actor.UpdateConcerns));
            Assert.True(Assert.Single(actor.UpdateAcks));
            Assert.False(actor.Client.Settings.RetryWrites);
            Assert.Equal(ReadConcern.Majority, actor.Database.Settings.ReadConcern);
            Assert.Equal(ReadPreference.Primary, actor.Database.Settings.ReadPreference);
            // Un no-op confirmado no prueba un rehash persistido.
            using var noOpBudget = new RequestBudget(8000, default);
            Assert.Equal(UserCredentialUpdateStatus.Unavailable, (await actor.Store.CompareExchangePasswordHashAsync(
                original.Id, "synthetic-cas-next", UserStatus.Active, "synthetic-cas-next", saved.UpdatedAt, noOpBudget)).Status);
            output.WriteLine("T026 CAS=Updated FIELDS_CHANGED=2 UTC_MILLISECONDS=true WRITE=majority+j:true WTIMEOUT_MS=500 ACK=true RETRY_WRITES=false NO_UPSERT=true COLLATION=simple NOOP=Unavailable");
        });

    [Theory]
    [InlineData("hash")]
    [InlineData("status")]
    [InlineData("id")]
    public Task Nonmatching_preconditions_are_conflict_without_mutation(string mismatch) =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture);
            await Mongo026Evidence.PrepareAsync(scope);
            var original = Mongo026Evidence.User(mismatch == "status" ? UserStatus.Disabled : UserStatus.Active);
            using var budget = new RequestBudget(8000, default);
            Assert.Equal(UserInsertStatus.Confirmed, (await scope.Store.InsertAsync(original, budget)).Status);
            var result = await scope.Store.CompareExchangePasswordHashAsync(mismatch == "id" ? Guid.NewGuid() : original.Id,
                mismatch == "hash" ? "synthetic-obsolete" : original.PasswordHash, UserStatus.Active,
                "synthetic-next", Mongo026Evidence.Later, budget);
            Assert.Equal(UserCredentialUpdateStatus.Conflict, result.Status);
            var saved = await scope.Users.Find(user => user.Id == original.Id).SingleAsync();
            Assert.True(saved.ToBsonDocument().Equals(UserDocument.FromUser(original).ToBsonDocument()));
            Assert.Equal(1, await scope.Users.CountDocumentsAsync(FilterDefinition<UserDocument>.Empty));
            output.WriteLine($"T026 MISMATCH={mismatch} RESULT=Conflict MUTATION=false UPSERT=false STATUS_PRESERVED=true");
        });

    [Fact]
    public Task Independent_clients_race_one_winner_and_one_conflict_without_overwrite() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture);
            await Mongo026Evidence.PrepareAsync(scope);
            var original = Mongo026Evidence.User();
            using var budget = new RequestBudget(8000, default);
            Assert.Equal(UserInsertStatus.Confirmed, (await scope.Store.InsertAsync(original, budget)).Status);
            await Mongo026Evidence.RaceAsync(scope, original);
            output.WriteLine("T026 INDEPENDENT_CLIENTS=2 RACE=Updated+Conflict WINNER_PRESERVED=true STALE_HASH=Conflict AUTO_READBACK=false");
        });

    [Fact]
    public Task Real_update_with_suppressed_ack_is_uncertain_one_attempt_without_readback() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture);
            await Mongo026Evidence.PrepareAsync(scope);
            var original = Mongo026Evidence.User();
            using var budget = new RequestBudget(8000, default);
            Assert.Equal(UserInsertStatus.Confirmed, (await scope.Store.InsertAsync(original, budget)).Status);
            await Mongo026Evidence.SuppressAckAsync(scope, original);
            output.WriteLine("T026 REAL_UPDATE=true ACK_DELIVERY_SUPPRESSED=adapter_boundary RESULT=Uncertain ATTEMPTS=1 AUTO_READBACK=false RETRY=false COMPENSATION=false TCP_CUT=false FAILOVER=false");
        });
}

// Helpers del incremento 026/027. La fixture y los helpers anteriores permanecen intactos.
internal static class Mongo026Evidence
{
    public static readonly DateTime Created = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime Later = Created.AddMinutes(1);
    public static User User(UserStatus status = UserStatus.Active, string email = "synthetic-cas@example.invalid") =>
        new(Guid.NewGuid(), "Synthetic CAS", email, "synthetic-cas-original", status, Created, Created);

    public static Task WarmAsync(IMongoClient client) => Mongo024IntegrationScope.WaitForClientReadyAsync(client);

    public static async Task PrepareAsync(Mongo024IntegrationScope scope)
    {
        await WarmAsync(scope.Client);
        using var setup = new RequestBudget(5000, default);
        await scope.CreateSetupIndexAsync(MongoUsersInitializer.CreateIndexDefinition(), setup.Token);
        Assert.Equal(MongoUsersInitializationStatus.Ready, await scope.Initializer.InitializeAsync(setup));
    }

    public static void AssertPreserved(User original, UserDocument saved, string expectedHash, DateTime expectedTime)
    {
        // Comparaciones booleanas: un fallo no imprime hashes ni documentos BSON.
        Assert.True(saved.PasswordHash == expectedHash);
        Assert.Equal(expectedTime, saved.UpdatedAt);
        Assert.Equal(DateTimeKind.Utc, saved.UpdatedAt.Kind);
        Assert.Equal(0, saved.UpdatedAt.Ticks % TimeSpan.TicksPerMillisecond);
        Assert.True(saved.Id == original.Id && saved.FullName == original.FullName &&
            saved.NormalizedEmail == original.NormalizedEmail && saved.Status == original.Status && saved.CreatedAt == original.CreatedAt);
        Assert.Equal(7, saved.ToBsonDocument().ElementCount);
    }

    public static async Task RaceAsync(Mongo024IntegrationScope scope, User original)
    {
        using var a = new Mongo026Actor(scope);
        using var b = new Mongo026Actor(scope);
        Assert.NotSame(a.Client, b.Client);
        await Task.WhenAll(WarmAsync(a.Client), WarmAsync(b.Client));
        using var budgetA = new RequestBudget(8000, default);
        using var budgetB = new RequestBudget(8000, default);
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<UserCredentialUpdateResult> Run(Mongo026Actor actor, string hash, DateTime instant, RequestBudget budget)
        {
            await start.Task;
            return await actor.Store.CompareExchangePasswordHashAsync(original.Id, original.PasswordHash,
                original.Status, hash, instant, budget);
        }
        var first = Run(a, "synthetic-winner-a", Later, budgetA);
        var second = Run(b, "synthetic-winner-b", Later.AddSeconds(1), budgetB);
        start.SetResult();
        var results = await Task.WhenAll(first, second);
        Assert.Single(results, result => result.Status == UserCredentialUpdateStatus.Updated);
        Assert.Single(results, result => result.Status == UserCredentialUpdateStatus.Conflict);
        var aWon = results[0].Status == UserCredentialUpdateStatus.Updated;
        var saved = await scope.Users.Find(user => user.Id == original.Id).SingleAsync();
        AssertPreserved(original, saved, aWon ? "synthetic-winner-a" : "synthetic-winner-b", aWon ? Later : Later.AddSeconds(1));
        Assert.True(Assert.Single(a.UpdateConcerns));
        Assert.True(Assert.Single(b.UpdateConcerns));
        Assert.True(Assert.Single(a.UpdateAcks));
        Assert.True(Assert.Single(b.UpdateAcks));
        Assert.Equal(0, a.ReadCommands + b.ReadCommands);
        using var staleBudget = new RequestBudget(8000, default);
        Assert.Equal(UserCredentialUpdateStatus.Conflict, (await b.Store.CompareExchangePasswordHashAsync(original.Id,
            original.PasswordHash, original.Status, "synthetic-stale-retry", Later.AddMinutes(1), staleBudget)).Status);
        var after = await scope.Users.Find(user => user.Id == original.Id).SingleAsync();
        Assert.True(saved.ToBsonDocument().Equals(after.ToBsonDocument()));
    }

    public static async Task SuppressAckAsync(Mongo024IntegrationScope scope, User original)
    {
        var calls = 0;
        var reads = 0;
        var otherWrites = 0;
        var collection = Mongo024IntegrationProxy.Create<IMongoCollection<UserDocument>>((method, args) =>
        {
            if (method.Name == "UpdateOneAsync")
            {
                calls++;
                return LoseAsync((Task<UpdateResult>)Mongo024IntegrationProxy.Forward(method, scope.Users, args)!);
            }
            if (method.Name == "FindAsync") reads++;
            if (method.Name.Contains("Delete", StringComparison.Ordinal) || method.Name.Contains("Insert", StringComparison.Ordinal) ||
                method.Name.Contains("Replace", StringComparison.Ordinal) || method.Name.Contains("BulkWrite", StringComparison.Ordinal)) otherWrites++;
            return Mongo024IntegrationProxy.Forward(method, scope.Users, args);
        });
        var database = Mongo024IntegrationProxy.Create<IMongoDatabase>((method, args) =>
        {
            if (method.Name == "GetCollection") return collection;
            if (method.Name == "RunCommandAsync") otherWrites++;
            return Mongo024IntegrationProxy.Forward(method, scope.Database, args);
        });
        var store = new MongoUserStore(database, scope.Options);
        using var budget = new RequestBudget(8000, default);
        var result = await store.CompareExchangePasswordHashAsync(original.Id, original.PasswordHash, original.Status,
            "synthetic-ack-suppressed", Later, budget);
        Assert.Equal(UserCredentialUpdateStatus.Uncertain, result.Status);
        Assert.True(result.ResultUncertain);
        Assert.Equal(1, calls);
        Assert.Equal(0, reads + otherWrites);
        // Observación del test, posterior al retorno; no repara el resultado del adapter.
        var saved = await scope.Users.Find(user => user.Id == original.Id).SingleAsync();
        AssertPreserved(original, saved, "synthetic-ack-suppressed", Later);
        Assert.Equal(UserCredentialUpdateStatus.Uncertain, result.Status);
    }

    private static async Task<UpdateResult> LoseAsync(Task<UpdateResult> operation)
    {
        var result = await operation;
        Assert.True(result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1);
        throw new TimeoutException("Entrega de ACK suprimida exclusivamente por el test.");
    }
}

internal sealed class Mongo026Actor : IDisposable
{
    private int readCommands;
    public Mongo026Actor(Mongo024IntegrationScope scope)
    {
        var settings = MongoClientFactory.CreateSettings(scope.Options);
        settings.ClusterConfigurator = builder =>
        {
            builder.Subscribe<CommandStartedEvent>(command =>
            {
                if (command.DatabaseNamespace.DatabaseName != scope.Options.DatabaseName) return;
                if (command.CommandName == "find") Interlocked.Increment(ref readCommands);
                if (command.CommandName != "update") return;
                var concern = command.Command.GetValue("writeConcern", new BsonDocument()).AsBsonDocument;
                var update = command.Command["updates"][0].AsBsonDocument;
                UpdateConcerns.Enqueue(concern.GetValue("w", "").Equals(new BsonString("majority")) &&
                    concern.GetValue("j", false).Equals(BsonBoolean.True) && concern.GetValue("wtimeout", 0).ToInt64() == 500 &&
                    !command.Command.Contains("txnNumber") && !update.GetValue("upsert", false).ToBoolean() &&
                    update.GetValue("collation", new BsonDocument()).AsBsonDocument.GetValue("locale", "").AsString == "simple");
            });
            builder.Subscribe<CommandSucceededEvent>(command =>
            {
                if (command.CommandName == "update") UpdateAcks.Enqueue(command.Reply.GetValue("ok", 0).ToDouble() == 1 &&
                    !command.Reply.Contains("writeErrors") && !command.Reply.Contains("writeConcernError"));
            });
        };
        Client = new MongoClient(settings);
        Database = Client.GetDatabase(scope.Options.DatabaseName);
        Store = new MongoUserStore(Database, scope.Options);
    }
    public MongoClient Client { get; }
    public IMongoDatabase Database { get; }
    public MongoUserStore Store { get; }
    public ConcurrentQueue<bool> UpdateConcerns { get; } = new();
    public ConcurrentQueue<bool> UpdateAcks { get; } = new();
    public int ReadCommands => readCommands;
    public void Dispose() => Client.Dispose();
}
