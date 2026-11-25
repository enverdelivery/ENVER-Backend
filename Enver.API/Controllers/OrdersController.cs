using System.Globalization;
using System.Text;
using System.Text.Json;
using Enver.API.Features.Orders.Application;
using Enver.API.Features.Orders.Contracts;
using Enver.API.Features.Orders.Core;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Enver.API.Controllers;

[ApiController]
[Authorize(Policy = AuthenticationPolicies.Normal)]
[Route("api/v1/orders")]
public sealed class OrdersController(OrderService orders, OrderCursor cursors, OrderDeliveryService delivery) : ControllerBase
{
    private const int MaxBodyBytes = 24 * 1024;
    private static readonly UTF8Encoding Utf8 = new(false, true);

    [HttpGet] public Task Feed() => ExecuteAsync(Operation.Feed);
    [HttpGet("mine")] public Task Mine() => ExecuteAsync(Operation.Mine);
    [HttpGet("{id}")] public Task Detail([FromRoute] string id) => ExecuteAsync(Operation.Detail, id);
    [HttpPost] public Task Create() => ExecuteAsync(Operation.Create);
    [HttpPost("{id}/cancel")] public Task Cancel([FromRoute] string id) => ExecuteAsync(Operation.Cancel, id);
    [HttpGet("assigned-to-me")] public Task AssignedToMe() => ExecuteAsync(Operation.AssignedToMe);
    [HttpPost("{id}/accept")] public Task Accept([FromRoute] string id) => ExecuteAsync(Operation.Delivery, id, OrderDeliveryAction.Accept);
    [HttpPost("{id}/start-delivery")] public Task StartDelivery([FromRoute] string id) => ExecuteAsync(Operation.Delivery, id, OrderDeliveryAction.Start);
    [HttpPost("{id}/arrive")] public Task Arrive([FromRoute] string id) => ExecuteAsync(Operation.Delivery, id, OrderDeliveryAction.Arrive);
    [HttpPost("{id}/request-completion")] public Task RequestCompletion([FromRoute] string id) => ExecuteAsync(Operation.Delivery, id, OrderDeliveryAction.RequestCompletion);
    [HttpPost("{id}/confirm-completion")] public Task ConfirmCompletion([FromRoute] string id) => ExecuteAsync(Operation.Delivery, id, OrderDeliveryAction.ConfirmCompletion);

    private async Task ExecuteAsync(Operation operation, string? routeId = null, OrderDeliveryAction deliveryAction = default)
    {
        if (HttpContext.Features.Get<ValidatedJwtIdentity>() is not { } identity || !OrderInput.TryId(identity.Subject, out var viewer))
        { await ApiProblemDetails.TryWriteAsync(HttpContext, 401); return; }
        using var budget = new RequestBudget(3000, HttpContext.RequestAborted);
        try
        {
            var (outcome, account) = await orders.AuthenticateAccountAsync(viewer, budget);
            if (account is null) { await WriteErrorAsync(outcome); return; }
            OrderListing? listing = operation switch
            {
                Operation.Feed => OrderListing.Feed,
                Operation.Mine => OrderListing.Mine,
                Operation.AssignedToMe => OrderListing.AssignedToMe,
                _ => null
            };
            if (listing is { } list)
            {
                if (await HasBodyAsync(budget.Token)) { await InvalidAsync(); return; }
                await ListAsync(list, viewer, budget);
                return;
            }
            if (Request.Query.Count != 0) { await InvalidAsync(); return; }

            OrderResult result;
            if (operation == Operation.Create)
            {
                var key = Request.Headers["Idempotency-Key"];
                if (key.Count != 1 || !OrderInput.TryId(key[0], out var createKey)) { await InvalidAsync(); return; }
                var (error, draft) = await ReadDraftAsync(budget.Token);
                if (draft is null) { await ApiProblemDetails.TryWriteAsync(HttpContext, error); return; }
                result = await orders.CreateAsync(account, createKey, draft, budget);
            }
            else
            {
                if (!OrderInput.TryId(routeId, out var id) || await HasBodyAsync(budget.Token)) { await InvalidAsync(); return; }
                result = operation switch
                {
                    Operation.Cancel => await orders.CancelAsync(id, viewer, budget),
                    Operation.Delivery => await delivery.AdvanceAsync(id, viewer, deliveryAction, budget),
                    _ => await orders.DetailAsync(id, viewer, budget)
                };
            }

            if (result.Order is not { } order) { await WriteErrorAsync(result.Outcome, operation); return; }
            budget.ThrowIfCancellationRequested();
            if (result.Outcome == OrderOutcome.Created)
            {
                Response.StatusCode = StatusCodes.Status201Created;
                Response.Headers.Location = $"{Request.PathBase}/api/v1/orders/{order.Id:D}";
            }
            await Response.WriteAsJsonAsync(OrderDetail.From(order, viewer), budget.Token);
        }
        catch (OperationCanceledException) when (budget.Cause != BudgetCancellationCause.None)
        { await ApiProblemDetails.TryWriteAsync(HttpContext, 503); }
    }

    private async Task ListAsync(OrderListing listing, Guid viewer, RequestBudget budget)
    {
        var limit = 20;
        OrderPosition? after = null;
        foreach (var entry in Request.Query)
        {
            if (entry.Value.Count != 1 || entry.Value[0] is not { } value) { await InvalidAsync(); return; }
            switch (entry.Key)
            {
                case "limit" when value.Length <= 2 && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out limit)
                    && limit is >= 1 and <= 50: break;
                case "cursor" when cursors.TryDecode(value, listing, viewer, out after): break;
                default: await InvalidAsync(); return;
            }
        }
        var result = await orders.ListAsync(listing, viewer, after, limit + 1, budget);
        if (result is not { Status: OrderStoreStatus.Confirmed, Value: { } data }) { await WriteErrorAsync(OrderOutcome.Unavailable); return; }
        var next = data.Count > limit ? cursors.Encode(listing, viewer, new(data[limit - 1].CreatedAt, data[limit - 1].Id)) : null;
        if (listing == OrderListing.Feed)
            await Response.WriteAsJsonAsync(new OrderPage<OrderFeedItem>(data.Take(limit).Select(OrderFeedItem.From).ToArray(), next), budget.Token);
        else await Response.WriteAsJsonAsync(new OrderPage<OrderSummary>(data.Take(limit).Select(OrderSummary.From).ToArray(), next), budget.Token);
    }

    private async ValueTask<bool> HasBodyAsync(CancellationToken token) =>
        Request.ContentLength is > 0 || await Request.Body.ReadAsync(new byte[1].AsMemory(), token) != 0;

    private async Task<(int Error, OrderDraft? Draft)> ReadDraftAsync(CancellationToken token)
    {
        if (Request.ContentLength > MaxBodyBytes) return (413, null);
        var encodings = Request.Headers.ContentEncoding;
        if (!MediaTypeHeaderValue.TryParse(Request.ContentType, out var media) ||
            !media.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
            (media.Parameters.Count != 0 && (media.Parameters.Count != 1 ||
                !HeaderUtilities.RemoveQuotes(media.Charset).Equals("utf-8", StringComparison.OrdinalIgnoreCase))) ||
            (encodings.Count != 0 && (encodings.Count != 1 || !string.Equals(encodings[0], "identity", StringComparison.OrdinalIgnoreCase))))
            return (415, null);

        var bytes = new byte[MaxBodyBytes + 1];
        var length = 0;
        while (length < bytes.Length)
        {
            var read = await Request.Body.ReadAsync(bytes.AsMemory(length), token);
            if (read == 0) break;
            length += read;
        }
        if (length > MaxBodyBytes) return (413, null);
        try
        {
            _ = Utf8.GetCharCount(bytes.AsSpan(0, length));
            using var json = JsonDocument.Parse(bytes.AsMemory(0, length), new JsonDocumentOptions { MaxDepth = 4, AllowDuplicateProperties = false });
            return (400, OrderInput.Parse(json.RootElement));
        }
        catch (Exception ex) when (ex is JsonException or DecoderFallbackException or InvalidOperationException)
        { return (400, null); }
    }

    private ValueTask<bool> InvalidAsync() => ApiProblemDetails.TryWriteAsync(HttpContext, 400);

    private async ValueTask WriteErrorAsync(OrderOutcome outcome, Operation? operation = null)
    {
        if (outcome == OrderOutcome.Unavailable) { await ApiProblemDetails.TryWriteAsync(HttpContext, 503); return; }
        var (status, title, code, detail) = outcome switch
        {
            OrderOutcome.Forbidden => (403, "Forbidden", "forbidden", "No se permite acceder a este recurso."),
            OrderOutcome.NotFound => (404, "Not Found", "not_found", "No se encontró este recurso."),
            OrderOutcome.StateConflict => (409, "Conflict", "order_state_conflict", operation == Operation.Cancel
                ? "El estado actual no permite cancelar el pedido." : "El estado actual no permite esta transición."),
            OrderOutcome.IdempotencyConflict => (409, "Conflict", "idempotency_conflict", "La clave ya se usó con otros datos."),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome))
        };
        if (Response.HasStarted || HttpContext.RequestAborted.IsCancellationRequested) return;
        Response.StatusCode = status;
        var problem = new ProblemDetails { Type = "about:blank", Status = status, Title = title, Detail = detail };
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = RequestContextMiddleware.GetOrCreateTraceId(HttpContext);
        await Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: HttpContext.RequestAborted);
    }

    private enum Operation { Feed, Mine, Detail, Create, Cancel, AssignedToMe, Delivery }
}
