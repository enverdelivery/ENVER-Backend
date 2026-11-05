using System.Security.Cryptography;
using System.Text;

namespace Enver.API.Infrastructure.Redis;

/// <summary>
/// Construye únicamente claves de revocación con namespace fijo.
/// La validación de formato no autentica un identificador JWT.
/// </summary>
public static class RedisKeyBuilder
{
    public const string Issuer = "enver-api";

    public static string BuildRevocationKey(string environment, string issuer, string jti)
    {
        ValidateEnvironment(environment);
        if (!string.Equals(issuer, Issuer, StringComparison.Ordinal))
        {
            throw new ArgumentException("El issuer de revocación no es válido.", nameof(issuer));
        }

        ValidateJti(jti);
        return $"enver:{environment}:auth:revoked:{Issuer}:{jti}";
    }

    private static void ValidateEnvironment(string environment)
    {
        if (string.IsNullOrEmpty(environment) || environment != environment.ToLowerInvariant())
        {
            throw new ArgumentException("El ambiente debe ser canónico y no vacío.", nameof(environment));
        }

        foreach (var character in environment)
        {
            if (!(character is >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_'))
            {
                throw new ArgumentException("El ambiente contiene caracteres no permitidos.", nameof(environment));
            }
        }
    }

    private static void ValidateJti(string jti)
    {
        if (jti.Length != 43 || jti.Contains('=') || jti.Any(character =>
                !(character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_')))
        {
            throw new ArgumentException("El jti debe ser Base64url canónico de 32 bytes sin padding.", nameof(jti));
        }

        byte[]? bytes = null;
        try
        {
            bytes = Convert.FromBase64String(jti.Replace('-', '+').Replace('_', '/') + "=");
            if (bytes.Length != 32 || !CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(jti), Encoding.ASCII.GetBytes(ToBase64Url(bytes))))
            {
                throw new ArgumentException("El jti debe ser Base64url canónico de 32 bytes sin padding.", nameof(jti));
            }
        }
        catch (FormatException)
        {
            throw new ArgumentException("El jti debe ser Base64url canónico de 32 bytes sin padding.", nameof(jti));
        }
        finally
        {
            if (bytes is not null)
            {
                CryptographicOperations.ZeroMemory(bytes);
            }
        }
    }

    private static string ToBase64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
