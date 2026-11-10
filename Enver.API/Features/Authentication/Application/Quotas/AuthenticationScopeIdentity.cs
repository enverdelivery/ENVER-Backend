using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Quotas;

/// <summary>
/// Representación, no validación institucional completa de 028. El caller debe haber acreditado
/// dominio/sintaxis/límites institucionales antes de construir esta identidad; no se integra a endpoints en 053.
/// </summary>
public sealed class AuthenticationCanonicalEmail
{
    private AuthenticationCanonicalEmail(string value) => Value = value;
    internal string Value { get; }

    public static AuthenticationCanonicalEmail FromValidatedEmail(string validatedEmail)
    {
        if (validatedEmail is null) throw new ArgumentException("Identidad de correo no disponible.");
        var trimmed = validatedEmail.Trim();
        if (trimmed.Length == 0 || trimmed.Any(c => c > 127 || char.IsWhiteSpace(c) || char.IsControl(c)))
            throw new ArgumentException("Representación de correo incompatible.");
        var at = trimmed.IndexOf('@');
        if (at <= 0 || at != trimmed.LastIndexOf('@') || at == trimmed.Length - 1)
            throw new ArgumentException("Representación de correo incompatible.");
        return new(trimmed.ToLowerInvariant());
    }

    public override string ToString() => nameof(AuthenticationCanonicalEmail);
}

/// <summary>Dirección canónica interna, todavía NO pseudonimizada. Nunca registrar su valor.</summary>
public sealed class AuthenticationCanonicalOrigin
{
    private AuthenticationCanonicalOrigin(string value) => Value = value;
    internal string Value { get; }

    public static AuthenticationCanonicalOrigin FromAddress(IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        // No perder el zone id al convertir IPv4-mapped IPv6.
        if (address.AddressFamily == AddressFamily.InterNetworkV6 && address.ScopeId != 0)
            throw new ArgumentException("Origen con zona no admitido.");
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        var prefix = address.AddressFamily switch
        {
            AddressFamily.InterNetwork => "ipv4:",
            AddressFamily.InterNetworkV6 => "ipv6:",
            _ => throw new ArgumentException("Familia de origen no admitida.")
        };
        return new(prefix + Convert.ToHexStringLower(address.GetAddressBytes()));
    }

    public override string ToString() => nameof(AuthenticationCanonicalOrigin);
}

/// <summary>
/// Consume exclusivamente el principal acreditado por la capa de autenticación anterior.
/// IsAuthenticated es una precondición, no prueba de firma: 053 no parsea ni valida JWT.
/// </summary>
public sealed class AuthenticationAuthenticatedAccount
{
    private AuthenticationAuthenticatedAccount(string value) => Value = value;
    internal string Value { get; }

    public static AuthenticationAuthenticatedAccount FromAuthenticatedPrincipal(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var identities = principal.Identities.Where(identity => identity.IsAuthenticated).ToArray();
        if (identities.Length != 1) throw new ArgumentException("Identidad autenticada no disponible o ambigua.");
        var subjects = identities[0].FindAll("sub").ToArray();
        if (subjects.Length != 1 || !Guid.TryParseExact(subjects[0].Value, "D", out var account) || account == Guid.Empty)
            throw new ArgumentException("Identidad de cuenta incompatible.");
        return new(account.ToString("D").ToLowerInvariant());
    }

    public override string ToString() => nameof(AuthenticationAuthenticatedAccount);
}

/// <summary>Componentes canónicos tipados; sin HttpContext, tokens, clave ni tipos Redis.</summary>
public sealed class AuthenticationScopeIdentity
{
    private AuthenticationScopeIdentity(AuthenticationOperation operation, AuthenticationQuotaScopeType scopeType, params string[] components)
    {
        if (!AuthenticationQuotaPolicyDescriptor.RequiredScopes(operation).Contains(scopeType))
            throw new ArgumentException("Ámbito incompatible con la operación.");
        Operation = operation;
        ScopeType = scopeType;
        Components = Array.AsReadOnly(components);
    }

    public AuthenticationOperation Operation { get; }
    public AuthenticationQuotaScopeType ScopeType { get; }
    internal IReadOnlyList<string> Components { get; }

    public static AuthenticationScopeIdentity Email(AuthenticationOperation operation, AuthenticationCanonicalEmail email)
    {
        ArgumentNullException.ThrowIfNull(email);
        return new(operation, AuthenticationQuotaScopeType.Email, email.Value);
    }

    public static AuthenticationScopeIdentity Origin(AuthenticationOperation operation, AuthenticationCanonicalOrigin origin)
    {
        ArgumentNullException.ThrowIfNull(origin);
        return new(operation, AuthenticationQuotaScopeType.Origin, origin.Value);
    }

    public static AuthenticationScopeIdentity OriginEmail(AuthenticationCanonicalOrigin origin, AuthenticationCanonicalEmail email)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(email);
        return new(AuthenticationOperation.Login, AuthenticationQuotaScopeType.OriginEmail, origin.Value, email.Value);
    }

    public static AuthenticationScopeIdentity Account(AuthenticationAuthenticatedAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);
        return new(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Account, account.Value);
    }

    public override string ToString() => nameof(AuthenticationScopeIdentity);
}
