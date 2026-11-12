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

public enum AuthenticationEventKind { Register, SecurityAdmission, Quota, Hashing, Persistence, Dependency, Logout, Login, Rehash }
public enum AuthenticationEventOutcome { Admitted, Rejected, Invalid, Confirmed, Conflict, Unavailable, Uncertain, Saturated, RateLimited, Success, RehashUpdated, RehashRaceLost }
public interface IAuthenticationEvents
{
    void Record(AuthenticationEventKind kind, AuthenticationEventOutcome outcome);
}
