using System.Collections.Concurrent;
using System.Text.Json;
using Enver.API.Features.Orders.Application;
using Enver.API.Features.Orders.Core;
using Enver.API.Features.Users.Application;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using static Enver.API.IntegrationTests.OrdersIntegrationTests;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class DeliveryIntegrationTests(AuthenticationDependenciesFixture fixture)
{
    [Fact]
    public Task Http_lifecycle_enforces_server_eligibility_participation_paging_and_replays() => Mongo024IntegrationScope.SafelyAsync(async () =>
    {
        var eligibility = new DeliveryEligibility();
        await using var scope = await RegistrationTestScope.CreateAsync(fixture, initializeQuota: false, probeRegistrationStore: false,
            configureServices: services => services.Replace(ServiceDescriptor.Singleton<IRescuerEligibility>(eligibility)));
        using var setup = new RequestBudget(15000, default);
        var redis = await scope.Services.GetRequiredService<RedisConnections>().GetMultiplexerAsync(setup.Token);
        await redis.GetDatabase().PingAsync().WaitAsync(setup.Token);
        var owner = await SeedAsync(scope, "delivery-owner@uttt.edu.mx", setup);
        var rescuer = await SeedAsync(scope, "delivery-rescuer@uttt.edu.mx", setup);
        var stranger = await SeedAsync(scope, "delivery-stranger@uttt.edu.mx", setup);
        var id = await CreateAsync();
        var second = await CreateAsync();
        var foreign = await CreateAsync();
        var cancelled = await CreateAsync();
        var path = Path(id);

        using (var mode = await SendAsync(scope, rescuer.Token, HttpMethod.Put, "/api/v1/users/me/active-role",
            payload: """{"activeRole":"Rescatista"}"""))
        using (var json = await JsonAsync(mode, 200))
        {
            Assert.Equal("Rescatista", json.RootElement.GetProperty("activeRole").GetString());
            Assert.False(json.RootElement.GetProperty("rescuerEnabled").GetBoolean());
        }
        await Reject(HttpMethod.Post, path + "/accept", rescuer.Token, 403, "forbidden");
        eligibility.Values[rescuer.Id] = new(null);
        await Reject(HttpMethod.Post, path + "/accept", rescuer.Token, 503, "temporarily_unavailable");
        eligibility.Values[owner.Id] = new(new(true, null));
        eligibility.Values[rescuer.Id] = new(new(true, null));
        eligibility.Values[stranger.Id] = new(new(true, null));
        await Reject(HttpMethod.Post, path + "/accept", owner.Token, 403, "forbidden");
        await Reject(HttpMethod.Post, Root + "/invalid/accept", rescuer.Token, 400, "validation_failed");
        await Reject(HttpMethod.Post, path + $"/accept?rescuerId={stranger.Id:D}", rescuer.Token, 400, "validation_failed");
        await Reject(HttpMethod.Post, path + "/accept", rescuer.Token, 400, "validation_failed",
            $$"""{"assignedTo":"{{stranger.Id:D}}"}""");

        await Step(cancelled, "cancel", owner.Token, "Cancelado");
        await Reject(HttpMethod.Post, Path(cancelled) + "/accept", rescuer.Token, 404, "not_found");
        var collection = scope.Database.GetCollection<BsonDocument>(MongoOrderStore.CollectionName);
        var cancelledRow = await Row(cancelled);
        Assert.Equal((int)OrderStatus.Cancelled, cancelledRow["status"].AsInt32);
        Assert.True(cancelledRow["rescuerId"].IsBsonNull);
        Assert.False(cancelledRow.Contains("delivery"));
        Assert.False((await Row(id)).Contains("delivery"));

        await Step(id, "accept", rescuer.Token, "Aceptado");
        await Step(second, "accept", rescuer.Token, "Aceptado");
        await Step(foreign, "accept", stranger.Token, "Aceptado");
        var accepted = await Row(id);
        await Reject(HttpMethod.Post, path + "/accept", stranger.Token, 404, "not_found");
        await Reject(HttpMethod.Post, path + "/start-delivery", stranger.Token, 404, "not_found");
        await Reject(HttpMethod.Post, path + "/start-delivery", owner.Token, 403, "forbidden");
        await Reject(HttpMethod.Post, path + "/arrive", rescuer.Token, 409, "order_state_conflict");
        Assert.Equal(accepted, await Row(id));

        // El permiso de accept se revalida aun en retry, pero no bloquea la entrega ya asignada.
        eligibility.Values.TryRemove(rescuer.Id, out _);
        await Reject(HttpMethod.Post, path + "/accept", rescuer.Token, 403, "forbidden");
        await Step(id, "start-delivery", rescuer.Token, "En camino");
        await Step(id, "arrive", rescuer.Token, "Llegué");
        var arrived = await Row(id);
        await Step(id, "arrive", rescuer.Token, "Llegué");
        Assert.Equal(arrived, await Row(id));
        await Step(id, "request-completion", rescuer.Token, "Esperando confirmación");
        await Reject(HttpMethod.Post, path + "/confirm-completion", rescuer.Token, 403, "forbidden");
        await Reject(HttpMethod.Post, path + "/confirm-completion", stranger.Token, 404, "not_found");
        await Step(id, "confirm-completion", owner.Token, "Completado");

        var completed = await Row(id);
        eligibility.Values[rescuer.Id] = new(new(true, null));
        foreach (var action in new[] { "accept", "start-delivery", "arrive", "request-completion" })
            await Step(id, action, rescuer.Token, "Completado");
        await Step(id, "confirm-completion", owner.Token, "Completado");
        Assert.Equal(completed, await Row(id));
        Assert.Equal(rescuer.Id, completed["rescuerId"].AsBsonBinaryData.ToGuid());
        Assert.True(completed["cancelledAt"].IsBsonNull);
        Assert.Equal(5, completed["delivery"].AsBsonDocument.ElementCount);
        foreach (var (name, actor) in new[] { ("accepted", rescuer.Id), ("started", rescuer.Id), ("arrived", rescuer.Id),
                     ("completionRequested", rescuer.Id), ("completed", owner.Id) })
        {
            var mark = completed["delivery"][name];
            Assert.Equal(actor, mark["actorId"].AsBsonBinaryData.ToGuid());
            Assert.InRange(mark["at"].AsBsonDateTime.ToUniversalTime(), completed["createdAt"].AsBsonDateTime.ToUniversalTime(),
                completed["updatedAt"].AsBsonDateTime.ToUniversalTime());
        }

        // Empatar fecha para comprobar desempate por UUID y cursor ligado a usuario y listado.
        await collection.UpdateOneAsync(Filter(second), Builders<BsonDocument>.Update.Set("createdAt", completed["createdAt"]));
        var expected = new[] { id, second }.OrderByDescending(x => x.ToString("D"), StringComparer.Ordinal).ToArray();
        var statuses = new Dictionary<Guid, string> { [id] = "Completado", [second] = "Aceptado" };
        var assigned = Root + "/assigned-to-me";
        using var firstResponse = await SendAsync(scope, rescuer.Token, HttpMethod.Get, assigned + "?limit=1");
        using var firstPage = await JsonAsync(firstResponse, 200);
        AssertPage(firstPage, expected[0], statuses[expected[0]]);
        var cursor = firstPage.RootElement.GetProperty("nextCursor").GetString();
        Assert.NotNull(cursor);
        using var nextResponse = await SendAsync(scope, rescuer.Token, HttpMethod.Get, assigned + "?limit=1&cursor=" + cursor);
        using var nextPage = await JsonAsync(nextResponse, 200);
        AssertPage(nextPage, expected[1], statuses[expected[1]]);
        Assert.Equal(JsonValueKind.Null, nextPage.RootElement.GetProperty("nextCursor").ValueKind);
        using (var foreignResponse = await SendAsync(scope, stranger.Token, HttpMethod.Get, assigned))
        using (var foreignPage = await JsonAsync(foreignResponse, 200)) AssertPage(foreignPage, foreign, "Aceptado");
        await Reject(HttpMethod.Get, assigned + "?cursor=" + cursor, stranger.Token, 400, "validation_failed");
        await Reject(HttpMethod.Get, Root + "/mine?cursor=" + cursor, rescuer.Token, 400, "validation_failed");
        await Reject(HttpMethod.Get, assigned + $"?rescuerId={stranger.Id:D}", rescuer.Token, 400, "validation_failed");

        async Task<Guid> CreateAsync()
        {
            using var response = await SendAsync(scope, owner.Token, HttpMethod.Post, Root, Guid.NewGuid(), Payload);
            using var json = await JsonAsync(response, 201);
            return json.RootElement.GetProperty("id").GetGuid();
        }

        async Task Reject(HttpMethod method, string url, string token, int status, string code, string? payload = null)
        {
            using var response = await SendAsync(scope, token, method, url, payload: payload);
            await scope.AssertProblemAsync(response, status, code);
        }

        async Task Step(Guid order, string action, string token, string status)
        {
            using var response = await SendAsync(scope, token, HttpMethod.Post, Path(order) + "/" + action);
            using var json = await JsonAsync(response, 200);
            Assert.Equal(status, json.RootElement.GetProperty("status").GetString());
            Assert.Equal(order, json.RootElement.GetProperty("id").GetGuid());
            Assert.Equal("Salón 302", json.RootElement.GetProperty("location").GetProperty("detail").GetString());
            Assert.False(json.RootElement.TryGetProperty("delivery", out _));
            Assert.False(json.RootElement.TryGetProperty("rescuerId", out _));
        }

        Task<BsonDocument> Row(Guid order) => collection.Find(Filter(order)).SingleAsync();
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task Concurrent_accept_competes_in_Mongo_with_accept_or_cancel(bool competeWithCancel) => Mongo024IntegrationScope.SafelyAsync(async () =>
    {
        await using var scope = new Mongo024IntegrationScope(fixture);
        using var peer = new Mongo026Actor(scope);
        await Mongo026Evidence.WarmAsync(scope.Client);
        await Mongo026Evidence.WarmAsync(peer.Client);
        var first = new MongoOrderStore(scope.Database, scope.Options);
        var second = new MongoOrderStore(peer.Database, scope.Options);
        var now = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).UtcDateTime;
        var owner = Guid.NewGuid();
        var rescuerA = Guid.NewGuid();
        var rescuerB = Guid.NewGuid();
        var order = new Order(Guid.NewGuid(), owner, null, "Público",
            new(OrderCategory.Food, "Torta", null, 30, new("Salón", null, null, null, null), DeliveryPreference.Now, null),
            Order.V1Currency, OrderStatus.Published, now, now, null);
        using var setup = new RequestBudget(10000, default);
        Assert.Equal(OrderStoreStatus.Confirmed, (await first.InsertAsync(new(order, Guid.NewGuid(), "fixture"), setup)).Status);

        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var a = Compete(b => first.AdvanceDeliveryAsync(order.Id, rescuerA, OrderDeliveryAction.Accept, now.AddMilliseconds(1), b));
        var b = Compete(budget => competeWithCancel
            ? second.CancelPublishedAsync(order.Id, owner, now.AddMilliseconds(2), budget)
            : second.AdvanceDeliveryAsync(order.Id, rescuerB, OrderDeliveryAction.Accept, now.AddMilliseconds(2), budget));
        start.SetResult();
        var results = await Task.WhenAll(a, b);
        var winner = Assert.Single(results, x => x.Status == OrderStoreStatus.Confirmed).Value!;
        Assert.Null(Assert.Single(results, x => x.Status == OrderStoreStatus.Missing).Value);
        Assert.Equal(winner, (await second.FindOwnedAsync(order.Id, owner, setup)).Value);
        if (competeWithCancel && results[1].Status == OrderStoreStatus.Confirmed)
        {
            Assert.Equal(OrderStatus.Cancelled, winner.Status);
            Assert.Null(winner.RescuerId);
            Assert.Null(winner.Delivery);
            Assert.Equal(winner.UpdatedAt, winner.CancelledAt);
        }
        else
        {
            var actor = results[0].Status == OrderStoreStatus.Confirmed ? rescuerA : rescuerB;
            Assert.Equal(OrderStatus.Accepted, winner.Status);
            Assert.Equal(actor, winner.RescuerId);
            Assert.Equal(new OrderDeliveryMark(actor, winner.UpdatedAt), winner.Delivery?.Accepted);
            Assert.Null(winner.Delivery?.Started);
            Assert.Null(winner.CancelledAt);
        }

        async Task<OrderStoreResult<Order>> Compete(Func<RequestBudget, Task<OrderStoreResult<Order>>> write)
        {
            await start.Task;
            using var budget = new RequestBudget(10000, default);
            return await write(budget);
        }
    });

    private static string Path(Guid id) => $"{Root}/{id:D}";
    private static FilterDefinition<BsonDocument> Filter(Guid id) =>
        Builders<BsonDocument>.Filter.Eq("_id", new BsonBinaryData(id, GuidRepresentation.Standard));

    private static void AssertPage(JsonDocument page, Guid expected, string status)
    {
        var item = Assert.Single(page.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal(expected, item.GetProperty("id").GetGuid());
        Assert.Equal(status, item.GetProperty("status").GetString());
        Assert.False(item.TryGetProperty("location", out _));
        Assert.False(item.TryGetProperty("specialInstructions", out _));
    }

    private sealed record EligibilityResult(RescuerEligibility? Value);
    private sealed class DeliveryEligibility : IRescuerEligibility
    {
        internal ConcurrentDictionary<Guid, EligibilityResult> Values { get; } = new();
        public ValueTask<RescuerEligibility?> ReadAsync(Guid id, RequestBudget budget) =>
            ValueTask.FromResult(Values.TryGetValue(id, out var result) ? result.Value : new(false, null));
    }
}
