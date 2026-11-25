using System.Text;
using System.Text.Json;
using Enver.API.Features.Orders.Contracts;
using Enver.API.Features.Ratings;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Enver.API.Controllers;

[ApiController]
[Authorize(Policy = AuthenticationPolicies.Normal)]
[Route("api/v1/orders/{id}/rating")]
public sealed class RatingsController(RatingService ratings) : ControllerBase
{
    private const int MaxBodyBytes = 8 * 1024;
    private static readonly UTF8Encoding Utf8 = new(false, true);

    [HttpPost]
    public async Task Submit([FromRoute] string id)
    {
        if (HttpContext.Features.Get<ValidatedJwtIdentity>() is not { } identity || !OrderInput.TryId(identity.Subject, out var author))
        { await ApiProblemDetails.TryWriteAsync(HttpContext, 401); return; }
        if (!OrderInput.TryId(id, out var orderId) || Request.Query.Count != 0)
        { await ApiProblemDetails.TryWriteAsync(HttpContext, 400); return; }

        using var budget = new RequestBudget(3000, HttpContext.RequestAborted);
        try
        {
            var (status, input) = await ReadAsync(budget.Token);
            if (input is null) { await ApiProblemDetails.TryWriteAsync(HttpContext, status); return; }
            var (outcome, rating) = await ratings.SubmitAsync(orderId, author, input, budget);
            if (rating is null) { await WriteErrorAsync(outcome); return; }
            budget.ThrowIfCancellationRequested();
            Response.Headers.CacheControl = "no-store";
            Response.StatusCode = outcome == RatingOutcome.Created ? 201 : 200;
            await Response.WriteAsJsonAsync(new { rating.Id, rating.OrderId, rating.Score, rating.Comment, rating.CreatedAt }, budget.Token);
        }
        catch (OperationCanceledException) when (budget.Cause != BudgetCancellationCause.None)
        { await ApiProblemDetails.TryWriteAsync(HttpContext, 503); }
    }

    private async Task<(int Status, RatingInput? Input)> ReadAsync(CancellationToken token)
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
        var length = await Request.Body.ReadAtLeastAsync(bytes, bytes.Length, false, token);
        if (length > MaxBodyBytes) return (413, null);
        try
        {
            _ = Utf8.GetCharCount(bytes.AsSpan(0, length));
            using var json = JsonDocument.Parse(bytes.AsMemory(0, length), new JsonDocumentOptions { MaxDepth = 3, AllowDuplicateProperties = false });
            return (400, RatingInput.Parse(json.RootElement));
        }
        catch (Exception ex) when (ex is JsonException or DecoderFallbackException or InvalidOperationException)
        { return (400, null); }
    }

    private async Task WriteErrorAsync(RatingOutcome outcome)
    {
        if (outcome == RatingOutcome.Unavailable) { await ApiProblemDetails.TryWriteAsync(HttpContext, 503); return; }
        if (Response.HasStarted || HttpContext.RequestAborted.IsCancellationRequested) return;
        var (status, title, code, detail) = outcome switch
        {
            RatingOutcome.Forbidden => (403, "Forbidden", "forbidden", "No se permite acceder a este recurso."),
            RatingOutcome.NotFound => (404, "Not Found", "not_found", "No se encontró este recurso."),
            RatingOutcome.StateConflict => (409, "Conflict", "order_state_conflict", "El estado actual no permite calificar el pedido."),
            RatingOutcome.AlreadyExists => (409, "Conflict", "rating_already_exists", "Ya existe una calificación con otros datos."),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome))
        };
        Response.Headers.CacheControl = "no-store";
        Response.StatusCode = status;
        var problem = new ProblemDetails { Type = "about:blank", Title = title, Status = status, Detail = detail };
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = RequestContextMiddleware.GetOrCreateTraceId(HttpContext);
        await Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: HttpContext.RequestAborted);
    }
}
