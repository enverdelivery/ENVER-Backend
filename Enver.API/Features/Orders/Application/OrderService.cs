using System.Security.Cryptography;
using System.Text.Json;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Orders.Contracts;
using Enver.API.Features.Orders.Core;
using Enver.API.Features.Users.Application;
using Enver.API.Http;

namespace Enver.API.Features.Orders.Application;

public enum OrderOutcome { Ok, Created, Forbidden, NotFound, StateConflict, IdempotencyConflict, Unavailable }
public sealed record OrderResult(OrderOutcome Outcome, Order? Order = null);

public sealed class OrderService(IUserStore accounts, IUserProfileStore profiles, IOrderStore orders, TimeProvider clock)
{
    public async ValueTask<(OrderOutcome Outcome, User? Account)> AuthenticateAccountAsync(Guid id, RequestBudget budget)
    {
        var result = await accounts.FindByIdAsync(id, budget);
        if (budget.Cause != BudgetCancellationCause.None || result.Status == UserReadStatus.Unavailable)
            return (OrderOutcome.Unavailable, null);
        return result.Status == UserReadStatus.Found && result.User is { Status: UserStatus.Active } user && user.Id == id
            ? (OrderOutcome.Ok, user) : (OrderOutcome.Forbidden, null);
    }

    public Task<OrderStoreResult<IReadOnlyList<Order>>> ListAsync(OrderListing listing, Guid viewer, OrderPosition? after, int take, RequestBudget budget) =>
        orders.ListAsync(listing, viewer, after, take, budget);

    public async Task<OrderResult> DetailAsync(Guid id, Guid viewer, RequestBudget budget)
    {
        var result = await orders.FindVisibleAsync(id, viewer, budget);
        return result.Status switch
        {
            OrderStoreStatus.Confirmed when result.Value is { } value => value.IsVisibleTo(viewer)
                ? new(OrderOutcome.Ok, value) : new(OrderOutcome.NotFound),
            OrderStoreStatus.Missing => new(OrderOutcome.NotFound), _ => new(OrderOutcome.Unavailable)
        };
    }

    public async Task<OrderResult> CreateAsync(User account, Guid key, OrderDraft draft, RequestBudget budget)
    {
        // V1 sin 008: no se acepta ningún medio que no se pueda verificar.
        if (draft.MediaId is not null) return new(OrderOutcome.Unavailable);

        // Solo campos de negocio normalizados; no incluir identidad, snapshot, fecha ni entrada de búsqueda UI.
        var canonical = JsonSerializer.SerializeToUtf8Bytes(new
        {
            version = 1, draft.Category, draft.Title, draft.SpecialInstructions, amount = OrderLabels.Cost(draft.Amount),
            draft.Location, draft.DeliveryTime, draft.MediaId
        });
        var fingerprint = Convert.ToHexString(SHA256.HashData(canonical));
        OrderResult Replay(OrderStoreResult<OrderCreation> found) => found is { Status: OrderStoreStatus.Confirmed, Value: { } previous }
            ? previous.Fingerprint == fingerprint ? new(OrderOutcome.Ok, previous.Order) : new(OrderOutcome.IdempotencyConflict)
            : new(OrderOutcome.Unavailable);

        var existing = await orders.FindCreationAsync(account.Id, key, budget);
        if (existing.Status != OrderStoreStatus.Missing) return Replay(existing);

        var profile = await profiles.ReadAsync(account.Id, budget);
        var name = profile.Status switch
        {
            ProfileStoreStatus.Missing => account.FullName,
            ProfileStoreStatus.Confirmed when profile.Profile is { } value => value.FullName ?? account.FullName,
            _ => null
        };
        if (name is null || budget.Cause != BudgetCancellationCause.None) return new(OrderOutcome.Unavailable);

        var now = DateTimeOffset.FromUnixTimeMilliseconds(clock.GetUtcNow().ToUnixTimeMilliseconds()).UtcDateTime;
        var order = new Order(Guid.NewGuid(), account.Id, null, name, draft, Order.V1Currency, OrderStatus.Published, now, now, null);
        var inserted = await orders.InsertAsync(new(order, key, fingerprint), budget);
        if (inserted is { Status: OrderStoreStatus.Confirmed, Value: { } confirmed }) return new(OrderOutcome.Created, confirmed);
        if (inserted.Status == OrderStoreStatus.Duplicate)
            return Replay(await orders.FindCreationAsync(account.Id, key, budget));
        // Un ACK incierto no prueba ausencia de efectos. Solo un retry del cliente con la misma clave reconcilia.
        return new(OrderOutcome.Unavailable);
    }

    public async Task<OrderResult> CancelAsync(Guid id, Guid owner, RequestBudget budget)
    {
        var changed = await orders.CancelPublishedAsync(id, owner, clock.GetUtcNow().UtcDateTime, budget);
        if (changed is { Status: OrderStoreStatus.Confirmed, Value: { } confirmed }) return new(OrderOutcome.Ok, confirmed);
        if (changed.Status != OrderStoreStatus.Missing) return new(OrderOutcome.Unavailable);
        var current = await orders.FindOwnedAsync(id, owner, budget);
        return current switch
        {
            { Status: OrderStoreStatus.Missing } => new(OrderOutcome.NotFound),
            { Status: OrderStoreStatus.Confirmed, Value: { Status: OrderStatus.Cancelled } value } => new(OrderOutcome.Ok, value),
            { Status: OrderStoreStatus.Confirmed, Value: { Status: not OrderStatus.Published } } => new(OrderOutcome.StateConflict),
            _ => new(OrderOutcome.Unavailable)
        };
    }
}
