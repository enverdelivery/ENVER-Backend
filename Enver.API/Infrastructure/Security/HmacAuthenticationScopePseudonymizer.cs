using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Microsoft.Extensions.Options;

namespace Enver.API.Infrastructure.Security;

/// <summary>
/// Snapshot inmutable de clave/ambiente, sin recarga automática ni registro DI productivo.
/// Cambiar clave, representación, dominio o versión requiere drenaje coordinado antes de activar.
/// </summary>
public sealed class HmacAuthenticationScopePseudonymizer : IAuthenticationScopePseudonymizer, IDisposable
{
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private readonly object gate = new();
    private readonly string environment;
    private readonly byte[] key;
    private bool disposed;

    public HmacAuthenticationScopePseudonymizer(IOptions<RateLimitingOptions> options, IOptions<JwtOptions> jwt, IHostEnvironment host)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(jwt);
        ArgumentNullException.ThrowIfNull(host);
        // Validar incluso cuando el caller utiliza Options.Create sin el validador registrado.
        var encoded = options.Value.PseudonymizationKeyBase64;
        if (!Checks.Base64(encoded, 32) || Checks.PseudonymizationKeyMatchesJwt(encoded, jwt.Value))
            throw new ArgumentException("Configuración de pseudonimización incompatible.");
        var name = host.EnvironmentName;
        if (string.IsNullOrWhiteSpace(name) || name.Any(c => char.IsControl(c) || char.IsWhiteSpace(c)))
            throw new ArgumentException("Ambiente de pseudonimización incompatible.");
        environment = name.ToLowerInvariant();
        try { _ = Utf8.GetByteCount(environment); }
        catch (EncoderFallbackException) { throw new ArgumentException("Ambiente de pseudonimización incompatible."); }
        key = Convert.FromBase64String(encoded);
    }

    public string Pseudonymize(AuthenticationScopeIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        using var payload = new MemoryStream();
        byte[]? digest = null;
        try
        {
            WriteField(payload, "enver.auth.quota");
            WriteField(payload, "1");
            WriteField(payload, environment);
            WriteField(payload, identity.Operation switch
            {
                AuthenticationOperation.Register => "register",
                AuthenticationOperation.Login => "login",
                AuthenticationOperation.Logout => "logout",
                _ => throw new ArgumentException("Operación incompatible.")
            });
            WriteField(payload, identity.ScopeType switch
            {
                AuthenticationQuotaScopeType.Email => "email",
                AuthenticationQuotaScopeType.Origin => "origin",
                AuthenticationQuotaScopeType.OriginEmail => "origin-email",
                AuthenticationQuotaScopeType.Account => "account",
                _ => throw new ArgumentException("Ámbito incompatible.")
            });
            foreach (var component in identity.Components) WriteField(payload, component);
            lock (gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                digest = HMACSHA256.HashData(key, payload.GetBuffer().AsSpan(0, checked((int)payload.Length)));
            }
            return Convert.ToBase64String(digest).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
        finally
        {
            if (digest is not null) CryptographicOperations.ZeroMemory(digest);
            CryptographicOperations.ZeroMemory(payload.GetBuffer());
        }
    }

    private static void WriteField(Stream output, string value)
    {
        var bytes = Utf8.GetBytes(value);
        try
        {
            Span<byte> length = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(length, checked((uint)bytes.Length));
            output.Write(length);
            output.Write(bytes);
        }
        finally { CryptographicOperations.ZeroMemory(bytes); }
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            CryptographicOperations.ZeroMemory(key);
            disposed = true;
        }
    }

    public override string ToString() => nameof(HmacAuthenticationScopePseudonymizer);
}
