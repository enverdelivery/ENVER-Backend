using Enver.API.Features.Authentication.Application.Ports;

namespace Enver.API.Observability;

/// <summary>Allowlist cerrada: sin excepciones, payloads, identidades, claves ni labels del cliente.</summary>
public sealed class AuthenticationEvents(ILogger<AuthenticationEvents> logger) : IAuthenticationEvents
{
    public void Record(AuthenticationEventKind kind, AuthenticationEventOutcome outcome)
    {
        if (!Enum.IsDefined(kind) || !Enum.IsDefined(outcome)) throw new ArgumentOutOfRangeException(nameof(kind));
        logger.LogInformation(new EventId(2036, "Authentication"),
            "Authentication {Operation} {Kind} {Outcome}", "register", kind, outcome);
    }
}
