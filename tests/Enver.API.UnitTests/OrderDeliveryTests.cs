using Enver.API.Features.Orders.Application;
using Enver.API.Features.Orders.Core;
using Enver.API.Features.Users.Application;
using Enver.API.Http;

namespace Enver.API.UnitTests;

public sealed class OrderDeliveryTests
{
    [Theory]
    [InlineData(false, OrderOutcome.Forbidden)]
    [InlineData(null, OrderOutcome.Unavailable)]
    public async Task Accept_fails_closed_before_touching_orders(bool? enabled, OrderOutcome expected)
    {
        var store = new DeliveryStore();
        var eligibility = new Eligibility(enabled is { } value ? new(value, null) : null);
        var service = new OrderDeliveryService(store, eligibility, TimeProvider.System);
        using var budget = new RequestBudget(3000, default);
        var actor = Guid.NewGuid();

        Assert.Equal(expected, (await service.AdvanceAsync(Guid.NewGuid(), actor, OrderDeliveryAction.Accept, budget)).Outcome);
        Assert.Equal(actor, eligibility.CheckedActor);
        Assert.Equal(0, store.Attempts);
        Assert.Equal(0, store.Reads);
    }

    [Theory]
    [InlineData(true, true, OrderOutcome.Ok)]
    [InlineData(true, false, OrderOutcome.Unavailable)]
    [InlineData(false, true, OrderOutcome.Unavailable)]
    [InlineData(false, false, OrderOutcome.Unavailable)]
    public async Task Uncertain_write_needs_confirmed_mark_from_same_actor(bool hasMark, bool sameActor, OrderOutcome expected)
    {
        var actor = Guid.NewGuid();
        var other = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var current = new Order(Guid.NewGuid(), Guid.NewGuid(), actor, "Público",
            new(OrderCategory.Food, "Título", null, 30m, new("Lugar", null, null, null, null), DeliveryPreference.Now, null),
            Order.V1Currency, OrderStatus.Completed, now, now, null)
        {
            Delivery = hasMark ? new(null, null, new(sameActor ? actor : other, now), null, null) : null
        };
        var store = new DeliveryStore { Snapshot = current };
        var eligibility = new Eligibility(null);
        var service = new OrderDeliveryService(store, eligibility, TimeProvider.System);
        using var budget = new RequestBudget(3000, default);

        var result = await service.AdvanceAsync(current.Id, actor, OrderDeliveryAction.Arrive, budget);

        Assert.Equal(expected, result.Outcome);
        Assert.Equal(expected == OrderOutcome.Ok ? current : null, result.Order);
        Assert.Equal(1, store.Attempts);
        Assert.Equal(1, store.Reads);
        Assert.Null(eligibility.CheckedActor);
    }

    [Fact]
    public async Task Uncertain_write_cannot_report_not_found_when_a_read_shows_no_order()
    {
        var store = new DeliveryStore();
        var service = new OrderDeliveryService(store, new Eligibility(new(true, null)), TimeProvider.System);
        using var budget = new RequestBudget(3000, default);

        var result = await service.AdvanceAsync(Guid.NewGuid(), Guid.NewGuid(), OrderDeliveryAction.Accept, budget);

        Assert.Equal(OrderOutcome.Unavailable, result.Outcome);
        Assert.Null(result.Order);
        Assert.Equal(1, store.Attempts);
        Assert.Equal(1, store.Reads);
    }

    private sealed class Eligibility(RescuerEligibility? status) : IRescuerEligibility
    {
        internal Guid? CheckedActor { get; private set; }
        public ValueTask<RescuerEligibility?> ReadAsync(Guid id, RequestBudget budget)
        {
            CheckedActor = id;
            return ValueTask.FromResult(status);
        }
    }

    private sealed class DeliveryStore : IOrderStore
    {
        internal Order? Snapshot { get; init; }
        internal int Attempts { get; private set; }
        internal int Reads { get; private set; }

        public Task<OrderStoreResult<Order>> AdvanceDeliveryAsync(Guid id, Guid actor, OrderDeliveryAction action, DateTime now, RequestBudget budget)
        {
            Attempts++;
            return Task.FromResult(new OrderStoreResult<Order>(OrderStoreStatus.Unavailable));
        }

        public Task<OrderStoreResult<Order>> FindVisibleAsync(Guid id, Guid viewer, RequestBudget budget)
        {
            Reads++;
            return Task.FromResult(Snapshot is { } order ?
                new OrderStoreResult<Order>(OrderStoreStatus.Confirmed, order) : new(OrderStoreStatus.Missing));
        }

        public Task<OrderStoreResult<OrderCreation>> FindCreationAsync(Guid owner, Guid key, RequestBudget budget) => throw new NotSupportedException();
        public Task<OrderStoreResult<Order>> InsertAsync(OrderCreation creation, RequestBudget budget) => throw new NotSupportedException();
        public Task<OrderStoreResult<Order>> FindOwnedAsync(Guid id, Guid owner, RequestBudget budget) => throw new NotSupportedException();
        public Task<OrderStoreResult<Order>> CancelPublishedAsync(Guid id, Guid owner, DateTime now, RequestBudget budget) => throw new NotSupportedException();
        public Task<OrderStoreResult<IReadOnlyList<Order>>> ListAsync(OrderListing listing, Guid viewer, OrderPosition? after, int take,
            RequestBudget budget) => throw new NotSupportedException();
    }
}
