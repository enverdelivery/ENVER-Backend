using System.Text.Json;
using Enver.API.Configuration;
using Enver.API.Features.Ratings;
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
using static Enver.API.IntegrationTests.OrdersIntegrationTests;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RatingsIntegrationTests(AuthenticationDependenciesFixture fixture)
{
    [Fact]
    public Task Completed_delivery_accepts_owner_only_and_concurrent_retries_preserve_winner() => Mongo024IntegrationScope.SafelyAsync(async () =>
    {
        var eligibility = new FixtureEligibility();
        await using var scope = await RegistrationTestScope.CreateAsync(fixture, initializeQuota: false, probeRegistrationStore: false,
            configureServices: services =>
            {
                services.Replace(ServiceDescriptor.Singleton<IRescuerEligibility>(eligibility));
                services.AddSingleton(sp => new LoseOneAck(new MongoRatingStore(sp.GetRequiredService<IMongoDatabase>(),
                    sp.GetRequiredService<IOptions<MongoDbOptions>>().Value)));
                services.Replace(ServiceDescriptor.Singleton<IRatingStore>(sp => sp.GetRequiredService<LoseOneAck>()));
            });
        using var setup = new RequestBudget(15000, default);
        var redis = await scope.Services.GetRequiredService<RedisConnections>().GetMultiplexerAsync(setup.Token);
        await redis.GetDatabase().PingAsync().WaitAsync(setup.Token);
        var owner = await SeedAsync(scope, "rating-owner@uttt.edu.mx", setup);
        var rescuer = await SeedAsync(scope, "rating-rescuer@uttt.edu.mx", setup);
        var stranger = await SeedAsync(scope, "rating-stranger@uttt.edu.mx", setup);
        eligibility.Rescuer = rescuer.Id;
        var orderId = await Create();
        var path = $"{Root}/{orderId:D}/rating";
        const string scoreOnly = """{"score":5}""";
        await Reject(path, owner.Token, 409, "order_state_conflict", scoreOnly);
        await Reject(path, stranger.Token, 403, "forbidden", scoreOnly); // The published order is visible.
        await Complete(orderId);
        await Reject(path, rescuer.Token, 403, "forbidden", scoreOnly);
        await Reject(path, stranger.Token, 404, "not_found", scoreOnly);
        await Reject($"{Root}/{Guid.NewGuid():D}/rating", owner.Token, 404, "not_found", scoreOnly);
        await Reject($"{Root}/invalid/rating", owner.Token, 400, "validation_failed", scoreOnly);
        await Reject(path, owner.Token, 400, "validation_failed", """{"score":6}""");
        await Reject(path, owner.Token, 400, "validation_failed", """{"score":1,"score":5}""");
        await Reject(path, owner.Token, 400, "validation_failed", $$"""{"score":5,"recipientId":"{{stranger.Id:D}}"}""");
        await Reject(path, owner.Token, 400, "validation_failed", JsonSerializer.Serialize(new { score = 5, comment = new string('x', 501) }));
        await Reject(path, owner.Token, 413, "payload_too_large", new string(' ', 8193));

        var rows = scope.Database.GetCollection<BsonDocument>(MongoRatingStore.CollectionName);
        Assert.Equal(0, await rows.CountDocumentsAsync(new BsonDocument()));
        const string normalized = """{"score":5,"comment":"Entrega rápida"}""";
        var results = await Task.WhenAll(
            SendAsync(scope, owner.Token, HttpMethod.Post, path, payload: """{"score":5,"comment":" Entrega rápida "}"""),
            SendAsync(scope, owner.Token, HttpMethod.Post, path, payload: normalized));
        using var created = Assert.Single(results, r => (int)r.StatusCode == 201);
        using var replayed = Assert.Single(results, r => (int)r.StatusCode == 200);
        using var json = await JsonAsync(created, 201);
        using var replay = await JsonAsync(replayed, 200);
        var reply = json.RootElement;
        Assert.Equal(reply.GetRawText(), replay.RootElement.GetRawText());
        Assert.Equal(new[] { "comment", "createdAt", "id", "orderId", "score" }, reply.EnumerateObject().Select(p => p.Name).Order().ToArray());
        Assert.Equal(orderId, reply.GetProperty("orderId").GetGuid());
        Assert.Equal(5, reply.GetProperty("score").GetInt32());
        Assert.Equal("Entrega rápida", reply.GetProperty("comment").GetString());
        Assert.Equal(DateTimeKind.Utc, reply.GetProperty("createdAt").GetDateTime().Kind);
        await Reject(path, owner.Token, 409, "rating_already_exists", scoreOnly);
        var row = await rows.Find(new BsonDocument()).SingleAsync();
        Assert.Equal(reply.GetProperty("id").GetGuid(), row["_id"].AsBsonBinaryData.ToGuid());
        Assert.Equal(orderId, row["orderId"].AsBsonBinaryData.ToGuid());
        Assert.Equal(owner.Id, row["authorId"].AsBsonBinaryData.ToGuid());
        Assert.Equal(rescuer.Id, row["recipientId"].AsBsonBinaryData.ToGuid());
        Assert.Equal(5, row["score"].AsInt32);
        Assert.Equal("Entrega rápida", row["comment"].AsString);
        Assert.Equal(reply.GetProperty("createdAt").GetDateTime(), row["createdAt"].AsBsonDateTime.ToUniversalTime());
        using var indexes = await rows.Indexes.ListAsync();
        var relation = Assert.Single(await indexes.ToListAsync(), x => x["name"].AsString == "rating_relation");
        Assert.True(relation["unique"].AsBoolean);
        Assert.Equal(new BsonDocument { { "orderId", 1 }, { "authorId", 1 }, { "recipientId", 1 } }, relation["key"].AsBsonDocument);

        var otherId = await Create();
        await Complete(otherId);
        var other = $"{Root}/{otherId:D}/rating";
        scope.Services.GetRequiredService<LoseOneAck>().Arm();
        await Reject(other, owner.Token, 503, "temporarily_unavailable", scoreOnly);
        using var recovered = await SendAsync(scope, owner.Token, HttpMethod.Post, other, payload: """{"score":5,"comment":" "}""");
        using var recoveredJson = await JsonAsync(recovered, 200);
        Assert.Equal(JsonValueKind.Null, recoveredJson.RootElement.GetProperty("comment").ValueKind);
        Assert.Equal(2, await rows.CountDocumentsAsync(new BsonDocument()));

        async Task<Guid> Create()
        {
            using var response = await SendAsync(scope, owner.Token, HttpMethod.Post, Root, Guid.NewGuid(), Payload);
            using var body = await JsonAsync(response, 201);
            return body.RootElement.GetProperty("id").GetGuid();
        }

        async Task Complete(Guid id)
        {
            foreach (var (step, token) in new[] { ("accept", rescuer.Token), ("start-delivery", rescuer.Token),
                ("arrive", rescuer.Token), ("request-completion", rescuer.Token), ("confirm-completion", owner.Token) })
            {
                using var response = await SendAsync(scope, token, HttpMethod.Post, $"{Root}/{id:D}/{step}");
                using var body = await JsonAsync(response, 200);
                if (step == "confirm-completion") Assert.Equal("Completado", body.RootElement.GetProperty("status").GetString());
            }
        }

        async Task Reject(string url, string token, int status, string code, string payload)
        {
            using var response = await SendAsync(scope, token, HttpMethod.Post, url, payload: payload);
            await scope.AssertProblemAsync(response, status, code);
        }
    });

    private sealed class FixtureEligibility : IRescuerEligibility
    {
        public Guid Rescuer { get; set; }
        public ValueTask<RescuerEligibility?> ReadAsync(Guid id, RequestBudget budget) => ValueTask.FromResult<RescuerEligibility?>(new(id == Rescuer, null));
    }

    private sealed class LoseOneAck(IRatingStore inner) : IRatingStore
    {
        private int armed;
        public void Arm() => Interlocked.Exchange(ref armed, 1);

        public async Task<RatingInsert> InsertAsync(Rating rating, RequestBudget budget)
        {
            var result = await inner.InsertAsync(rating, budget);
            return result == RatingInsert.Confirmed && Interlocked.Exchange(ref armed, 0) == 1 ? RatingInsert.Unavailable : result;
        }

        public Task<Rating?> FindByRelationAsync(Guid order, Guid author, Guid recipient, RequestBudget budget) =>
            inner.FindByRelationAsync(order, author, recipient, budget);
    }
}
