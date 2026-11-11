using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Ports;

/// <summary>Admisión/cuota de Logout; no es otro almacén de revocaciones ni emite tokens.</summary>
public interface ILogoutControls
{
    TimeSpan EffectiveClockSkew { get; }
    TimeSpan OperationalMargin { get; }
    ValueTask<AuthenticationSecurityDecision> CheckAsync(RequestBudget budget);
    ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(AuthenticationAuthenticatedAccount account,
        AuthenticationCanonicalOrigin origin, RequestBudget budget);
}
