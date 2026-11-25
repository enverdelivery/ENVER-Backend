using System.Text;
using System.Text.Json;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Features.Users.Application;
using Enver.API.Features.Users.Contracts;
using Enver.API.Features.Users.Core;
using Enver.API.Features.Users.Validation;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Enver.API.Controllers;

[ApiController]
[Authorize(Policy = AuthenticationPolicies.Normal)]
[Route("api/v1/users/me")]
public sealed class UsersController(UserProfileService profiles, AuthenticationInputValidator names) : ControllerBase
{
    private const int MaxBodyBytes = 4096;
    private static readonly UTF8Encoding Utf8 = new(false, true);

    [HttpGet]
    public Task Get() => ExecuteAsync();

    [HttpPatch]
    public Task Patch() => ExecuteAsync(write: true);

    [HttpPut("active-role")]
    public Task SelectRole() => ExecuteAsync(write: true, selectMode: true);

    [HttpGet("rescuer-status")]
    public Task GetRescuerStatus() => ExecuteAsync(eligibilityOnly: true);

    private async Task ExecuteAsync(bool write = false, bool selectMode = false, bool eligibilityOnly = false)
    {
        if (HttpContext.Features.Get<ValidatedJwtIdentity>() is not { } identity ||
            !Guid.TryParseExact(identity.Subject, "D", out var id))
        { await ApiProblemDetails.TryWriteAsync(HttpContext, 401); return; }

        using var budget = new RequestBudget(2000, HttpContext.RequestAborted);
        try
        {
            var (error, change) = await ReadRequestAsync(write, selectMode, budget.Token);
            if (error != 0)
            {
                await ApiProblemDetails.TryWriteAsync(HttpContext, error, error == 400 ? [ApiErrorField.Body] : null);
                return;
            }

            var result = await profiles.ExecuteAsync(id, budget, change, eligibilityOnly);
            if (result.Status == OwnProfileStatus.Forbidden)
            {
                Response.StatusCode = 403;
                var problem = new ProblemDetails { Type = "about:blank", Status = 403, Title = "Forbidden",
                    Detail = "No se permite acceder a este recurso." };
                problem.Extensions["code"] = "forbidden";
                problem.Extensions["traceId"] = RequestContextMiddleware.GetOrCreateTraceId(HttpContext);
                await Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: budget.Token);
            }
            else if (result.Profile is { } profile)
                await Response.WriteAsJsonAsync(ProfileResponse.From(profile), budget.Token);
            else if (result.Eligibility is { } status)
                await Response.WriteAsJsonAsync(new RescuerStatusResponse(status.Enabled, status.Level), budget.Token);
            else await ApiProblemDetails.TryWriteAsync(HttpContext, 503);
        }
        catch (OperationCanceledException) when (budget.Cause != BudgetCancellationCause.None)
        {
            if (!HttpContext.RequestAborted.IsCancellationRequested) await ApiProblemDetails.TryWriteAsync(HttpContext, 503);
        }
    }

    private async Task<(int Error, ProfileChange? Change)> ReadRequestAsync(bool write, bool selectMode, CancellationToken token)
    {
        if (!write)
            return (await Request.Body.ReadAsync(new byte[1].AsMemory(), token) == 0 ? 0 : 400, null);
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
            using var json = JsonDocument.Parse(bytes.AsMemory(0, length), new JsonDocumentOptions
            { MaxDepth = 4, AllowDuplicateProperties = false });
            var change = ProfileInput.Parse(json.RootElement, selectMode, names);
            return change is null ? (400, null) : (0, change);
        }
        catch (Exception ex) when (ex is JsonException or DecoderFallbackException or InvalidOperationException)
        { return (400, null); }
    }
}
