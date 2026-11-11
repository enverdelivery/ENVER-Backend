using System.Security.Cryptography;
using Enver.API.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Enver.API.Security;

/// <summary>Snapshot local por host. Nunca descubre claves ni modifica el conjunto en runtime.</summary>
public sealed class JwtKeyRing
{
    private readonly Dictionary<string, SymmetricSecurityKey> keys = new(StringComparer.Ordinal);
    private readonly string activeKid;
    internal int SkewSeconds { get; }

    public JwtKeyRing(JwtOptions options, RateLimitingOptions rateLimiting)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(rateLimiting);
        var validation = new JwtOptionsValidator().Validate(null, options);
        if (validation.Failed || !Checks.Base64(rateLimiting.PseudonymizationKeyBase64, 32)
            || Checks.PseudonymizationKeyMatchesJwt(rateLimiting.PseudonymizationKeyBase64, options))
            throw new OptionsValidationException(nameof(JwtOptions), typeof(JwtOptions), ["JWT: configuración de claves incompatible."]);
        activeKid = options.ActiveKid;
        SkewSeconds = options.ClockSkewSeconds!.Value;
        foreach (var configured in options.Keys)
        {
            var material = Convert.FromBase64String(configured!.KeyBase64);
            try { keys.Add(configured.Kid, new SymmetricSecurityKey(material) { KeyId = configured.Kid }); }
            finally { CryptographicOperations.ZeroMemory(material); }
        }
    }

    internal bool TryGetSigningCredentials(out SigningCredentials credentials)
    {
        if (keys.TryGetValue(activeKid, out var key))
        { credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); return true; }
        credentials = null!; return false;
    }

    internal bool TryGetValidationKey(string kid, out SecurityKey key)
    {
        if (keys.TryGetValue(kid, out var value)) { key = value; return true; }
        key = null!; return false;
    }

    /// <summary>Puerta del procedimiento manual; el operador debe acreditar el cese de toda emisión anterior.</summary>
    public static bool CanRetirePreviousKey(DateTimeOffset lastPossibleEmissionUtc, DateTimeOffset nowUtc) =>
        lastPossibleEmissionUtc.Offset == TimeSpan.Zero && nowUtc.Offset == TimeSpan.Zero
        && nowUtc - lastPossibleEmissionUtc >= TimeSpan.FromSeconds(930);

    public override string ToString() => nameof(JwtKeyRing);
}
