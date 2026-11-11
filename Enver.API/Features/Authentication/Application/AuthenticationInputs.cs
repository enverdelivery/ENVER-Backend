using System.Globalization;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Security;

namespace Enver.API.Features.Authentication.Application;

/// <summary>Datos efímeros. Nunca registrar/serializar como evento, ni conservar después de la operación.</summary>
public sealed class RegisterInput(string? fullName, string? email, string? password, bool readableBody = true)
{
    public string? FullName { get; } = fullName;
    public string? Email { get; } = email;
    public string? Password { get; } = password;
    public bool ReadableBody { get; } = readableBody;
    public override string ToString() => nameof(RegisterInput);
}

/// <summary>Solo se construye desde el principal acreditado por el configurador Bearer; nunca desde binding HTTP.</summary>
public sealed class LogoutInput
{
    private LogoutInput(AuthenticationAuthenticatedAccount account, string jti, DateTimeOffset expiresAtUtc)
    { Account = account; Jti = jti; ExpiresAtUtc = expiresAtUtc; }

    public AuthenticationAuthenticatedAccount Account { get; }
    public string Issuer => "enver-api";
    public string Jti { get; }
    public DateTimeOffset ExpiresAtUtc { get; }

    public static bool TryFromAuthenticatedContext(HttpContext context, out LogoutInput? input)
    {
        input = null;
        if (context.User.Identity?.IsAuthenticated != true || context.Features.Get<ValidatedJwtIdentity>() is not { } identity
            || !ReferenceEquals(identity.Principal, context.User) || !JwtClaimsValidator.IsCanonicalSubject(identity.Subject)
            || !JwtClaimsValidator.IsCanonicalJti(identity.Jti)) return false;
        var claims = context.User.Claims.ToArray();
        bool Single(string name, string value) => claims.Count(c => c.Type == name) == 1 && claims.Single(c => c.Type == name).Value == value;
        var expiry = claims.Where(c => c.Type == "exp").ToArray();
        if (!Single("sub", identity.Subject) || !Single("jti", identity.Jti) || !Single("iss", IssuerName)
            || expiry.Length != 1 || expiry[0].Value.Length == 0 || expiry[0].Value.Any(c => !char.IsAsciiDigit(c))
            || !long.TryParse(expiry[0].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)) return false;
        try
        {
            input = new(AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal(context.User), identity.Jti,
                DateTimeOffset.FromUnixTimeSeconds(seconds));
            return true;
        }
        catch (ArgumentException) { return false; }
    }

    private const string IssuerName = "enver-api";
    public override string ToString() => nameof(LogoutInput);
}
