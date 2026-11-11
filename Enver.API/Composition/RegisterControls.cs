using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;

namespace Enver.API.Composition;

internal sealed class RegisterAdmission(AuthenticationDependencyAdmission admission) : IRegisterAdmission
{
    public ValueTask<AuthenticationSecurityDecision> CheckAsync(RequestBudget budget) =>
        admission.EvaluateOperationAsync(AuthenticationOperation.Register, budget);
}

internal sealed class RegisterQuota(IAuthenticationQuotaStore store, IAuthenticationScopePseudonymizer pseudonyms,
    RedisAuthenticationQuotaConfiguration configuration) : IRegisterQuota
{
    public ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(string normalizedEmail, AuthenticationCanonicalOrigin origin, RequestBudget budget)
    {
        var operation = AuthenticationOperation.Register;
        var identities = new[]
        {
            AuthenticationScopeIdentity.Email(operation, AuthenticationCanonicalEmail.FromValidatedEmail(normalizedEmail)),
            AuthenticationScopeIdentity.Origin(operation, origin)
        };
        var request = new AuthenticationQuotaConsumeRequest(operation, Guid.NewGuid().ToString("N"), configuration.Snapshot.Policy.Revision,
            identities.Select(identity => new AuthenticationQuotaScope(identity.ScopeType, pseudonyms.Pseudonymize(identity),
                configuration.Snapshot.Policy.GetDefinition(operation, identity.ScopeType))));
        return store.EvaluateAndConsumeAsync(request, budget);
    }
}
