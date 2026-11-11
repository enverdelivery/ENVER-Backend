using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Microsoft.Extensions.Options;
using Enver.API.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Enver.API.Composition;

internal sealed class LogoutControls(AuthenticationDependencyAdmission admission, IAuthenticationQuotaStore store,
    IAuthenticationScopePseudonymizer pseudonyms, RedisAuthenticationQuotaConfiguration configuration,
    IOptions<JwtOptions> jwt, IOptions<RedisOptions> redis) : ILogoutControls
{
    public TimeSpan EffectiveClockSkew => TimeSpan.FromSeconds(jwt.Value.ClockSkewSeconds
        ?? throw new InvalidOperationException("Política temporal no disponible."));
    public TimeSpan OperationalMargin => TimeSpan.FromSeconds(redis.Value.RevocationOperationalMarginSeconds);

    public ValueTask<AuthenticationSecurityDecision> CheckAsync(RequestBudget budget) =>
        admission.EvaluateOperationAsync(AuthenticationOperation.Logout, budget);

    public ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(AuthenticationAuthenticatedAccount account,
        AuthenticationCanonicalOrigin origin, RequestBudget budget)
    {
        var operation = AuthenticationOperation.Logout;
        var identities = new[] { AuthenticationScopeIdentity.Account(account), AuthenticationScopeIdentity.Origin(operation, origin) };
        var request = new AuthenticationQuotaConsumeRequest(operation, Guid.NewGuid().ToString("N"), configuration.Snapshot.Policy.Revision,
            identities.Select(identity => new AuthenticationQuotaScope(identity.ScopeType, pseudonyms.Pseudonymize(identity),
                configuration.Snapshot.Policy.GetDefinition(operation, identity.ScopeType))));
        return store.EvaluateAndConsumeAsync(request, budget);
    }
}

internal sealed class LogoutEndpointConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var action in application.Controllers.Where(c => c.ControllerType.AsType() == typeof(AuthenticationController))
                     .SelectMany(c => c.Actions).Where(a => a.ActionName == nameof(AuthenticationController.Logout)))
            foreach (var selector in action.Selectors)
                selector.EndpointMetadata.Add(new AuthenticationRequestMetadata(AuthenticationOperation.Logout));
    }
}
