using Enver.API.Features.Orders.Core;
using Enver.API.Http;

namespace Enver.API.Features.Orders.Application;

public enum OrderStoreStatus { Confirmed, Missing, Duplicate, Unavailable }
public sealed record OrderStoreResult<T>(OrderStoreStatus Status, T? Value = null) where T : class;
public sealed record OrderPosition(DateTime CreatedAt, Guid Id);
public enum OrderListing { Feed, Mine }
public sealed record OrderCreation(Order Order, Guid IdempotencyKey, string Fingerprint);

public interface IOrderStore
{
    Task<OrderStoreResult<OrderCreation>> FindCreationAsync(Guid owner, Guid key, RequestBudget budget);
    Task<OrderStoreResult<Order>> InsertAsync(OrderCreation creation, RequestBudget budget);
    Task<OrderStoreResult<Order>> FindVisibleAsync(Guid id, Guid viewer, RequestBudget budget);
    Task<OrderStoreResult<IReadOnlyList<Order>>> ListAsync(OrderListing listing, Guid viewer, OrderPosition? after, int take, RequestBudget budget);
    Task<OrderStoreResult<Order>> CancelPublishedAsync(Guid id, Guid owner, DateTime now, RequestBudget budget);
    Task<OrderStoreResult<Order>> FindOwnedAsync(Guid id, Guid owner, RequestBudget budget);
}
