using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Ports;

public interface ILoginControls
{
    ValueTask<AuthenticationSecurityDecision> CheckAsync(RequestBudget budget);
    ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(string normalizedEmail, AuthenticationCanonicalOrigin origin, RequestBudget budget);
}
