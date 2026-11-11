using Enver.API.Features.Authentication.Application.Ports;

namespace Enver.API.Observability;

/// <summary>Allowlist cerrada: sin excepciones, payloads, identidades, claves ni labels del cliente.</summary>
public sealed class AuthenticationEvents(ILogger<AuthenticationEvents> logger) : IAuthenticationEvents
{
    public void Record(AuthenticationEventKind kind, AuthenticationEventOutcome outcome)
    {
        if (!Enum.IsDefined(kind) || !Enum.IsDefined(outcome)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (kind == AuthenticationEventKind.Logout)
        {
            var label = outcome switch
            {
                AuthenticationEventOutcome.Confirmed => "confirmed",
                AuthenticationEventOutcome.Rejected => "rejected",
                AuthenticationEventOutcome.RateLimited => "rate_limited",
                AuthenticationEventOutcome.Unavailable => "dependency_unavailable",
                AuthenticationEventOutcome.Uncertain => "uncertain",
                _ => throw new ArgumentOutOfRangeException(nameof(outcome))
            };
            logger.LogInformation(new EventId(2045, "Logout"), "Authentication {Operation} {Outcome}", "logout", label);
            return;
        }
        logger.LogInformation(new EventId(2036, "Authentication"),
            "Authentication {Operation} {Kind} {Outcome}", "register", kind, outcome);
    }
}
