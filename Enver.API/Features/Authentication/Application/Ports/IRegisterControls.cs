using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Ports;

public interface IRegisterAdmission
{
    ValueTask<AuthenticationSecurityDecision> CheckAsync(RequestBudget budget);
}

public interface IRegisterQuota
{
    ValueTask<AuthenticationQuotaConsumeResult> ConsumeAsync(string normalizedEmail, AuthenticationCanonicalOrigin origin, RequestBudget budget);
}

public enum AuthenticationEventKind { Register, SecurityAdmission, Quota, Hashing, Persistence, Dependency }
public enum AuthenticationEventOutcome { Admitted, Rejected, Invalid, Confirmed, Conflict, Unavailable, Uncertain, Saturated }
public interface IAuthenticationEvents
{
    void Record(AuthenticationEventKind kind, AuthenticationEventOutcome outcome);
}
