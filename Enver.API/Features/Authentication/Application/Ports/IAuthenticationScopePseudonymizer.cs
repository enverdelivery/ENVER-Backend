using Enver.API.Features.Authentication.Application.Quotas;

namespace Enver.API.Features.Authentication.Application.Ports;

/// <summary>Derivación local síncrona, sin presupuesto de dependencia, E/S, retry o fallback.</summary>
public interface IAuthenticationScopePseudonymizer
{
    string Pseudonymize(AuthenticationScopeIdentity identity);
}
