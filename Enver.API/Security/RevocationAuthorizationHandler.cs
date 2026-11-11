using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Enver.API.Security;

public sealed class RevocationAuthorizationHandler(
    IAuthenticationSecurityState securityState, ITokenRevocationStore revocations, IOptions<AuthenticationOptions> options) : IAuthorizationHandler
{
    internal static readonly object OperationalFailureKey = new();
    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var requirements = context.Requirements.OfType<RevocationRequirement>().ToArray();
        if (requirements.Length == 0) return;
        if (context.Resource is not HttpContext http || context.User.Identity?.IsAuthenticated != true
            || http.Features.Get<ValidatedJwtIdentity>() is not { } jwt || !ReferenceEquals(jwt.Principal, context.User))
        { context.Fail(); return; }
        var logout = requirements.All(r => r.AllowRevokedForLogout);
        if (requirements.Any(r => r.AllowRevokedForLogout) &&
            (!logout || !HttpMethods.IsPost(http.Request.Method) || http.Request.Path != "/api/v1/auth/logout"
            || http.GetEndpoint()?.Metadata.GetOrderedMetadata<IAuthorizeData>().Any(a => a.Policy == AuthenticationPolicies.Logout) != true))
        { context.Fail(); return; }
        using var ownedBudget = http.Features.Get<RequestBudget>() is null
            ? RequestBudget.Create(AuthenticationOperation.Logout, options.Value, http.RequestAborted) : null;
        var budget = http.Features.Get<RequestBudget>() ?? ownedBudget!;
        var admission = await securityState.EvaluateAsync(budget).ConfigureAwait(false);
        if (!admission.CanContinue) { OperationalFailure(http, context); return; }
        var result = await revocations.CheckAsync(new("enver-api", jwt.Jti), budget).ConfigureAwait(false);
        if (result.Status is TokenRevocationReadStatus.Unavailable or TokenRevocationReadStatus.Uncertain)
        { OperationalFailure(http, context); return; }
        if (result.Status == TokenRevocationReadStatus.NotRevoked || (logout && result.Status == TokenRevocationReadStatus.Revoked))
            foreach (var requirement in requirements) context.Succeed(requirement);
        else context.Fail();
    }
    private static void OperationalFailure(HttpContext http, AuthorizationHandlerContext context)
    { http.Items[OperationalFailureKey] = true; context.Fail(); }
}
