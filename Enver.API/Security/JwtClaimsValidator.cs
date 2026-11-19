using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Enver.API.Security;

/// <summary>El preparse solo rechaza. La identidad exige después firma y todas las validaciones.</summary>
public sealed class JwtClaimsValidator
{
    public const int MaximumCompactBytes = 4096;
    private readonly JwtKeyRing ring;
    private readonly TimeProvider clock;
    private readonly StrictTokenHandler handler;
    public JwtClaimsValidator(JwtKeyRing ring, TimeProvider clock)
    {
        this.ring = ring; this.clock = clock;
        handler = new StrictTokenHandler(this) { MapInboundClaims = false, MaximumTokenSizeInBytes = MaximumCompactBytes };
    }

    public Task<TokenValidationResult> ValidateAsync(string token) => handler.ValidateTokenAsync(token, CreateParameters());
    internal TokenHandler Handler => handler;
    internal TokenValidationParameters CreateParameters() => new()
    {
        RequireSignedTokens = true, ValidateIssuerSigningKey = true,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        ValidateIssuer = true, ValidIssuer = "enver-api",
        ValidateAudience = true, RequireAudience = true, ValidAudience = "enver-mobile",
        IgnoreTrailingSlashWhenValidatingAudience = false,
        ValidateLifetime = true, RequireExpirationTime = true, ClockSkew = TimeSpan.FromSeconds(ring.SkewSeconds),
        LifetimeValidator = (_, _, token, _) => token is JsonWebToken jwt && TryRead(jwt.EncodedToken, out var profile) && ValidTime(profile),
        IssuerSigningKeyResolver = (_, _, kid, _) => kid is not null && ring.TryGetValidationKey(kid, out var key) ? [key] : [],
        TryAllIssuerSigningKeys = false, SaveSigninToken = false,
        IncludeTokenOnFailedValidation = false, LogTokenId = false, LogValidationExceptions = false,
        AuthenticationType = AuthenticationPolicies.Scheme
    };

    internal bool TryRead(string? token, out JwtProfile profile)
    {
        profile = default;
        // JWT compacto solo ASCII: longitud equivale a bytes y se comprueba antes de decode/cripto.
        if (string.IsNullOrEmpty(token) || token.Length > MaximumCompactBytes || token.Any(c => c > 127)) return false;
        var segments = token.Split('.');
        if (segments.Length != 3 || segments.Any(s => s.Length == 0 || s.Any(c => !IsBase64Url(c)))) return false;
        try
        {
            var headerBytes = Base64UrlEncoder.DecodeBytes(segments[0]);
            var payloadBytes = Base64UrlEncoder.DecodeBytes(segments[1]);
            var utf8 = new UTF8Encoding(false, true);
            _ = utf8.GetCharCount(headerBytes); _ = utf8.GetCharCount(payloadBytes);
            if (Base64UrlEncoder.Encode(headerBytes) != segments[0] || Base64UrlEncoder.Encode(payloadBytes) != segments[1]) return false;
            using var header = JsonDocument.Parse(headerBytes, new JsonDocumentOptions { MaxDepth = 4 });
            using var payload = JsonDocument.Parse(payloadBytes, new JsonDocumentOptions { MaxDepth = 4 });
            if (!UniqueObject(header.RootElement) || !UniqueObject(payload.RootElement)) return false;
            var h = header.RootElement; var p = payload.RootElement;
            foreach (var property in h.EnumerateObject())
                if (property.Name is not ("alg" or "kid" or "typ")) return false;
            if (!String(h, "alg", out var alg) || alg != "HS256" || !String(h, "kid", out var kid)
                || !ring.TryGetValidationKey(kid, out _)) return false;
            if (h.TryGetProperty("typ", out var typ) && (typ.ValueKind != JsonValueKind.String || typ.GetString() != "JWT")) return false;
            foreach (var property in p.EnumerateObject())
                if (property.Name is not ("sub" or "jti" or "iss" or "aud" or "iat" or "exp" or "nbf")) return false;
            if (!String(p, "sub", out var sub) || !IsCanonicalSubject(sub)
                || !String(p, "jti", out var jti) || !IsCanonicalJti(jti)
                || !String(p, "iss", out var issuer) || issuer != "enver-api"
                || !String(p, "aud", out var audience) || audience != "enver-mobile"
                || !Integer(p, "iat", out var iat) || !Integer(p, "exp", out var exp)) return false;
            long? nbf = null;
            if (p.TryGetProperty("nbf", out _))
            { if (!Integer(p, "nbf", out var value)) return false; nbf = value; }
            // También prueba representabilidad sin overflow antes de pasar al stack.
            _ = DateTimeOffset.FromUnixTimeSeconds(iat); _ = DateTimeOffset.FromUnixTimeSeconds(exp);
            if (nbf is { } n) _ = DateTimeOffset.FromUnixTimeSeconds(n);
            profile = new(sub, jti, iat, exp, nbf); return true;
        }
        catch (Exception error) when (error is JsonException or ArgumentException or FormatException or OverflowException or InvalidOperationException)
        { return false; }
    }

    private bool ValidTime(JwtProfile p)
    {
        try
        {
            var now = clock.GetUtcNow(); var skew = TimeSpan.FromSeconds(ring.SkewSeconds);
            return p.Expires > p.Issued && checked(p.Expires - p.Issued) == 900
                && DateTimeOffset.FromUnixTimeSeconds(p.Issued) <= now.Add(skew)
                && now < DateTimeOffset.FromUnixTimeSeconds(p.Expires).Add(skew)
                && (p.NotBefore is not { } n || (n <= p.Expires && DateTimeOffset.FromUnixTimeSeconds(n) <= now.Add(skew)));
        }
        catch (Exception error) when (error is ArgumentException or OverflowException) { return false; }
    }

    public static bool IsCanonicalSubject(string? value) => value is { Length: 36 }
        && Guid.TryParseExact(value, "D", out var id) && value == id.ToString("D")
        && value[14] == '4' && value[19] is '8' or '9' or 'a' or 'b';

    public static bool IsCanonicalJti(string? value)
    {
        if (value is not { Length: 43 } || value.Any(c => !IsBase64Url(c))) return false;
        try { var bytes = Base64UrlEncoder.DecodeBytes(value); return bytes.Length == 32 && Base64UrlEncoder.Encode(bytes) == value; }
        catch (FormatException) { return false; }
    }
    private static bool IsBase64Url(char c) => char.IsAsciiLetterOrDigit(c) || c is '-' or '_';
    private static bool UniqueObject(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) return false;
        var names = new HashSet<string>(StringComparer.Ordinal);
        return element.EnumerateObject().All(p => names.Add(p.Name));
    }
    private static bool String(JsonElement element, string name, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(name, out var p) || p.ValueKind != JsonValueKind.String) return false;
        value = p.GetString()!; return true;
    }
    private static bool Integer(JsonElement element, string name, out long value)
    {
        value = 0;
        if (!element.TryGetProperty(name, out var p) || p.ValueKind != JsonValueKind.Number) return false;
        var raw = p.GetRawText();
        return raw.All(c => c is >= '0' and <= '9' or '-') && p.TryGetInt64(out value);
    }

    private static TokenValidationResult Rejected() => new()
    { IsValid = false, Exception = new SecurityTokenValidationException("No fue posible autenticar la solicitud.") };

    private sealed class StrictTokenHandler(JwtClaimsValidator owner) : JsonWebTokenHandler
    {
        public override async Task<TokenValidationResult> ValidateTokenAsync(string token, TokenValidationParameters validationParameters)
        {
            if (!owner.TryRead(token, out _)) return Rejected();
            try
            {
                // No acepta parámetros debilitados por un consumidor directo.
                var result = await base.ValidateTokenAsync(token, owner.CreateParameters()).ConfigureAwait(false);
                return result.IsValid ? result : Rejected();
            }
            catch (Exception error) when (error is SecurityTokenException or ArgumentException or FormatException)
            { return Rejected(); }
        }
    }

    internal readonly record struct JwtProfile(string Subject, string Jti, long Issued, long Expires, long? NotBefore);
}
