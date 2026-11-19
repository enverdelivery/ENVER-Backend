using System.Text.Json;
using Enver.API.Features.Authentication.Application;
using Enver.API.Features.Authentication.Contracts;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enver.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthenticationController(AuthenticationService authentication, IAuthenticationOriginProvider origins) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task Login()
    {
        var boundary = HttpContext.Features.Get<AuthenticationBoundaryResult>();
        var budget = HttpContext.Features.Get<RequestBudget>();
        if (boundary?.Operation != AuthenticationOperation.Login || budget is null)
        { await ApiProblemDetails.TryWriteAsync(HttpContext, 503); return; }
        LoginInput input;
        if (!boundary.JsonReadable) input = new(null, null, readableBody: false);
        else
        {
            using var document = await JsonDocument.ParseAsync(Request.Body, cancellationToken: budget.Token);
            var root = document.RootElement;
            string? Text(string field) => root.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
            // No MVC body binding: secundarios inválidos conservan cuota; JSON ilegible nunca crea buckets.
            input = new(Text("email"), Text("password"), invalidFields: boundary.InvalidFields.Select(field => field switch
            {
                ApiErrorField.Email => Enver.API.Features.Authentication.Validation.AuthenticationValidationField.Email,
                ApiErrorField.Password => Enver.API.Features.Authentication.Validation.AuthenticationValidationField.Password,
                _ => Enver.API.Features.Authentication.Validation.AuthenticationValidationField.Body
            }).ToArray());
        }
        var origin = origins.Resolve(HttpContext).Origin;
        if (HttpContext.Features.Get<LoginAttemptEvents>() is { } attempt) attempt.HandedToApplication = true;
        var result = await authentication.LoginAsync(input, origin, budget);
        if (result.Status == LoginStatus.Issued)
        {
            Response.StatusCode = 200; Response.Headers.CacheControl = "no-store";
            await Response.WriteAsJsonAsync(new LoginResponse(result.AccessToken!), budget.Token); return;
        }
        var status = result.Status switch
        {
            LoginStatus.ValidationFailed => 400, LoginStatus.AuthenticationFailed => 401,
            LoginStatus.RateLimited => 429, _ => 503
        };
        await ApiProblemDetails.TryWriteAsync(HttpContext, status,
            result.InvalidFields.Select(AuthenticationValidationCoordinator.ToApi), result.RetryAfter);
    }

    [Authorize(Policy = AuthenticationPolicies.Logout)]
    [HttpPost("logout")]
    public async Task Logout()
    {
        var boundary = HttpContext.Features.Get<AuthenticationBoundaryResult>();
        var budget = HttpContext.Features.Get<RequestBudget>();
        if (boundary?.Operation != AuthenticationOperation.Logout || budget is null)
        { await ApiProblemDetails.TryWriteAsync(HttpContext, 503); return; }
        if (!LogoutInput.TryFromAuthenticatedContext(HttpContext, out var input))
        { await ApiProblemDetails.TryWriteAsync(HttpContext, 401); return; }
        var result = await authentication.LogoutAsync(input!, origins.Resolve(HttpContext).Origin,
            boundary.ErrorStatus is null, budget);
        var status = result.Status switch
        {
            LogoutStatus.Confirmed => 204, LogoutStatus.ValidationFailed => 400,
            LogoutStatus.RateLimited => 429, _ => 503
        };
        await ApiProblemDetails.TryWriteAsync(HttpContext, status,
            status == 400 ? new[] { ApiErrorField.Body } : null, result.RetryAfter);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task Register()
    {
        var boundary = HttpContext.Features.Get<AuthenticationBoundaryResult>();
        var budget = HttpContext.Features.Get<RequestBudget>();
        if (boundary is null || budget is null) { await ApiProblemDetails.TryWriteAsync(HttpContext, 503); return; }
        RegisterInput input;
        if (!boundary.JsonReadable || boundary.InvalidFields.Contains(ApiErrorField.Body)) input = new(null, null, null, readableBody: false);
        else
        {
            // La frontera ya acreditó UTF-8, tamaño, profundidad, duplicados y nombres exactos.
            using var document = await JsonDocument.ParseAsync(Request.Body, cancellationToken: budget.Token);
            var root = document.RootElement;
            if (boundary.ErrorStatus is null)
            {
                var options = new JsonSerializerOptions();
                AuthenticationRequestLimitsMiddleware.ConfigureSerializer(options);
                var request = root.Deserialize<RegisterRequest>(options)!;
                input = new(request.FullName, request.Email, request.Password);
            }
            else
            {
                // Sin coerción: campos secundarios inválidos no evitan el cargo con email válido.
                string? Text(string field) => root.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
                input = new(Text("fullName"), Text("email"), Text("password"));
            }
        }
        var result = await authentication.RegisterAsync(input, origins.Resolve(HttpContext).Origin, budget);
        if (result.Status == RegistrationStatus.Confirmed)
        {
            var identity = result.Identity!;
            Response.StatusCode = 201;
            Response.Headers.CacheControl = "no-store";
            await Response.WriteAsJsonAsync(new RegisterResponse(identity.Id, identity.FullName, identity.Email), budget.Token);
            return;
        }
        var status = result.Status switch
        {
            RegistrationStatus.ValidationFailed => 400, RegistrationStatus.Conflict => 409,
            RegistrationStatus.RateLimited => 429, _ => 503
        };
        await ApiProblemDetails.TryWriteAsync(HttpContext, status,
            result.InvalidFields.Select(AuthenticationValidationCoordinator.ToApi), result.RetryAfter);
    }
}
