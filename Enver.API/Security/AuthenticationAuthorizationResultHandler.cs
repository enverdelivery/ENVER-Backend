using Enver.API.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Enver.API.Security;

public sealed class AuthenticationAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult result)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (result.Succeeded) { await next(context); return; }
        if (context.Items.ContainsKey(RevocationAuthorizationHandler.OperationalFailureKey))
        { await ApiProblemDetails.TryWriteAsync(context, 503); return; }
        if (result.Challenged || result.AuthorizationFailure?.FailedRequirements.OfType<RevocationRequirement>().Any() == true
            || result.AuthorizationFailure?.FailCalled == true)
        { await ApiProblemDetails.TryWriteAsync(context, 401); return; }
        // Otros requisitos del recurso nunca se conceden por tener un JWT válido.
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
    }
}
