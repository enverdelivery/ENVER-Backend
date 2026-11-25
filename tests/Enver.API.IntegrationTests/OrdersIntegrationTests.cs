using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Orders.Application;
using Enver.API.Features.Orders.Core;
using Enver.API.Features.Users.Application;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class OrdersIntegrationTests(AuthenticationDependenciesFixture fixture)
{
    internal const string Root = "/api/v1/orders";
    internal const string Payload = """
        {"selectedCategory":"Comida","orderTitle":"Torta","specialInstructions":"Sin cebolla","tarifa":"30.01",
        "locationSearch":"No persistir","locationDetail":"Salón 302","selectedLat":20,"selectedLng":-99,
        "selectedAddressLabel":"Edificio B","selectedHabitualPlace":"Salón habitual","deliveryTime":"Ahora","mediaId":null}
        """;

    [Fact]
    public Task Http_contract_persists_orders_and_enforces_visibility_idempotency_and_cancellation() => Mongo024IntegrationScope.SafelyAsync(async () =>
    {
        await using var scope = await RegistrationTestScope.CreateAsync(fixture, initializeQuota: false, probeRegistrationStore: false);
        using var setup = new RequestBudget(15000, default);
        // Mismo precalentamiento que la fixture del pipeline; no prepara cuotas ni reintenta requests.
        var redis = await scope.Services.GetRequiredService<RedisConnections>().GetMultiplexerAsync(setup.Token);
        await redis.GetDatabase().PingAsync().WaitAsync(setup.Token);
        var first = await SeedAsync(scope, "order-first@uttt.edu.mx", setup);
        var second = await SeedAsync(scope, "order-second@uttt.edu.mx", setup);
        var profiles = scope.Services.GetRequiredService<IUserProfileStore>();
        Assert.Equal(ProfileStoreStatus.Confirmed, (await profiles.UpdateAsync(first.Id, new("Nombre público", false, null), setup)).Status);
        var key = Guid.NewGuid();

        using (var invalid = await SendAsync(scope, first.Token, HttpMethod.Post, Root, key, Payload.Insert(1, "\"ownerId\":\"other\",")))
            await scope.AssertProblemAsync(invalid, 400, "validation_failed");
        using (var unavailable = await SendAsync(scope, first.Token, HttpMethod.Post, Root, key,
            Payload.Replace("\"mediaId\":null", $"\"mediaId\":\"{Guid.NewGuid():D}\"", StringComparison.Ordinal)))
            await scope.AssertProblemAsync(unavailable, 503, "temporarily_unavailable");

        using var created = await SendAsync(scope, first.Token, HttpMethod.Post, Root, key, Payload);
        using var creation = await JsonAsync(created, 201);
        var id = creation.RootElement.GetProperty("id").GetGuid();
        var path = $"{Root}/{id:D}";
        Assert.Equal(path, created.Headers.Location?.OriginalString);
        Assert.Equal("Publicado", creation.RootElement.GetProperty("status").GetString());
        Assert.Equal("30.01", creation.RootElement.GetProperty("cost").GetString());
        Assert.Equal("Nombre público", creation.RootElement.GetProperty("requesterName").GetString());
        Assert.Equal("Salón 302", creation.RootElement.GetProperty("location").GetProperty("detail").GetString());
        Assert.False(creation.RootElement.GetProperty("is100PercentYours").GetBoolean());
        Assert.Equal(JsonValueKind.Null, creation.RootElement.GetProperty("imageUrl").ValueKind);
        foreach (var keyName in new[] { "ownerId", "rescuerId", "email", "fingerprint", "idempotencyKey", "currency", "locationSearch" })
            Assert.False(creation.RootElement.TryGetProperty(keyName, out _));

        using (var replay = await SendAsync(scope, first.Token, HttpMethod.Post, Root, key, Payload.Replace(",\"mediaId\":null", "", StringComparison.Ordinal)))
        using (var json = await JsonAsync(replay, 200)) Assert.Equal(id, json.RootElement.GetProperty("id").GetGuid());
        using (var conflict = await SendAsync(scope, first.Token, HttpMethod.Post, Root, key, Payload.Replace("30.01", "31", StringComparison.Ordinal)))
            await scope.AssertProblemAsync(conflict, 409, "idempotency_conflict");

        using var other = await SendAsync(scope, second.Token, HttpMethod.Post, Root, key, Payload);
        using var otherCreation = await JsonAsync(other, 201);
        var otherId = otherCreation.RootElement.GetProperty("id").GetGuid();
        Assert.NotEqual(id, otherId);
        var collection = scope.Database.GetCollection<BsonDocument>(MongoOrderStore.CollectionName);
        var rows = await collection.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
        Assert.Equal(2, rows.Count);
        var primary = rows.Single(x => x["_id"].AsBsonBinaryData.ToGuid() == id);
        Assert.Equal(first.Id, primary["ownerId"].AsBsonBinaryData.ToGuid());
        Assert.Equal(BsonType.Decimal128, primary["amount"].BsonType);
        Assert.Equal(Decimal128.Parse("30.01"), primary["amount"].AsDecimal128);
        Assert.Equal("MXN", primary["currency"].AsString);
        Assert.Equal((int)OrderStatus.Published, primary["status"].AsInt32);
        Assert.True(primary["mediaId"].IsBsonNull);
        Assert.False(primary.Contains("locationSearch"));
        using (var indexCursor = await collection.Indexes.ListAsync())
        {
            var indexes = (await indexCursor.ToListAsync()).ToDictionary(x => x["name"].AsString);
            foreach (var paging in new[] { ("feed_page", "status"), ("owner_page", "ownerId"), ("rescuer_page", "rescuerId") })
                Assert.Equal(new BsonDocument { { paging.Item2, 1 }, { "createdAt", -1 }, { "_id", -1 } }, indexes[paging.Item1]["key"]);
            Assert.True(indexes["owner_create_key"]["unique"].AsBoolean);
            Assert.Equal(new BsonDocument { { "ownerId", 1 }, { "idempotencyKey", 1 } }, indexes["owner_create_key"]["key"]);
            Assert.True(indexes["media_attachment"]["unique"].AsBoolean);
            Assert.True(indexes["media_attachment"].Contains("partialFilterExpression"));
        }

        // Fijar empate de reloj en la fixture; la ruta se ordena por UUID binario descendente.
        await collection.UpdateManyAsync(FilterDefinition<BsonDocument>.Empty, Builders<BsonDocument>.Update.Set("createdAt", primary["createdAt"]));
        using (var feed = await SendAsync(scope, first.Token, HttpMethod.Get, Root + "?limit=1"))
        using (var page = await JsonAsync(feed, 200))
        {
            var expected = new[] { id, otherId }.OrderByDescending(x => x.ToString("D"), StringComparer.Ordinal).ToArray();
            var item = Assert.Single(page.RootElement.GetProperty("items").EnumerateArray());
            Assert.Equal(expected[0], item.GetProperty("id").GetGuid());
            Assert.False(item.TryGetProperty("location", out _));
            Assert.False(item.TryGetProperty("status", out _));
            var cursor = page.RootElement.GetProperty("nextCursor").GetString();
            using var continuation = await SendAsync(scope, first.Token, HttpMethod.Get, Root + "?limit=1&cursor=" + cursor);
            using var next = await JsonAsync(continuation, 200);
            Assert.Equal(expected[1], Assert.Single(next.RootElement.GetProperty("items").EnumerateArray()).GetProperty("id").GetGuid());
            Assert.Equal(JsonValueKind.Null, next.RootElement.GetProperty("nextCursor").ValueKind);
            using var foreignCursor = await SendAsync(scope, second.Token, HttpMethod.Get, Root + "?cursor=" + cursor);
            await scope.AssertProblemAsync(foreignCursor, 400, "validation_failed");
        }

        using (var reduced = await SendAsync(scope, second.Token, HttpMethod.Get, path))
        using (var json = await JsonAsync(reduced, 200))
            foreach (var field in new[] { "location", "specialInstructions", "mediaId" }) Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty(field).ValueKind);
        using (var mine = await SendAsync(scope, first.Token, HttpMethod.Get, Root + "/mine"))
        using (var json = await JsonAsync(mine, 200))
            Assert.Equal(id, Assert.Single(json.RootElement.GetProperty("items").EnumerateArray()).GetProperty("id").GetGuid());
        using (var notMine = await SendAsync(scope, second.Token, HttpMethod.Get, Root + $"/mine?ownerId={first.Id:D}"))
            await scope.AssertProblemAsync(notMine, 400, "validation_failed");
        using (var denied = await SendAsync(scope, second.Token, HttpMethod.Post, path + "/cancel"))
            await scope.AssertProblemAsync(denied, 404, "not_found");

        using var cancelled = await SendAsync(scope, first.Token, HttpMethod.Post, path + "/cancel");
        using var cancellation = await JsonAsync(cancelled, 200);
        Assert.Equal("Cancelado", cancellation.RootElement.GetProperty("status").GetString());
        var stamp = cancellation.RootElement.GetProperty("updatedAt").GetDateTime();
        using (var repeat = await SendAsync(scope, first.Token, HttpMethod.Post, path + "/cancel"))
        using (var json = await JsonAsync(repeat, 200)) Assert.Equal(stamp, json.RootElement.GetProperty("updatedAt").GetDateTime());
        using (var replay = await SendAsync(scope, first.Token, HttpMethod.Post, Root, key, Payload))
        using (var json = await JsonAsync(replay, 200)) Assert.Equal("Cancelado", json.RootElement.GetProperty("status").GetString());
        using (var hidden = await SendAsync(scope, second.Token, HttpMethod.Get, path))
            await scope.AssertProblemAsync(hidden, 404, "not_found");

        // Simular estado compartido posterior en fixture, no implementar la transición 007.
        var otherFilter = Builders<BsonDocument>.Filter.Eq("_id", new BsonBinaryData(otherId, GuidRepresentation.Standard));
        await collection.UpdateOneAsync(otherFilter, Builders<BsonDocument>.Update.Set("status", (int)OrderStatus.Accepted)
            .Set("rescuerId", new BsonBinaryData(first.Id, GuidRepresentation.Standard)));
        using (var invalidState = await SendAsync(scope, second.Token, HttpMethod.Post, $"{Root}/{otherId:D}/cancel"))
            await scope.AssertProblemAsync(invalidState, 409, "order_state_conflict");
        using (var assigned = await SendAsync(scope, first.Token, HttpMethod.Get, $"{Root}/{otherId:D}"))
        using (var json = await JsonAsync(assigned, 200))
        {
            Assert.Equal("Aceptado", json.RootElement.GetProperty("status").GetString());
            Assert.Equal("Sin cebolla", json.RootElement.GetProperty("specialInstructions").GetString());
        }
        using (var feed = await SendAsync(scope, first.Token, HttpMethod.Get, Root))
        using (var json = await JsonAsync(feed, 200)) Assert.Empty(json.RootElement.GetProperty("items").EnumerateArray());
    });

    [Fact]
    public Task Concurrent_cancellation_has_exactly_one_confirmed_mutation() => Mongo024IntegrationScope.SafelyAsync(async () =>
    {
        await using var scope = new Mongo024IntegrationScope(fixture);
        using var peer = new Mongo026Actor(scope);
        await Mongo026Evidence.WarmAsync(scope.Client);
        await Mongo026Evidence.WarmAsync(peer.Client);
        IOrderStore a = new MongoOrderStore(scope.Database, scope.Options), b = new MongoOrderStore(peer.Database, scope.Options);
        var now = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).UtcDateTime;
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), null, "Público",
            new(OrderCategory.Food, "Torta", null, 30, new("Salón", null, null, null, null), DeliveryPreference.Now, null),
            Order.V1Currency, OrderStatus.Published, now, now, null);
        var creation = new OrderCreation(order, Guid.NewGuid(), "fixture");
        using var first = new RequestBudget(10000, default);
        using var second = new RequestBudget(10000, default);
        // Ejecutar el DDL inicial antes de medir la carrera de escrituras de negocio.
        foreach (var store in new[] { a, b })
            Assert.Equal(OrderStoreStatus.Confirmed, (await store.InsertAsync(creation with
                { Order = order with { Id = Guid.NewGuid() }, IdempotencyKey = Guid.NewGuid() }, first)).Status);
        var inserts = await Task.WhenAll(a.InsertAsync(creation, first), b.InsertAsync(creation with { Order = order with { Id = Guid.NewGuid() } }, second));
        var winner = Assert.Single(inserts, x => x.Status == OrderStoreStatus.Confirmed).Value!;
        Assert.Single(inserts, x => x.Status == OrderStoreStatus.Duplicate);
        var results = await Task.WhenAll(a.CancelPublishedAsync(winner.Id, winner.OwnerId, now.AddMilliseconds(1), first),
            b.CancelPublishedAsync(winner.Id, winner.OwnerId, now.AddMilliseconds(2), second));
        var changed = Assert.Single(results, x => x.Status == OrderStoreStatus.Confirmed).Value!;
        Assert.Single(results, x => x.Status == OrderStoreStatus.Missing);
        Assert.Equal(OrderStatus.Cancelled, changed.Status);
        Assert.Equal(changed.UpdatedAt, changed.CancelledAt);
        Assert.Equal(changed, (await b.FindOwnedAsync(winner.Id, winner.OwnerId, second)).Value);

        // Simular ACK perdido al llamar al servicio: Mongo sí confirmó el primer insert.
        var uncertain = new LoseFirstInsertAck(a);
        var account = Mongo024IntegrationScope.User();
        var service = new OrderService(scope.Store, new MongoUserProfileStore(scope.Database, scope.Options), uncertain, TimeProvider.System);
        var key = Guid.NewGuid();
        using var retryBudget = new RequestBudget(10000, default);
        Assert.Equal(OrderOutcome.Unavailable, (await service.CreateAsync(account, key, order.Draft, retryBudget)).Outcome);
        Assert.Equal(OrderOutcome.Ok, (await service.CreateAsync(account, key, order.Draft, retryBudget)).Outcome);
        Assert.Equal(1, uncertain.InsertAttempts);
    });

    private sealed class LoseFirstInsertAck(IOrderStore store) : IOrderStore
    {
        public int InsertAttempts { get; private set; }
        public async Task<OrderStoreResult<Order>> InsertAsync(OrderCreation creation, RequestBudget budget)
        {
            InsertAttempts++;
            Assert.Equal(OrderStoreStatus.Confirmed, (await store.InsertAsync(creation, budget)).Status);
            return new(OrderStoreStatus.Unavailable);
        }
        public Task<OrderStoreResult<OrderCreation>> FindCreationAsync(Guid owner, Guid key, RequestBudget budget) => store.FindCreationAsync(owner, key, budget);
        public Task<OrderStoreResult<Order>> FindOwnedAsync(Guid id, Guid owner, RequestBudget budget) => throw new NotSupportedException();
        public Task<OrderStoreResult<Order>> FindVisibleAsync(Guid id, Guid viewer, RequestBudget budget) => throw new NotSupportedException();
        public Task<OrderStoreResult<Order>> CancelPublishedAsync(Guid id, Guid owner, DateTime now, RequestBudget budget) => throw new NotSupportedException();
        public Task<OrderStoreResult<Order>> AdvanceDeliveryAsync(Guid id, Guid actor, OrderDeliveryAction action, DateTime now,
            RequestBudget budget) => throw new NotSupportedException();
        public Task<OrderStoreResult<IReadOnlyList<Order>>> ListAsync(OrderListing listing, Guid viewer, OrderPosition? after, int take,
            RequestBudget budget) => throw new NotSupportedException();
    }

    internal static async Task<(Guid Id, string Token)> SeedAsync(RegistrationTestScope scope, string email, RequestBudget budget)
    {
        var user = User.Create("Nombre Auth", email, "synthetic-not-for-login", DateTime.UtcNow);
        Assert.Equal(UserInsertStatus.Confirmed, (await scope.Services.GetRequiredService<IUserStore>().InsertAsync(user, budget)).Status);
        var token = scope.Services.GetRequiredService<IAccessTokenIssuer>().Issue(user.Id);
        Assert.Equal(AccessTokenIssueStatus.Issued, token.Status);
        return (user.Id, token.AccessToken!);
    }

    internal static async Task<HttpResponseMessage> SendAsync(RegistrationTestScope scope, string token, HttpMethod method,
        string path, Guid? key = null, string? payload = null)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (key is { } id) request.Headers.Add("Idempotency-Key", id.ToString("D"));
        if (payload is not null) request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        return await scope.Client.SendAsync(request);
    }

    internal static async Task<JsonDocument> JsonAsync(HttpResponseMessage response, int status)
    {
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
