using Enver.API.Features.Authentication.Application.Quotas;

namespace Enver.API.Http;

public enum AuthenticationOriginStatus { Available, Unavailable }

public sealed class AuthenticationOriginResult
{
    private AuthenticationOriginResult(AuthenticationCanonicalOrigin? origin) => Origin = origin;
    public AuthenticationCanonicalOrigin? Origin { get; }
    public AuthenticationOriginStatus Status => Origin is null ? AuthenticationOriginStatus.Unavailable : AuthenticationOriginStatus.Available;
    public static AuthenticationOriginResult Unavailable { get; } = new(null);
    internal static AuthenticationOriginResult Available(AuthenticationCanonicalOrigin origin) => new(origin);
    public override string ToString() => nameof(AuthenticationOriginResult);
}

public interface IAuthenticationOriginProvider
{
    AuthenticationOriginResult Resolve(HttpContext context);
}

/// <summary>
/// Feature exclusivamente de servidor: una futura capa de transporte autorizada deberá crearla.
/// Nunca se construye a partir de un header del cliente ni prueba por sí sola confianza de proxies.
/// En proxy requerido, resolvedOrigin solo se suministra tras forwarding acreditado; null mantiene cierre.
/// No existe middleware que la cree en producción en 053 (pendiente de 056/infraestructura).
/// </summary>
public sealed class AuthenticationOriginTrustContext
{
    private AuthenticationOriginTrustContext(bool directConnectionApproved, AuthenticationCanonicalOrigin? verifiedForwardedOrigin)
    {
        DirectConnectionApproved = directConnectionApproved;
        VerifiedForwardedOrigin = verifiedForwardedOrigin;
    }

    internal bool DirectConnectionApproved { get; }
    internal AuthenticationCanonicalOrigin? VerifiedForwardedOrigin { get; }
    public static AuthenticationOriginTrustContext ApproveDirectConnection() => new(true, null);
    public static AuthenticationOriginTrustContext RequireVerifiedForwarding(AuthenticationCanonicalOrigin? resolvedOrigin) => new(false, resolvedOrigin);
    public override string ToString() => nameof(AuthenticationOriginTrustContext);
}

/// <summary>
/// No lee ni parsea Forwarded/X-Forwarded-For/X-Original-For. Sin evidencia de transporte, no admite.
/// No deduce confianza de RemoteIpAddress, ni usa el proxy peer como fallback a forwarding fallido.
/// Unavailable se traducirá a no admisión/503 antes de despachar cuotas en la futura frontera HTTP.
/// </summary>
public sealed class AuthenticationOriginProvider : IAuthenticationOriginProvider
{
    public AuthenticationOriginResult Resolve(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var trust = context.Features.Get<AuthenticationOriginTrustContext>();
        if (trust is null) return AuthenticationOriginResult.Unavailable;
        if (!trust.DirectConnectionApproved)
            return trust.VerifiedForwardedOrigin is { } resolved
                ? AuthenticationOriginResult.Available(resolved) : AuthenticationOriginResult.Unavailable;
        if (context.Connection.RemoteIpAddress is not { } peer) return AuthenticationOriginResult.Unavailable;
        try { return AuthenticationOriginResult.Available(AuthenticationCanonicalOrigin.FromAddress(peer)); }
        catch (ArgumentException) { return AuthenticationOriginResult.Unavailable; }
    }
}
