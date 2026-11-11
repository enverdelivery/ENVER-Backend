using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.Testing;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class MongoPersistenceIntegrationTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public Task Confirmed_creation_is_visible_to_other_client_with_exact_bson_index_and_local_topology() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture);
            await Mongo026Evidence.PrepareAsync(scope);
            using var b = new Mongo026Actor(scope);
            await Mongo026Evidence.WarmAsync(b.Client);
            Assert.NotSame(scope.Client, b.Client);
            var original = Mongo026Evidence.User();
            using var aBudget = new RequestBudget(8000, default);
            Assert.Equal(UserInsertStatus.Confirmed, (await scope.Store.InsertAsync(original, aBudget)).Status);
            using var bBudget = new RequestBudget(8000, default);
            var byId = await b.Store.FindByIdAsync(original.Id, bBudget);
            var byEmail = await b.Store.FindByNormalizedEmailAsync(original.NormalizedEmail, bBudget);
            Assert.Equal(UserReadStatus.Found, byId.Status);
            Assert.Equal(UserReadStatus.Found, byEmail.Status);
            Assert.True(byId.User!.Id == original.Id && byEmail.User!.Id == original.Id);
            var raw = await b.Database.GetCollection<BsonDocument>("users").Find(
                new BsonDocument("_id", new BsonBinaryData(original.Id, GuidRepresentation.Standard))).SingleAsync();
            Assert.Equal(new[] { "_id", "createdAt", "fullName", "normalizedEmail", "passwordHash", "status", "updatedAt" }, raw.Names.Order());
            Assert.Equal(BsonBinarySubType.UuidStandard, raw["_id"].AsBsonBinaryData.SubType);
            Assert.True(raw.Equals(UserDocument.FromUser(original).ToBsonDocument()));
            Assert.Equal(BsonType.DateTime, raw["createdAt"].BsonType);
            Assert.Equal(raw["createdAt"], raw["updatedAt"]);
            using var indexBudget = new RequestBudget(8000, default);
            Assert.Equal(MongoUsersInitializationStatus.Ready, await new MongoUsersInitializer(b.Database, scope.Options).InitializeAsync(indexBudget));
            using var indexes = await b.Database.GetCollection<UserDocument>("users").Indexes.ListAsync();
            var definitions = await indexes.ToListAsync();
            Assert.Single(definitions, MongoUsersInitializer.IsCompatible);
            Assert.Single(definitions, index => index.GetValue("name", "").AsString == "_id_");
            Assert.True(Assert.Single(scope.Writes));
            Assert.Equal(ReadConcern.Majority, b.Database.Settings.ReadConcern);
            Assert.Equal(ReadPreference.Primary, b.Database.Settings.ReadPreference);
            Assert.False(b.Client.Settings.RetryWrites);
            using var topologyTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var admin = b.Client.GetDatabase("admin");
            var hello = await admin.RunCommandAsync<BsonDocument>(new BsonDocument("hello", 1), cancellationToken: topologyTimeout.Token);
            var build = await admin.RunCommandAsync<BsonDocument>(new BsonDocument("buildInfo", 1), cancellationToken: topologyTimeout.Token);
            Assert.Equal("rs0", hello["setName"].AsString);
            Assert.True(hello["isWritablePrimary"].AsBoolean);
            Assert.Single(hello["hosts"].AsBsonArray);
            Assert.Equal("8.0.34", build["version"].AsString);
            output.WriteLine("T027 COMPONENTS=023+024+025 CLIENTS=2 CREATION=Confirmed CROSS_CLIENT_READS=id+email BSON_FIELDS=7 UUID=Standard_SUBTYPE4 INDEX=unique_compatible INITIALIZER=Ready TOPOLOGY=rs0 NODES=1 VERSION=8.0.34 ATLAS=false FAILOVER=false");
        });

    [Fact]
    public Task Concurrent_equivalent_creations_then_cas_have_one_winner_and_preserve_stale_state() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var scope = new Mongo024IntegrationScope(fixture);
            await Mongo026Evidence.PrepareAsync(scope);
            using var a = new Mongo026Actor(scope);
            using var b = new Mongo026Actor(scope);
            Assert.NotSame(a.Client, b.Client);
            await Task.WhenAll(Mongo026Evidence.WarmAsync(a.Client), Mongo026Evidence.WarmAsync(b.Client));
            // 028 normalizará entradas HTTP después. Aquí ambas identidades ya son el mismo normalizedEmail.
            var first = Mongo026Evidence.User();
            var second = Mongo026Evidence.User(email: first.NormalizedEmail);
            Assert.NotEqual(first.Id, second.Id);
            using var aBudget = new RequestBudget(8000, default);
            using var bBudget = new RequestBudget(8000, default);
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task<UserInsertResult> Insert(Mongo026Actor actor, User user, RequestBudget budget)
            {
                await start.Task;
                return await actor.Store.InsertAsync(user, budget);
            }
            var insertA = Insert(a, first, aBudget);
            var insertB = Insert(b, second, bBudget);
            start.SetResult();
            var results = await Task.WhenAll(insertA, insertB);
            Assert.Single(results, result => result.Status == UserInsertStatus.Confirmed);
            Assert.Single(results, result => result.Status == UserInsertStatus.Conflict);
            var winner = results[0].Status == UserInsertStatus.Confirmed ? first : second;
            Assert.Equal(1, await scope.Users.CountDocumentsAsync(FilterDefinition<UserDocument>.Empty));
            using var readBudget = new RequestBudget(8000, default);
            Assert.True((await b.Store.FindByNormalizedEmailAsync(winner.NormalizedEmail, readBudget)).User!.Id == winner.Id);
            await Mongo026Evidence.RaceAsync(scope, winner);
            var saved = await scope.Users.Find(user => user.Id == winner.Id).SingleAsync();
            using var mismatchBudget = new RequestBudget(8000, default);
            Assert.Equal(UserCredentialUpdateStatus.Conflict, (await a.Store.CompareExchangePasswordHashAsync(winner.Id,
                saved.PasswordHash, UserStatus.Disabled, "synthetic-status-mismatch", Mongo026Evidence.Later.AddHours(1), mismatchBudget)).Status);
            var after = await scope.Users.Find(user => user.Id == winner.Id).SingleAsync();
            Assert.True(saved.ToBsonDocument().Equals(after.ToBsonDocument()));
            Assert.Equal(1, await scope.Users.CountDocumentsAsync(FilterDefinition<UserDocument>.Empty));
            output.WriteLine("T027 COMPONENTS=023-026 INDEPENDENT_CLIENTS=true INSERT_RACE=Confirmed+Conflict FINAL_DOCUMENTS=1 CAS_RACE=Updated+Conflict WINNER_PRESERVED=true HASH_STATUS_MISMATCH=Conflict MUTATION=false NORMALIZATION_FLOW=false");
        });

    [Fact]
    public Task Uncertain_update_failed_read_and_owned_cleanup_do_not_claim_absence_or_rollback() =>
        Mongo024IntegrationScope.SafelyAsync(async () =>
        {
            await using var first = new Mongo024IntegrationScope(fixture);
            await using var second = new Mongo024IntegrationScope(fixture);
            await Mongo026Evidence.PrepareAsync(first);
            await Mongo026Evidence.PrepareAsync(second);
            var original = Mongo026Evidence.User();
            using var firstBudget = new RequestBudget(8000, default);
            using var secondBudget = new RequestBudget(8000, default);
            Assert.Equal(UserInsertStatus.Confirmed, (await first.Store.InsertAsync(original, firstBudget)).Status);
            Assert.Equal(UserReadStatus.NotFound, (await second.Store.FindByIdAsync(original.Id, secondBudget)).Status);
            Assert.Equal(UserInsertStatus.Confirmed, (await second.Store.InsertAsync(original, secondBudget)).Status);
            await Mongo026Evidence.SuppressAckAsync(first, original);
            // Error BSON real en frontera de lectura, no ausencia de usuario ni fallo de red inventado.
            const string malformedEmail = "synthetic-malformed@example.invalid";
            await first.Database.GetCollection<BsonDocument>("users").InsertOneAsync(new BsonDocument
            {
                { "_id", new BsonBinaryData(Guid.NewGuid(), GuidRepresentation.Standard) },
                { "normalizedEmail", malformedEmail }
            });
            using var failedReadBudget = new RequestBudget(8000, default);
            var failedRead = await first.Store.FindByNormalizedEmailAsync(malformedEmail, failedReadBudget);
            Assert.Equal(UserReadStatus.Unavailable, failedRead.Status);
            Assert.Null(failedRead.User);
            Assert.Equal(2, await first.Users.CountDocumentsAsync(FilterDefinition<UserDocument>.Empty));
            await first.DropOwnDatabaseAsync();
            var preserved = await second.Users.Find(user => user.Id == original.Id).SingleAsync();
            Assert.True(preserved.ToBsonDocument().Equals(UserDocument.FromUser(original).ToBsonDocument()));
            Assert.Equal(1, await second.Users.CountDocumentsAsync(FilterDefinition<UserDocument>.Empty));
            output.WriteLine("T027 REAL_UPDATE=true SUPPRESSED_ACK=Uncertain ATTEMPTS=1 AUTO_READBACK=false RETRY=false COMPENSATION=false REAL_BSON_READ_ERROR=Unavailable NOT_FOUND=false DATABASE_ISOLATION=true OWN_CLEANUP=true SECOND_DATABASE_PRESERVED=true EXACTLY_ONCE=false TCP_CUT=false CLOUD=false");
        });
}
