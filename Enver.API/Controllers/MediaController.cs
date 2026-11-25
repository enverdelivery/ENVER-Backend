using System.Text;
using System.Text.Json;
using Enver.API.Features.Media;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Enver.API.Controllers;

[ApiController]
[Authorize(Policy = AuthenticationPolicies.Normal)]
[Route("api/v1/media")]
public sealed class MediaController(MediaPresignService media) : ControllerBase
{
    private const int MaxBodyBytes = 4096;
    private static readonly UTF8Encoding Utf8 = new(false, true);

    [HttpPost("presign")]
    public async Task Presign()
    {
        if (HttpContext.Features.Get<ValidatedJwtIdentity>() is not { } identity || !Guid.TryParseExact(identity.Subject, "D", out var owner))
        { await ApiProblemDetails.TryWriteAsync(HttpContext, 401); return; }
        using var budget = new RequestBudget(3000, HttpContext.RequestAborted);
        try
        {
            var (error, mime) = await ReadAsync(budget.Token);
            if (mime is null) { await ApiProblemDetails.TryWriteAsync(HttpContext, error); return; }
            var (status, body, retry) = await media.IssueAsync(owner, mime, budget);
            if (status == 403)
            {
                Response.StatusCode = 403;
                var problem = new ProblemDetails { Type = "about:blank", Status = 403, Title = "Forbidden", Detail = "No se permite acceder a este recurso." };
                problem.Extensions["code"] = "forbidden";
                problem.Extensions["traceId"] = RequestContextMiddleware.GetOrCreateTraceId(HttpContext);
                await Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: budget.Token);
            }
            else if (body is null) await ApiProblemDetails.TryWriteAsync(HttpContext, status, retryAfter: retry);
            else
            {
                Response.StatusCode = 201;
                await Response.WriteAsJsonAsync(body, budget.Token);
            }
        }
        catch (OperationCanceledException) when (budget.Cause != BudgetCancellationCause.None)
        { await ApiProblemDetails.TryWriteAsync(HttpContext, 503); }
    }

    private async Task<(int Status, string? Mime)> ReadAsync(CancellationToken token)
    {
        if (Request.Query.Count != 0) return (400, null);
        if (Request.ContentLength > MaxBodyBytes) return (413, null);
        var encodings = Request.Headers.ContentEncoding;
        if (!MediaTypeHeaderValue.TryParse(Request.ContentType, out var type) ||
            !type.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
            (type.Parameters.Count != 0 && (type.Parameters.Count != 1 ||
                !HeaderUtilities.RemoveQuotes(type.Charset).Equals("utf-8", StringComparison.OrdinalIgnoreCase))) ||
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
            using var doc = JsonDocument.Parse(bytes.AsMemory(0, length), new JsonDocumentOptions { MaxDepth = 3, AllowDuplicateProperties = false });
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return (400, null);
            string? name = null, mime = null;
            foreach (var field in doc.RootElement.EnumerateObject())
            {
                if (field.Value.ValueKind != JsonValueKind.String) return (400, null);
                switch (field.Name)
                {
                    case "fileName": name = field.Value.GetString(); break;
                    case "contentType": mime = field.Value.GetString(); break;
                    default: return (400, null);
                }
            }
            if (string.IsNullOrWhiteSpace(name) || name.EnumerateRunes().Count() > 128 || name is "." or ".." ||
                name.Any(c => char.IsControl(c) || c is '/' or '\\' or ':') || mime is null) return (400, null);
            return (0, mime);
        }
        catch (Exception ex) when (ex is JsonException or DecoderFallbackException or InvalidOperationException) { return (400, null); }
    }
}
