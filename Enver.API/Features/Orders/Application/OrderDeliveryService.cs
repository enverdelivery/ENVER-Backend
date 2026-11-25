using Enver.API.Features.Orders.Core;
using Enver.API.Features.Users.Application;
using Enver.API.Http;

namespace Enver.API.Features.Orders.Application;

public sealed class OrderDeliveryService(IOrderStore orders, IRescuerEligibility eligibility, TimeProvider clock)
{
    public async Task<OrderResult> AdvanceAsync(Guid id, Guid actor, OrderDeliveryAction action, RequestBudget budget)
    {
        if (action == OrderDeliveryAction.Accept)
        {
            var permission = await eligibility.ReadAsync(actor, budget);
            if (permission is null || budget.Cause != BudgetCancellationCause.None)
                return new(OrderOutcome.Unavailable);
            if (!permission.Enabled) return new(OrderOutcome.Forbidden);
        }

        var changed = await orders.AdvanceDeliveryAsync(id, actor, action, clock.GetUtcNow().UtcDateTime, budget);
        if (changed is { Status: OrderStoreStatus.Confirmed, Value: { } confirmed })
            return new(OrderOutcome.Ok, confirmed);

        if (budget.Cause != BudgetCancellationCause.None) return new(OrderOutcome.Unavailable);
        // Un solo intento de escritura. Una lectura confirmada puede acreditar el paso, no la ausencia de una escritura incierta.
        var uncertain = changed.Status != OrderStoreStatus.Missing;
        var read = await orders.FindVisibleAsync(id, actor, budget);
        if (budget.Cause != BudgetCancellationCause.None) return new(OrderOutcome.Unavailable);
        if (read.Status == OrderStoreStatus.Missing)
            return new(uncertain ? OrderOutcome.Unavailable : OrderOutcome.NotFound);
        if (read is not { Status: OrderStoreStatus.Confirmed, Value: { } current })
            return new(OrderOutcome.Unavailable);

        var accepting = action == OrderDeliveryAction.Accept;
        if (!(accepting ? current.IsVisibleTo(actor) : current.IsParticipant(actor)))
            return new(uncertain ? OrderOutcome.Unavailable : OrderOutcome.NotFound);

        var authorized = action switch
        {
            OrderDeliveryAction.Accept => current.OwnerId != actor,
            OrderDeliveryAction.ConfirmCompletion => current.OwnerId == actor && current.RescuerId != actor,
            _ => current.RescuerId == actor
        };
        if (!authorized) return new(uncertain ? OrderOutcome.Unavailable : OrderOutcome.Forbidden);

        var (previous, next) = OrderDeliveryTransitions.For(action);
        if (current.Delivery?.For(action)?.ActorId == actor &&
            current.Status >= next && current.Status <= OrderStatus.Completed &&
            (accepting ? current.RescuerId == actor : current.RescuerId is not null))
            return new(OrderOutcome.Ok, current);

        // Un snapshot aún en el estado esperado puede preceder a la confirmación de una escritura concurrente.
        return new(uncertain || current.Status == previous ? OrderOutcome.Unavailable : OrderOutcome.StateConflict);
    }
}
