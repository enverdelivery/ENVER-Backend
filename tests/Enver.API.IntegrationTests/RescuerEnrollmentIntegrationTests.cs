using System.Collections.Concurrent;
using System.Text.Json;
using Enver.API.Configuration;
using Enver.API.Features.Users.Application;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using static Enver.API.IntegrationTests.OrdersIntegrationTests;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RescuerEnrollmentIntegrationTests(AuthenticationDependenciesFixture fixture)
{
    private const string Users = "/api/v1/users/me";
    private const string Enrollment = Users + "/rescuer-enrollment";
    private const string Rules = "test.rules_1";
    private const string Consent = """{"acceptedRules":true,"rulesVersion":"test.rules_1"}""";
    private const string Policy = "test.policy_1";

    [Fact]
    public async Task Default_composition_is_disabled_and_enrollment_writes_nothing()
    {
        await using var scope = await RegistrationTestScope.CreateAsync(fixture, initializeQuota: false, probeRegistrationStore: false);
        using var setup = new RequestBudget(15000, default);
        await WarmRedisAsync(scope, setup);
        var member = await SeedAsync(scope, "enroll-disabled@uttt.edu.mx", setup);
        Assert.Equal("DisabledRescuerEligibility", scope.Services.GetRequiredService<IRescuerEligibility>().GetType().Name);
        Assert.Equal("DisabledRescuerEnrollment", scope.Services.GetRequiredService<IRescuerEnrollment>().GetType().Name);
        Assert.Null(scope.Services.GetService<IRescuerEnrollmentStore>());
        Assert.Null(scope.Services.GetService<MongoRescuerEligibilityStore>());
        Assert.Null(scope.Services.GetService<IRescuerEnrollmentQuota>());
        Assert.Null(scope.Services.GetService<EnrollmentPolicy>());
        using var response = await SendAsync(scope, member.Token, HttpMethod.Post, Enrollment, payload: Consent);
        await scope.AssertProblemAsync(response, 503, "temporarily_unavailable");
        using var collections = await scope.Database.ListCollectionNamesAsync(cancellationToken: setup.Token);
        var names = await collections.ToListAsync(setup.Token);
        Assert.DoesNotContain(MongoRescuerEligibilityStore.Grants, names);
        Assert.DoesNotContain(MongoRescuerEligibilityStore.Events, names);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Lost_mongo_commit_reply_is_reconciled_in_the_same_enrollment_or_returns_503(bool canReadAudit)
    {
        var target = new MongoUrl(fixture.MongoConnectionString).Server;
        await using var relay = new MongoCommitReplyRelay(target.Host, target.Port);
        var trace = new ConcurrentQueue<string>();
        var commits = new ConcurrentDictionary<int, (Guid Session, long Transaction)>();
        var insertCounts = new ConcurrentDictionary<string, int>();
        var observing = 0;
        await using var scope = await RegistrationTestScope.CreateAsync(fixture, initializeQuota: false, probeRegistrationStore: false,
            configureServices: services =>
            {
                services.AddSingleton(p =>
                {
                    var options = p.GetRequiredService<IOptions<MongoDbOptions>>().Value;
                    var settings = MongoClientFactory.CreateSettings(options);
                    settings.Server = new MongoServerAddress("127.0.0.1", relay.Port);
                    settings.DirectConnection = true; // Do not let discovery bypass the test relay.
                    settings.ClusterConfigurator = cluster =>
                    {
                        cluster.Subscribe<CommandStartedEvent>(evt =>
                        {
                            if (Volatile.Read(ref observing) == 0) return;
                            if (evt.CommandName == "commitTransaction")
                            {
                                commits[evt.RequestId] = (evt.Command["lsid"]["id"].AsBsonBinaryData.ToGuid(), evt.Command["txnNumber"].ToInt64());
                                trace.Enqueue("commit-dispatched");
                                relay.DiscardReply(evt.RequestId, commit: true);
                            }
                            else if (evt.CommandName == "find")
                            {
                                var collection = evt.Command["find"].AsString;
                                trace.Enqueue(collection);
                                if (collection == MongoRescuerEligibilityStore.Events && !canReadAudit)
                                    relay.DiscardReply(evt.RequestId, commit: false);
                            }
                            else if (evt.CommandName == "insert")
                            {
                                var collection = evt.Command["insert"].AsString;
                                insertCounts.AddOrUpdate(collection, 1, (_, count) => count + 1);
                            }
                        });
                        cluster.Subscribe<CommandFailedEvent>(evt =>
                        {
                            if (commits.ContainsKey(evt.RequestId)) trace.Enqueue("commit-failed-at-client");
                        });
                        cluster.Subscribe<CommandSucceededEvent>(evt =>
                        {
                            if (commits.ContainsKey(evt.RequestId)) trace.Enqueue("commit-ack-at-client");
                        });
                    };
                    return new MongoClient(settings);
                });
                services.AddSingleton(p =>
                {
                    var options = p.GetRequiredService<IOptions<MongoDbOptions>>().Value;
                    return new MongoRescuerEligibilityStore(p.GetRequiredService<MongoClient>().GetDatabase(options.DatabaseName), options);
                });
                services.AddSingleton<IRescuerEnrollmentStore>(p => p.GetRequiredService<MongoRescuerEligibilityStore>());
                UseTestEnrollment(services);
            });
        using var setup = new RequestBudget(15000, default);
        await Mongo026Evidence.WarmAsync(scope.Services.GetRequiredService<MongoClient>());
        await scope.Services.GetRequiredService<MongoRescuerEligibilityStore>().PrepareAsync(setup);
        await WarmRedisAsync(scope, setup);
        var member = await SeedAsync(scope, "enroll-ack@uttt.edu.mx", setup);
        await SetPhoneAsync(scope, member.Token);

        Volatile.Write(ref observing, 1);
        using var response = await SendAsync(scope, member.Token, HttpMethod.Post, Enrollment, payload: Consent);
        Volatile.Write(ref observing, 0);
        var history = trace.ToArray();
        Assert.True(relay.AcknowledgedCommitsDiscarded >= 1);
        Assert.Equal(0, relay.OtherCommitRepliesDiscarded);
        Assert.NotEmpty(commits);
        Assert.Single(commits.Values.Distinct()); // Any driver retry uses the same lsid / transaction number.
        Assert.Contains("commit-failed-at-client", history);
        Assert.DoesNotContain("commit-ack-at-client", history);
        var audit = Array.IndexOf(history, MongoRescuerEligibilityStore.Events);
        Assert.True(audit > Array.LastIndexOf(history, "commit-failed-at-client"));
        Assert.Equal(1, insertCounts[MongoRescuerEligibilityStore.Grants]);
        Assert.Equal(1, insertCounts[MongoRescuerEligibilityStore.Events]);

        if (canReadAudit)
        {
            using var body = await JsonAsync(response, 200);
            Assert.True(body.RootElement.GetProperty("rescuerEnabled").GetBoolean());
            Assert.Equal(0, relay.AuditRepliesDiscarded);
            Assert.True(Array.LastIndexOf(history, MongoRescuerEligibilityStore.Grants) > audit);
        }
        else
        {
            await scope.AssertProblemAsync(response, 503, "temporarily_unavailable");
            Assert.True(relay.AuditRepliesDiscarded >= 1);
            Assert.DoesNotContain(MongoRescuerEligibilityStore.Grants, history.Skip(audit + 1));
        }

        // Independent client, not behind the fault: both records really committed even on HTTP 503.
        var grants = scope.Database.GetCollection<BsonDocument>(MongoRescuerEligibilityStore.Grants);
        var events = scope.Database.GetCollection<BsonDocument>(MongoRescuerEligibilityStore.Events);
        var grant = await grants.Find(Filter(member.Id)).SingleAsync(setup.Token);
        var evt = await events.Find(new BsonDocument("userId", Uuid(member.Id))).SingleAsync(setup.Token);
        Assert.Equal("enabled", grant["status"].AsString);
        Assert.Equal(1L, grant["revision"].AsInt64);
        Assert.Equal(1L, evt["revision"].AsInt64);
        Assert.Equal(grant["rulesAcceptedAt"], evt["decidedAt"]);
    }

    [Fact]
    public async Task Enrollment_paused_after_confirmed_absence_cannot_overwrite_a_later_confirmed_revocation()
    {
        await using var scope = await RegistrationTestScope.CreateAsync(fixture, initializeQuota: false, probeRegistrationStore: false,
            configureServices: services =>
            {
                services.AddSingleton(p => new MongoRescuerEligibilityStore(p.GetRequiredService<IMongoDatabase>(),
                    p.GetRequiredService<IOptions<MongoDbOptions>>().Value));
                services.AddSingleton(p => new FirstInsertBarrier(p.GetRequiredService<MongoRescuerEligibilityStore>()));
                services.AddSingleton<IRescuerEnrollmentStore>(p => p.GetRequiredService<FirstInsertBarrier>());
                UseTestEnrollment(services);
            });
        using var setup = new RequestBudget(15000, default);
        var store = scope.Services.GetRequiredService<MongoRescuerEligibilityStore>();
        var barrier = scope.Services.GetRequiredService<FirstInsertBarrier>();
        await store.PrepareAsync(setup);
        await WarmRedisAsync(scope, setup);
        var member = await SeedAsync(scope, "enroll-race@uttt.edu.mx", setup);
        await SetPhoneAsync(scope, member.Token);

        var first = SendAsync(scope, member.Token, HttpMethod.Post, Enrollment, payload: Consent);
        RescuerGrant? initial = null;
        var operation = Guid.NewGuid();
        try
        {
            Assert.Equal(member.Id, await barrier.BeforeFirstWrite.Task.WaitAsync(setup.Token));
            Assert.False(first.IsCompleted);
            using (var second = await SendAsync(scope, member.Token, HttpMethod.Post, Enrollment, payload: Consent))
            using (var applied = await JsonAsync(second, 200))
                Assert.True(applied.RootElement.GetProperty("rescuerEnabled").GetBoolean());

            initial = (await store.ReadAsync(member.Id, setup)).Grant;
            Assert.NotNull(initial);
            Assert.Equal(GrantStatus.Enabled, initial.Status);
            Assert.Equal(1L, initial.Revision);
            var revoke = new EligibilityTransition(operation, member.Id, GrantStatus.Enabled, 1,
                "test.actor", Policy, "test.reason", DateTime.UtcNow);
            Assert.Equal(GrantWriteStatus.Confirmed, await store.TransitionAsync(revoke, setup));
            var revoked = (await store.ReadAsync(member.Id, setup)).Grant;
            Assert.NotNull(revoked);
            Assert.Equal(GrantStatus.Revoked, revoked.Status);
            Assert.Equal(2L, revoked.Revision);
            Assert.False(first.IsCompleted);
        }
        finally { barrier.Release.TrySetResult(); }

        using var rejected = await first;
        await scope.AssertProblemAsync(rejected, 403, "forbidden");
        var final = (await store.ReadAsync(member.Id, setup)).Grant;
        Assert.NotNull(final);
        Assert.Equal(GrantStatus.Revoked, final.Status);
        Assert.Equal(2L, final.Revision);
        Assert.Equal(Rules, final.RulesVersion);
        Assert.Equal(initial!.RulesAcceptedAt, final.RulesAcceptedAt);
        Assert.Equal(2, barrier.InsertAttempts);
        var events = await scope.Database.GetCollection<BsonDocument>(MongoRescuerEligibilityStore.Events)
            .Find(new BsonDocument("userId", Uuid(member.Id))).Sort(new BsonDocument("revision", 1)).ToListAsync(setup.Token);
        Assert.Collection(events,
            enrolled =>
            {
                Assert.Equal(1L, enrolled["revision"].AsInt64);
                Assert.Equal("enabled", enrolled["status"].AsString);
                Assert.Equal(new BsonDateTime(final.RulesAcceptedAt), enrolled["decidedAt"]);
            },
            revoked =>
            {
                Assert.Equal(Uuid(operation), revoked["_id"].AsBsonBinaryData);
                Assert.Equal(2L, revoked["revision"].AsInt64);
                Assert.Equal("revoked", revoked["status"].AsString);
            });
    }

    [Fact]
    public async Task Mongo_enforces_atomic_events_and_one_grant_under_concurrent_enrollment_and_revocation()
    {
        await using var scope = new Mongo024IntegrationScope(fixture);
        using var committed = new CancellationTokenSource();
        var finds = new ConcurrentQueue<(bool ById, bool Bounded, bool Linearizable)>();
        var concerns = new ConcurrentQueue<bool>();
        var settings = MongoClientFactory.CreateSettings(scope.Options);
        settings.ClusterConfigurator = cluster =>
        {
            cluster.Subscribe<CommandStartedEvent>(evt =>
            {
                if (evt.CommandName == "find" && evt.DatabaseNamespace.DatabaseName == scope.Options.DatabaseName &&
                    evt.Command["find"].AsString == MongoRescuerEligibilityStore.Grants && !evt.Command.Contains("autocommit"))
                    finds.Enqueue((evt.Command["filter"].AsBsonDocument.ElementCount == 1 &&
                        evt.Command["filter"]["_id"].AsBsonBinaryData.SubType == BsonBinarySubType.UuidStandard,
                        evt.Command.GetValue("maxTimeMS", 0).ToInt64() > 0,
                        evt.Command.GetValue("readConcern", new BsonDocument()).AsBsonDocument.GetValue("level", "").AsString == "linearizable"));
                if (evt.CommandName == "commitTransaction")
                {
                    var concern = evt.Command["writeConcern"].AsBsonDocument;
                    concerns.Enqueue(concern["w"].AsString == "majority" && concern["j"].AsBoolean);
                }
            });
            cluster.Subscribe<CommandSucceededEvent>(evt =>
            {
                if (evt.CommandName == "commitTransaction" && !evt.Reply.Contains("writeConcernError")) committed.Cancel();
            });
        };
        using var monitored = new MongoClient(settings);
        using var peer = new Mongo026Actor(scope);
        var writer = new MongoRescuerEligibilityStore(peer.Database, scope.Options);
        var readerDatabase = monitored.GetDatabase(scope.Options.DatabaseName);
        var reader = new MongoRescuerEligibilityStore(readerDatabase, scope.Options);
        var adapter = new MongoRescuerEligibility(reader);
        using var setup = new RequestBudget(15000, default);
        await Mongo026Evidence.WarmAsync(scope.Client);
        await Mongo026Evidence.WarmAsync(peer.Client);
        await Mongo026Evidence.WarmAsync(monitored);
        var id = Guid.NewGuid();
        Assert.Null(await adapter.ReadAsync(id, setup));
        await writer.PrepareAsync(setup);
        await reader.PrepareAsync(setup);
        Assert.False((await adapter.ReadAsync(id, setup))!.Enabled);

        var now = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).UtcDateTime;
        var first = new EnrollmentDecision(Guid.NewGuid(), id, Rules, Policy, now);
        var revoke = new EligibilityTransition(Guid.NewGuid(), id, GrantStatus.Enabled, 1, "test.actor", Policy, "test.reason", now.AddMilliseconds(1));
        Assert.Equal(GrantWriteStatus.Conflict, await writer.TransitionAsync(revoke, setup));

        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancelled = new RequestBudget(10000, committed.Token);
        var a = Compete(first, reader, cancelled);
        using var secondBudget = new RequestBudget(10000, default);
        var b = Compete(first with { OperationId = Guid.NewGuid() }, writer, secondBudget);
        start.SetResult();
        var outcomes = await Task.WhenAll(a, b);
        Assert.All(outcomes, result => Assert.Contains(result, new[] { GrantWriteStatus.Confirmed, GrantWriteStatus.Conflict, GrantWriteStatus.Unavailable }));

        var grants = scope.Database.GetCollection<BsonDocument>(MongoRescuerEligibilityStore.Grants);
        var events = scope.Database.GetCollection<BsonDocument>(MongoRescuerEligibilityStore.Events);
        Assert.True((await adapter.ReadAsync(id, setup))!.Enabled);
        var saved = await grants.Find(Filter(id)).SingleAsync(setup.Token);
        var evt = await events.Find(new BsonDocument("userId", Uuid(id))).SingleAsync(setup.Token);
        Assert.Equal(1, saved["revision"].AsInt64);
        Assert.Equal(id, evt["initiatedBy"].AsBsonBinaryData.ToGuid());
        Assert.Equal("server_policy", evt["authority"].AsString);
        Assert.Equal(Policy, evt["policyVersion"].AsString);
        Assert.Equal(Rules, evt["rulesVersion"].AsString);
        Assert.Equal(5, evt["checkedRequirements"].AsBsonArray.Count);
        Assert.Equal(saved["rulesAcceptedAt"], evt["decidedAt"]);
        using var indexes = await events.Indexes.ListAsync(cancellationToken: setup.Token);
        Assert.Contains(await indexes.ToListAsync(setup.Token), index => index.TryGetValue("unique", out var unique) && unique.AsBoolean &&
            index["key"].AsBsonDocument.Names.SequenceEqual(new[] { "userId", "revision" }));

        // A confirmed server commit with a cancelled caller still isn't a successful response.
        var ack = first with { OperationId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        if (committed.IsCancellationRequested)
        {
            // The first monitored writer won; observe its recorded operation and reconcile exactly that event.
            Assert.Equal(GrantWriteStatus.Unavailable, outcomes[0]);
            Assert.Equal(GrantWriteStatus.Reconciled, await writer.InsertAsync(first, setup));
        }
        else
        {
            Assert.Equal(GrantWriteStatus.Unavailable, await reader.InsertAsync(ack, cancelled));
            Assert.True(committed.IsCancellationRequested);
            Assert.Equal(GrantWriteStatus.Reconciled, await writer.InsertAsync(ack, setup));
        }
        Assert.NotEmpty(concerns);
        Assert.All(concerns, concern => Assert.True(concern));

        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await Task.WhenAll(Revoke(), Retry());
        Assert.False((await adapter.ReadAsync(id, setup))!.Enabled);
        Assert.Equal(GrantWriteStatus.Reconciled, await writer.TransitionAsync(revoke, setup));
        Assert.Equal(GrantWriteStatus.Conflict, await writer.InsertAsync(first with { OperationId = Guid.NewGuid() }, setup));
        Assert.Equal(2, await events.CountDocumentsAsync(new BsonDocument("userId", Uuid(id)), cancellationToken: setup.Token));
        Assert.Equal(2, (await reader.ReadAsync(id, setup)).Grant!.Revision);
        Assert.Equal(GrantWriteStatus.Unavailable, await writer.TransitionAsync(revoke with { ReasonCode = "test.changed" }, setup));
        var reinstatement = new EligibilityTransition(Guid.NewGuid(), id, GrantStatus.Revoked, 2, "test.actor", Policy,
            "test.reinstatement", now.AddMilliseconds(2));
        Assert.Equal(GrantWriteStatus.Confirmed, await writer.TransitionAsync(reinstatement, setup));
        var reinstated = (await reader.ReadAsync(id, setup)).Grant!;
        Assert.Equal(3, reinstated.Revision);
        Assert.Equal(now, reinstated.RulesAcceptedAt);
        Assert.Equal(Rules, reinstated.RulesVersion);
        Assert.True((await adapter.ReadAsync(id, setup))!.Enabled);

        // An audit uniqueness failure must abort an already dispatched grant insert.
        var abort = first with { OperationId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        await events.InsertOneAsync(new BsonDocument("_id", Uuid(abort.OperationId)), cancellationToken: setup.Token);
        Assert.Equal(GrantWriteStatus.Unavailable, await writer.InsertAsync(abort, setup));
        Assert.Equal(GrantReadStatus.Missing, (await reader.ReadAsync(abort.UserId, setup)).Status);
        await grants.UpdateOneAsync(Filter(id), Builders<BsonDocument>.Update.Set("revision", new BsonInt32(3)),
            cancellationToken: setup.Token);
        Assert.Null(await adapter.ReadAsync(id, setup));
        Assert.NotEmpty(finds);
        Assert.All(finds, read => Assert.Equal((true, true, true), read));

        async Task<GrantWriteStatus> Compete(EnrollmentDecision decision, MongoRescuerEligibilityStore store, RequestBudget budget)
        {
            await start.Task;
            return await store.InsertAsync(decision, budget);
        }
        async Task Revoke()
        {
            gate.SetResult();
            Assert.Equal(GrantWriteStatus.Confirmed, await writer.TransitionAsync(revoke, setup));
        }
        async Task Retry()
        {
            await gate.Task;
            using var budget = new RequestBudget(10000, default);
            Assert.Equal(GrantWriteStatus.Conflict, await reader.InsertAsync(first with { OperationId = Guid.NewGuid() }, budget));
        }
    }

    [Fact]
    public async Task Controlled_enrollment_and_delivery_preserve_mode_and_continuity_after_revocation()
    {
        var quota = new TestQuota();
        await using var scope = await RegistrationTestScope.CreateAsync(fixture, initializeQuota: false, probeRegistrationStore: false,
            configureServices: services =>
            {
                services.AddSingleton(p => new MongoRescuerEligibilityStore(p.GetRequiredService<IMongoDatabase>(),
                    p.GetRequiredService<IOptions<MongoDbOptions>>().Value));
                services.AddSingleton<IRescuerEnrollmentStore>(p => p.GetRequiredService<MongoRescuerEligibilityStore>());
                services.AddSingleton(p => new DeliveryReadBarrier(new MongoRescuerEligibility(p.GetRequiredService<MongoRescuerEligibilityStore>())));
                services.Replace(ServiceDescriptor.Singleton<IRescuerEligibility>(p => p.GetRequiredService<DeliveryReadBarrier>()));
                services.Replace(ServiceDescriptor.Scoped<IRescuerEnrollment, RescuerEnrollmentService>());
                services.AddSingleton<IRescuerEnrollmentQuota>(quota);
                services.AddSingleton(new EnrollmentPolicy(Rules, Policy));
            });
        var store = scope.Services.GetRequiredService<MongoRescuerEligibilityStore>();
        using var setup = new RequestBudget(20000, default);
        await store.PrepareAsync(setup);
        await WarmRedisAsync(scope, setup);
        var owner = await SeedAsync(scope, "enroll-owner@uttt.edu.mx", setup);
        var rescuer = await SeedAsync(scope, "enroll-rescuer@uttt.edu.mx", setup);
        using (var response = await SendAsync(scope, rescuer.Token, HttpMethod.Get, Users))
        using (var initial = await JsonAsync(response, 200))
        {
            Assert.False(initial.RootElement.GetProperty("rescuerEnabled").GetBoolean());
            Assert.Equal("Solicitante", initial.RootElement.GetProperty("activeRole").GetString());
        }

        var order = await CreateOrder();
        await Reject("/accept", 403, "forbidden", order);
        await Reject(Enrollment, 409, "rescuer_phone_required", json: Consent);
        using (var response = await SendAsync(scope, rescuer.Token, HttpMethod.Patch, Users, payload: """{"phone":"+525512345678"}"""))
        using (var patched = await JsonAsync(response, 200)) Assert.False(patched.RootElement.GetProperty("rescuerEnabled").GetBoolean());
        quota.Status = EnrollmentQuotaStatus.Unavailable;
        await Reject(Enrollment, 503, "temporarily_unavailable", json: Consent);
        quota.Status = EnrollmentQuotaStatus.Limited;
        using (var limited = await SendAsync(scope, rescuer.Token, HttpMethod.Post, Enrollment, payload: Consent))
        {
            await scope.AssertProblemAsync(limited, 429, "rate_limited");
            Assert.Equal(TimeSpan.FromSeconds(3), limited.Headers.RetryAfter?.Delta);
        }
        quota.Status = EnrollmentQuotaStatus.Allowed;
        await Reject(Enrollment, 409, "rescuer_rules_version_conflict", json: Consent.Replace(Rules, "test.other"));
        await ConfirmEnrollment();
        await ConfirmEnrollment();
        using (var response = await SendAsync(scope, rescuer.Token, HttpMethod.Get, Users))
        using (var profile = await JsonAsync(response, 200))
        {
            Assert.True(profile.RootElement.GetProperty("rescuerEnabled").GetBoolean());
            Assert.Equal("Solicitante", profile.RootElement.GetProperty("activeRole").GetString());
        }
        await Step(order, "accept", rescuer.Token, "Aceptado");
        await Step(order, "accept", rescuer.Token, "Aceptado");

        var concurrent = await CreateOrder();
        var barrier = scope.Services.GetRequiredService<DeliveryReadBarrier>();
        barrier.Hold = true;
        var overlap = Step(concurrent, "accept", rescuer.Token, "Aceptado");
        try
        {
            await barrier.Observed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var revoke = new EligibilityTransition(Guid.NewGuid(), rescuer.Id, GrantStatus.Enabled, 1, "test.actor", Policy,
                "test.reason", DateTime.UtcNow);
            Assert.Equal(GrantWriteStatus.Confirmed, await store.TransitionAsync(revoke, setup));
        }
        finally { barrier.Release.SetResult(); }
        await overlap;
        await Reject(Enrollment, 403, "forbidden", json: Consent);
        await Reject("/accept", 403, "forbidden", await CreateOrder());
        await Reject("/accept", 403, "forbidden", order);
        using (var assignedResponse = await SendAsync(scope, rescuer.Token, HttpMethod.Get, Root + "/assigned-to-me"))
        using (var assigned = await JsonAsync(assignedResponse, 200)) Assert.Equal(2, assigned.RootElement.GetProperty("items").GetArrayLength());
        await Step(order, "start-delivery", rescuer.Token, "En camino");
        await Step(order, "arrive", rescuer.Token, "Llegué");
        await Step(order, "request-completion", rescuer.Token, "Esperando confirmación");
        await Step(order, "confirm-completion", owner.Token, "Completado");

        var untouched = await CreateOrder();
        await scope.Database.GetCollection<BsonDocument>(MongoRescuerEligibilityStore.Grants)
            .UpdateOneAsync(Filter(rescuer.Id), Builders<BsonDocument>.Update.Set("status", "unknown"), cancellationToken: setup.Token);
        await Reject(Enrollment, 503, "temporarily_unavailable", json: Consent);
        await Reject("/accept", 503, "temporarily_unavailable", untouched);
        using (var untouchedResponse = await SendAsync(scope, owner.Token, HttpMethod.Get, $"{Root}/{untouched:D}"))
        using (var json = await JsonAsync(untouchedResponse, 200)) Assert.Equal("Publicado", json.RootElement.GetProperty("status").GetString());

        async Task ConfirmEnrollment()
        {
            using var response = await SendAsync(scope, rescuer.Token, HttpMethod.Post, Enrollment, payload: Consent);
            using var result = await JsonAsync(response, 200);
            Assert.True(result.RootElement.GetProperty("rescuerEnabled").GetBoolean());
            Assert.Equal(JsonValueKind.Null, result.RootElement.GetProperty("rescuerLevel").ValueKind);
        }
        async Task<Guid> CreateOrder()
        {
            using var response = await SendAsync(scope, owner.Token, HttpMethod.Post, Root, Guid.NewGuid(), Payload);
            using var json = await JsonAsync(response, 201);
            return json.RootElement.GetProperty("id").GetGuid();
        }
        async Task Reject(string path, int status, string code, Guid? id = null, string? json = null)
        {
            using var response = await SendAsync(scope, rescuer.Token, HttpMethod.Post, id is { } orderId ? $"{Root}/{orderId:D}{path}" : path, payload: json);
            await scope.AssertProblemAsync(response, status, code);
        }
        async Task Step(Guid id, string action, string token, string state)
        {
            using var response = await SendAsync(scope, token, HttpMethod.Post, $"{Root}/{id:D}/{action}");
            using var json = await JsonAsync(response, 200);
            Assert.Equal(state, json.RootElement.GetProperty("status").GetString());
        }
    }

    private static BsonBinaryData Uuid(Guid id) => new(id, GuidRepresentation.Standard);
    private static BsonDocument Filter(Guid id) => new("_id", Uuid(id));

    private static async Task WarmRedisAsync(RegistrationTestScope scope, RequestBudget setup)
    {
        var redis = await scope.Services.GetRequiredService<RedisConnections>().GetMultiplexerAsync(setup.Token);
        await redis.GetDatabase().PingAsync().WaitAsync(setup.Token);
    }

    private static void UseTestEnrollment(IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Scoped<IRescuerEnrollment, RescuerEnrollmentService>());
        services.AddSingleton<IRescuerEnrollmentQuota>(new TestQuota());
        services.AddSingleton(new EnrollmentPolicy(Rules, Policy));
    }

    private static async Task SetPhoneAsync(RegistrationTestScope scope, string token)
    {
        using var response = await SendAsync(scope, token, HttpMethod.Patch, Users, payload: """{"phone":"+525512345678"}""");
        using var body = await JsonAsync(response, 200);
        Assert.False(body.RootElement.GetProperty("rescuerEnabled").GetBoolean());
    }

    private sealed class FirstInsertBarrier(IRescuerEnrollmentStore inner) : IRescuerEnrollmentStore
    {
        private int insertAttempts, observedAbsence;
        internal int InsertAttempts => Volatile.Read(ref insertAttempts);
        internal TaskCompletionSource<Guid> BeforeFirstWrite { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<GrantRead> ReadAsync(Guid id, RequestBudget budget)
        {
            var result = await inner.ReadAsync(id, budget);
            if (result.Status == GrantReadStatus.Missing) Volatile.Write(ref observedAbsence, 1);
            return result;
        }

        public async Task<GrantWriteStatus> InsertAsync(EnrollmentDecision decision, RequestBudget budget)
        {
            if (Interlocked.Increment(ref insertAttempts) == 1)
            {
                Assert.Equal(1, Volatile.Read(ref observedAbsence));
                BeforeFirstWrite.SetResult(decision.UserId);
                await Release.Task.WaitAsync(budget.Token);
            }
            return await inner.InsertAsync(decision, budget);
        }
    }

    private sealed class TestQuota : IRescuerEnrollmentQuota
    {
        internal EnrollmentQuotaStatus Status { get; set; } = EnrollmentQuotaStatus.Allowed;
        public ValueTask<EnrollmentQuotaResult> ConsumeAsync(Guid id, RequestBudget budget) =>
            ValueTask.FromResult(new EnrollmentQuotaResult(Status, TimeSpan.FromSeconds(3)));
    }

    private sealed class DeliveryReadBarrier(IRescuerEligibility inner) : IRescuerEligibility
    {
        internal bool Hold { get; set; }
        internal TaskCompletionSource Observed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<RescuerEligibility?> ReadAsync(Guid id, RequestBudget budget)
        {
            var permission = await inner.ReadAsync(id, budget);
            if (Hold && permission?.Enabled == true)
            {
                Hold = false;
                Observed.SetResult();
                await Release.Task.WaitAsync(budget.Token);
            }
            return permission;
        }
    }
}
