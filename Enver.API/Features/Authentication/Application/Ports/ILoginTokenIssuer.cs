using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Ports;

/// <summary>Orquesta una sola emisión protegida; null significa que no se puede devolver ningún token.</summary>
public interface ILoginTokenIssuer
{
    ValueTask<AccessTokenIssueResult?> IssueAsync(Guid subject, RequestBudget budget);
}
