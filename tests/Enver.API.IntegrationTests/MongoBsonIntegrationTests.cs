using System.Collections.Concurrent;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Core;
using Enver.API.Infrastructure.MongoDB;
using Enver.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Servers;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class MongoBsonIntegrationTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public Task Runtime_is_the_declared_single_node_primary_rs0_with_wiredtiger() => SafelyAsync(async () =>
    {
        using var owner = new MongoClientFactory(Options());
        var client = owner.Client;
        Assert.Same(client, owner.Client);
        var admin = client.GetDatabase("admin");
        var hello = await admin.RunCommandAsync<BsonDocument>(new BsonDocument("hello", 1));
        var status = await admin.RunCommandAsync<BsonDocument>(new BsonDocument("serverStatus", 1));
        var build = await admin.RunCommandAsync<BsonDocument>(new BsonDocument("buildInfo", 1));
        Assert.Equal("rs0", hello["setName"].AsString);
        Assert.True(hello["isWritablePrimary"].AsBoolean);
        Assert.Single(hello["hosts"].AsBsonArray);
        Assert.Equal("wiredTiger", status["storageEngine"]["name"].AsString);
        Assert.True(status["storageEngine"]["persistent"].AsBoolean);
        Assert.Equal("8.0.34", build["version"].AsString);
        Assert.Contains(client.Cluster.Description.Servers, server => server.Type == ServerType.ReplicaSetPrimary);
        Report("TOPOLOGY=rs0 NODES=1 WRITABLE_PRIMARY=true ENGINE=WiredTiger PERSISTENT=true VERSION=8.0.34 SINGLETON_REUSED=true CLOUD_OR_FAILOVER=false");
    });

    [Fact]
    public Task Real_bson_round_trip_has_seven_fields_and_effective_majority_journal_ack() => SafelyAsync(async () =>
    {
        var databaseName = OwnDatabaseName();
        var settings = MongoClientFactory.CreateSettings(Options());
        var writes = new ConcurrentQueue<bool>();
        var reads = new ConcurrentQueue<bool>();
        var acknowledgments = new ConcurrentQueue<bool>();
        // Solo señales agregadas; nunca conservar ni imprimir comandos/documentos/credenciales.
        settings.ClusterConfigurator = builder =>
        {
            builder.Subscribe<CommandStartedEvent>(command =>
            {
                if (command.DatabaseNamespace.DatabaseName != databaseName)
                {
                    return;
                }

                if (command.CommandName == "insert")
                {
                    var concern = command.Command.GetValue("writeConcern", new BsonDocument()).AsBsonDocument;
                    writes.Enqueue(concern.GetValue("w", "").Equals(new BsonString("majority")) &&
                        concern.GetValue("j", false).Equals(BsonBoolean.True) &&
                        concern.GetValue("wtimeout", 0).ToInt64() == 500 &&
                        !command.Command.Contains("txnNumber"));
                }
                else if (command.CommandName == "find")
                {
                    var concern = command.Command.GetValue("readConcern", new BsonDocument()).AsBsonDocument;
                    reads.Enqueue(concern.GetValue("level", "").Equals(new BsonString("majority")));
                }
            });
            builder.Subscribe<CommandSucceededEvent>(command =>
            {
                if (command.CommandName == "insert")
                {
                    acknowledgments.Enqueue(command.Reply.GetValue("ok", 0).ToDouble() == 1 &&
                        command.Reply.GetValue("n", 0).ToInt32() == 1 &&
                        !command.Reply.Contains("writeErrors") && !command.Reply.Contains("writeConcernError"));
                }
            });
        };
        using var client = new MongoClient(settings);
        try
        {
            var database = client.GetDatabase(databaseName);
            var typed = database.GetCollection<UserDocument>("users");
            var raw = database.GetCollection<BsonDocument>("users");
            Assert.Equal(ReadConcern.Majority, typed.Settings.ReadConcern);
            Assert.Equal(ReadPreference.Primary, typed.Settings.ReadPreference);
            Assert.Equal("majority", typed.Settings.WriteConcern.W.ToString());
            Assert.True(typed.Settings.WriteConcern.Journal);
            Assert.Equal(TimeSpan.FromMilliseconds(500), typed.Settings.WriteConcern.WTimeout);
            Assert.False(client.Settings.RetryWrites);
            var original = UserDocument.FromUser(User.Create("Synthetic", "roundtrip@example.invalid",
                "synthetic-not-a-real-hash", new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc).AddTicks(12345)));
            await typed.InsertOneAsync(original);
            var bson = await raw.Find(new BsonDocument("_id", new BsonBinaryData(original.Id, GuidRepresentation.Standard))).SingleAsync();
            Assert.Equal(7, bson.ElementCount);
            Assert.Equal(new[] { "_id", "createdAt", "fullName", "normalizedEmail", "passwordHash", "status", "updatedAt" }, bson.Names.Order());
            Assert.All(new[] { "fullName", "normalizedEmail", "passwordHash", "status" }, field =>
                Assert.Equal(BsonType.String, bson[field].BsonType));
            Assert.Equal(BsonBinarySubType.UuidStandard, bson["_id"].AsBsonBinaryData.SubType);
            Assert.Equal(original.Id, bson["_id"].AsBsonBinaryData.ToGuid(GuidRepresentation.Standard));
            Assert.Equal(original.Id.ToByteArray(bigEndian: true), bson["_id"].AsBsonBinaryData.Bytes);
            Assert.Equal("Active", bson["status"].AsString);
            Assert.Equal(BsonType.DateTime, bson["createdAt"].BsonType);
            Assert.Equal(BsonType.DateTime, bson["updatedAt"].BsonType);
            Assert.Equal(bson["createdAt"], bson["updatedAt"]);
            var restored = await typed.Find(document => document.Id == original.Id).SingleAsync();
            Assert.Equal(original.ToBsonDocument(), restored.ToBsonDocument());
            Assert.Equal(DateTimeKind.Utc, restored.CreatedAt.Kind);
            Assert.Equal(DateTimeKind.Utc, restored.UpdatedAt.Kind);
            Assert.Equal(0, restored.CreatedAt.Ticks % TimeSpan.TicksPerMillisecond);
            Assert.Equal(0, restored.UpdatedAt.Ticks % TimeSpan.TicksPerMillisecond);
            Assert.True(Assert.Single(writes));
            Assert.True(Assert.Single(acknowledgments));
            Assert.Equal(2, reads.Count);
            Assert.All(reads, value => Assert.True(value));
            Assert.Contains(client.Cluster.Description.Servers, server => server.Type == ServerType.ReplicaSetPrimary);
            Report("BSON_FIELDS=7 UUID=Standard_SUBTYPE4 STATUS=String UTC_MILLISECONDS=true ROUND_TRIP=true WRITE=majority+j:true WTIMEOUT_MS=500 ACK=true READ=majority PREFERENCE=primary RETRY_WRITES=false JOURNAL_WRITE_ACCEPTED=true");
        }
        finally
        {
            await DropOwnDatabaseAsync(client, databaseName);
        }
    });

    [Fact]
    public Task Run_environment_databases_and_cleanup_are_isolated_without_cross_contamination() => SafelyAsync(async () =>
    {
        var firstName = OwnDatabaseName();
        var secondName = OwnDatabaseName();
        using var owner = new MongoClientFactory(Options());
        var client = owner.Client;
        try
        {
            Assert.Equal($"T015_{fixture.RunId}", fixture.EnvironmentName);
            Assert.Equal($"enver_t015_{fixture.RunId}", fixture.MongoDatabase);
            Assert.NotEqual(firstName, secondName);
            var first = client.GetDatabase(firstName).GetCollection<UserDocument>("users");
            var second = client.GetDatabase(secondName).GetCollection<UserDocument>("users");
            var firstUser = UserDocument.FromUser(User.Create("Synthetic A", "a@example.invalid", "synthetic-a", DateTime.UtcNow));
            var secondUser = UserDocument.FromUser(User.Create("Synthetic B", "b@example.invalid", "synthetic-b", DateTime.UtcNow));
            await first.InsertOneAsync(firstUser);
            await second.InsertOneAsync(secondUser);
            Assert.Equal(1, await first.CountDocumentsAsync(FilterDefinition<UserDocument>.Empty));
            Assert.Equal(1, await second.CountDocumentsAsync(FilterDefinition<UserDocument>.Empty));
            Assert.False(await first.Find(user => user.Id == secondUser.Id).AnyAsync());
            Assert.False(await second.Find(user => user.Id == firstUser.Id).AnyAsync());
            await DropOwnDatabaseAsync(client, firstName);
            using var names = await client.GetDatabase(firstName).ListCollectionNamesAsync();
            Assert.Empty(await names.ToListAsync());
            Assert.True(await second.Find(user => user.Id == secondUser.Id).AnyAsync());
            Report("RUN_ENVIRONMENT_ISOLATED=true DATABASES=2 CROSS_CONTAMINATION=false OWN_CLEANUP_ONLY=true OTHER_DATABASE_PRESERVED=true GLOBAL_DELETE=false");
        }
        finally
        {
            await DropOwnDatabaseAsync(client, firstName);
            await DropOwnDatabaseAsync(client, secondName);
        }
    });

    private MongoDbOptions Options() => new ConfigurationBuilder().AddInMemoryCollection(fixture.CreateConfiguration())
        .Build().GetRequiredSection(MongoDbOptions.SectionName).Get<MongoDbOptions>()!;

    private string OwnDatabaseName() => $"{fixture.MongoDatabase}_t023_{Guid.NewGuid().ToString("N")[..12]}";

    private async Task DropOwnDatabaseAsync(IMongoClient client, string name)
    {
        Assert.StartsWith($"{fixture.MongoDatabase}_t023_", name, StringComparison.Ordinal);
        await client.DropDatabaseAsync(name);
    }

    private void Report(string report)
    {
        Assert.False(fixture.ContainsSyntheticSecret(report));
        Assert.DoesNotContain(fixture.MongoConnectionString, report);
        Assert.DoesNotContain("@", report);
        output.WriteLine(report + " SYNTHETIC_DATA_ONLY=true SENSITIVE_VALUES_LOGGED=false");
    }

    private static async Task SafelyAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            // El test runner no debe publicar URI, credenciales ni BSON de una excepción del driver.
            throw new XunitException($"T-AUTH-023: evidencia no acreditada; tipo={exception.GetType().Name}; detalles sensibles suprimidos.");
        }
    }
}
