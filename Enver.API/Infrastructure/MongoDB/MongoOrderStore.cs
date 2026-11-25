using System.Linq.Expressions;
using Enver.API.Configuration;
using Enver.API.Features.Orders.Application;
using Enver.API.Features.Orders.Core;
using Enver.API.Http;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Enver.API.Infrastructure.MongoDB;

public sealed class MongoOrderStore(IMongoDatabase database, MongoDbOptions options) : IOrderStore
{
    public const string CollectionName = "orders";
    private readonly IMongoCollection<OrderDocument> collection = database.GetCollection<OrderDocument>(CollectionName);
    private static readonly FilterDefinitionBuilder<OrderDocument> Filter = Builders<OrderDocument>.Filter;
    private static readonly SortDefinition<OrderDocument> Recent = Builders<OrderDocument>.Sort.Descending(x => x.CreatedAt).Descending(x => x.Id);
    private readonly SemaphoreSlim indexGate = new(1);
    private volatile bool indexesReady;

    private async Task EnsureIndexesAsync(CancellationToken token)
    {
        if (indexesReady) return;
        await indexGate.WaitAsync(token);
        try
        {
            if (indexesReady) return;
            var k = Builders<OrderDocument>.IndexKeys;
            await collection.Indexes.CreateManyAsync([
                new(k.Ascending(x => x.Status).Descending(x => x.CreatedAt).Descending(x => x.Id), new() { Name = "feed_page" }),
                new(k.Ascending(x => x.OwnerId).Descending(x => x.CreatedAt).Descending(x => x.Id), new() { Name = "owner_page" }),
                new(k.Ascending(x => x.RescuerId).Descending(x => x.CreatedAt).Descending(x => x.Id), new() { Name = "rescuer_page" }),
                new(k.Ascending(x => x.OwnerId).Ascending(x => x.IdempotencyKey), new() { Name = "owner_create_key", Unique = true }),
                new(k.Ascending(x => x.MediaId), new CreateIndexOptions<OrderDocument>
                    { Name = "media_attachment", Unique = true, PartialFilterExpression = Filter.Type(x => x.MediaId, BsonType.Binary) })
            ], cancellationToken: token).WaitAsync(token);
            indexesReady = true;
        }
        finally { indexGate.Release(); }
    }

    private async Task<OrderStoreResult<T>> RunAsync<T>(Func<CancellationToken, Task<T?>> call, RequestBudget budget)
        where T : class
    {
        using var operation = budget.BeginDependency(options.OperationTimeoutMilliseconds);
        try
        {
            operation.ThrowIfCancellationRequested();
            var value = await call(operation.Token).WaitAsync(operation.Token);
            operation.ThrowIfCancellationRequested();
            return value is null ? new(OrderStoreStatus.Missing) : new(OrderStoreStatus.Confirmed, value);
        }
        catch (MongoWriteException ex) when (ex.WriteConcernError is null && ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        { return new(OrderStoreStatus.Duplicate); }
        catch (Exception ex) when (ex is MongoException or TimeoutException or OperationCanceledException or FormatException)
        { return new(OrderStoreStatus.Unavailable); }
    }

    public Task<OrderStoreResult<OrderCreation>> FindCreationAsync(Guid owner, Guid key, RequestBudget budget) =>
        RunAsync<OrderCreation>(async token =>
        {
            var row = await collection.Find(Filter.Eq(x => x.OwnerId, owner) & Filter.Eq(x => x.IdempotencyKey, key))
                .FirstOrDefaultAsync(token);
            return row is null ? null : new(row.ToOrder(), row.IdempotencyKey, row.Fingerprint);
        }, budget);

    public async Task<OrderStoreResult<Order>> InsertAsync(OrderCreation creation, RequestBudget budget)
    {
        // Antes del primer insert confirmado, cada instancia comprueba el índice único en Mongo.
        var ready = await RunAsync<object>(async token => { await EnsureIndexesAsync(token); return this; }, budget);
        if (ready.Status != OrderStoreStatus.Confirmed) return new(OrderStoreStatus.Unavailable);
        return await RunAsync<Order>(async token =>
        {
            await collection.InsertOneAsync(OrderDocument.From(creation), cancellationToken: token);
            return creation.Order;
        }, budget);
    }

    public Task<OrderStoreResult<Order>> FindVisibleAsync(Guid id, Guid viewer, RequestBudget budget) =>
        FindAsync(Filter.Eq(x => x.Id, id) & (Filter.Eq(x => x.Status, OrderStatus.Published) |
            Filter.Eq(x => x.OwnerId, viewer) | Filter.Eq(x => x.RescuerId, viewer)), budget);

    public Task<OrderStoreResult<Order>> FindOwnedAsync(Guid id, Guid owner, RequestBudget budget) =>
        FindAsync(Filter.Eq(x => x.Id, id) & Filter.Eq(x => x.OwnerId, owner), budget);

    private Task<OrderStoreResult<Order>> FindAsync(FilterDefinition<OrderDocument> filter, RequestBudget budget) =>
        RunAsync<Order>(async token => (await collection.Find(filter).FirstOrDefaultAsync(token))?.ToOrder(), budget);

    public Task<OrderStoreResult<IReadOnlyList<Order>>> ListAsync(OrderListing listing, Guid viewer, OrderPosition? after,
        int take, RequestBudget budget) => RunAsync<IReadOnlyList<Order>>(async token =>
    {
        var filter = listing switch
        {
            OrderListing.Feed => Filter.Eq(x => x.Status, OrderStatus.Published),
            OrderListing.Mine => Filter.Eq(x => x.OwnerId, viewer),
            OrderListing.AssignedToMe => Filter.Eq(x => x.RescuerId, viewer),
            _ => throw new ArgumentOutOfRangeException(nameof(listing))
        };
        if (after is { } cursor)
            filter &= Filter.Lt(x => x.CreatedAt, cursor.CreatedAt) |
                (Filter.Eq(x => x.CreatedAt, cursor.CreatedAt) & Filter.Lt(x => x.Id, cursor.Id));
        var rows = await collection.Find(filter).Sort(Recent).Limit(take).ToListAsync(token);
        return rows.Select(x => x.ToOrder()).ToArray();
    }, budget);

    private Task<OrderStoreResult<Order>> UpdateAsync(FilterDefinition<OrderDocument> filter,
        UpdateDefinition<OrderDocument> update, RequestBudget budget) =>
        RunAsync<Order>(async token =>
        {
            var row = await collection.FindOneAndUpdateAsync(filter, update,
                new FindOneAndUpdateOptions<OrderDocument> { ReturnDocument = ReturnDocument.After }, token);
            return row?.ToOrder();
        }, budget);

    public Task<OrderStoreResult<Order>> CancelPublishedAsync(Guid id, Guid owner, DateTime now, RequestBudget budget) =>
        UpdateAsync(Filter.Eq(x => x.Id, id) & Filter.Eq(x => x.OwnerId, owner) & Filter.Eq(x => x.Status, OrderStatus.Published),
            Builders<OrderDocument>.Update.Set(x => x.Status, OrderStatus.Cancelled)
                .Max(x => x.UpdatedAt, now).Set(x => x.CancelledAt, now), budget);

    public Task<OrderStoreResult<Order>> AdvanceDeliveryAsync(Guid id, Guid actor, OrderDeliveryAction action, DateTime now,
        RequestBudget budget)
    {
        var (previous, next) = OrderDeliveryTransitions.For(action);
        Expression<Func<OrderDocument, OrderDeliveryMarkDocument?>> selectMark = action switch
        {
            OrderDeliveryAction.Accept => x => x.Delivery!.Accepted,
            OrderDeliveryAction.Start => x => x.Delivery!.Started,
            OrderDeliveryAction.Arrive => x => x.Delivery!.Arrived,
            OrderDeliveryAction.RequestCompletion => x => x.Delivery!.CompletionRequested,
            OrderDeliveryAction.ConfirmCompletion => x => x.Delivery!.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };
        var mark = new ExpressionFieldDefinition<OrderDocument, OrderDeliveryMarkDocument?>(selectMark);
        var actorFilter = action switch
        {
            OrderDeliveryAction.Accept => Filter.Ne(x => x.OwnerId, actor) & Filter.Eq(x => x.RescuerId, null),
            OrderDeliveryAction.ConfirmCompletion => Filter.Eq(x => x.OwnerId, actor) &
                Filter.Ne(x => x.RescuerId, null) & Filter.Ne(x => x.RescuerId, actor),
            _ => Filter.Eq(x => x.RescuerId, actor)
        };
        var filter = Filter.Eq(x => x.Id, id) & Filter.Eq(x => x.Status, previous) & actorFilter & Filter.Eq(mark, null);
        var update = Builders<OrderDocument>.Update.Set(x => x.Status, next).Max(x => x.UpdatedAt, now)
            .Set(mark, new OrderDeliveryMarkDocument { ActorId = actor, At = now });
        if (action == OrderDeliveryAction.Accept) update = update.Set(x => x.RescuerId, actor);
        return UpdateAsync(filter, update, budget);
    }
}
