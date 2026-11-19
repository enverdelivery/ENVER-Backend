using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Security;
using Enver.API.Infrastructure.Redis;
using Enver.API.Infrastructure.MongoDB;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Security;

namespace Enver.API.Composition;

internal sealed class LoginControls(AuthenticationDependencyAdmission admission, DummyPasswordHash dummy,
    IAuthenticationQuotaStore store, IAuthenticationScopePseudonymizer pseudonyms,
    RedisAuthenticationQuotaConfiguration configuration) : ILoginControls
{
    public async ValueTask<AuthenticationSecurityDecision> CheckAsync(RequestBudget budget)
    {
        if (!dummy.IsInitialized || await admission.RefreshIndexAsync(budget).ConfigureAwait(false) != MongoUsersInitializationStatus.Ready)
            return new(AuthenticationSecurityAdmission.Unavailable);
        return await admission.EvaluateOperationAsync(AuthenticationOperation.Login, budget).ConfigureAwait(false);
    }

    public ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(string normalizedEmail, AuthenticationCanonicalOrigin origin, RequestBudget budget)
    {
        var operation = AuthenticationOperation.Login;
        var email = AuthenticationCanonicalEmail.FromValidatedEmail(normalizedEmail);
        var identities = new[] { AuthenticationScopeIdentity.OriginEmail(origin, email),
            AuthenticationScopeIdentity.Email(operation, email), AuthenticationScopeIdentity.Origin(operation, origin) };
        return store.EvaluateAndConsumeAsync(new(operation, Guid.NewGuid().ToString("N"), configuration.Snapshot.Policy.Revision,
            identities.Select(identity => new AuthenticationQuotaScope(identity.ScopeType, pseudonyms.Pseudonymize(identity),
                configuration.Snapshot.Policy.GetDefinition(operation, identity.ScopeType)))), budget);
    }
}

/// <summary>Reutiliza la autoridad y el emisor existentes; fallar nunca compensa el horizonte potencial.</summary>
public sealed class LoginTokenIssuer(IAuthenticationSecurityState security, AuthenticationSecurityCoordinator coordinator,
    IAccessTokenIssuer issuer) : ILoginTokenIssuer
{
    public async ValueTask<AccessTokenIssueResult?> IssueAsync(Guid subject, RequestBudget budget)
    {
        if (budget.Cause != BudgetCancellationCause.None || !(await security.EvaluateAsync(budget).ConfigureAwait(false)).CanContinue ||
            budget.Cause != BudgetCancellationCause.None) return null;
        var lease = await coordinator.BeginEmissionAsync(budget).ConfigureAwait(false);
        if (lease is null) return null;
        var completed = false;
        try
        {
            if (budget.Cause != BudgetCancellationCause.None) return null;
            var token = issuer.Issue(subject);
            if (token.Status != AccessTokenIssueStatus.Issued || token.AccessToken is null || budget.Cause != BudgetCancellationCause.None ||
                !(await security.EvaluateAsync(budget).ConfigureAwait(false)).CanContinue || budget.Cause != BudgetCancellationCause.None)
                return null;
            coordinator.CompleteEmission(lease); completed = true;
            return token;
        }
        finally { if (!completed) coordinator.FailEmission(lease); }
    }
}
