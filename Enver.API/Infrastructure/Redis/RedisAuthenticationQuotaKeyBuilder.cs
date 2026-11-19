using Enver.API.Http;
using Enver.API.Features.Authentication.Application.Quotas;

namespace Enver.API.Infrastructure.Redis;

/// <summary>Identidad física estable; no incluye revisión, generación ni material HMAC.</summary>
public sealed class RedisAuthenticationQuotaKeyBuilder
{
    public RedisAuthenticationQuotaKeyBuilder(string environment)
    {
        if (string.IsNullOrEmpty(environment) || environment.Length > 64 ||
            environment.Any(c => !(c is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-')))
            throw new ArgumentException("Ambiente de cuotas inválido.");
        Prefix = $"enver:{{{environment}:auth:ratelimit}}:";
    }

    public string Prefix { get; }
    public string Registry => Prefix + "registry";
    public string Metadata => Prefix + "metadata";
    public string Bucket(AuthenticationOperation operation, AuthenticationQuotaScopeType scope, string opaqueId)
    {
        if (!AuthenticationQuotaPolicyDescriptor.RequiredScopes(operation).Contains(scope) || !IsOpaqueId(opaqueId))
            throw new ArgumentException("Identidad de cuotas inválida.");
        return Prefix + "bucket:" + Member(operation, scope, opaqueId);
    }

    public string CandidateBucket(string member)
    {
        var parts = member.Split(':');
        if (parts.Length != 3) throw new ArgumentException("Candidato de cuotas inválido.");
        var operation = parts[0] switch { "register" => AuthenticationOperation.Register, "login" => AuthenticationOperation.Login,
            "logout" => AuthenticationOperation.Logout, _ => throw new ArgumentException("Candidato de cuotas inválido.") };
        var scope = parts[1] switch { "email" => AuthenticationQuotaScopeType.Email, "origin" => AuthenticationQuotaScopeType.Origin,
            "origin-email" => AuthenticationQuotaScopeType.OriginEmail, "account" => AuthenticationQuotaScopeType.Account,
            _ => throw new ArgumentException("Candidato de cuotas inválido.") };
        return Bucket(operation, scope, parts[2]);
    }

    public static string OperationToken(AuthenticationOperation operation) => operation switch
    {
        AuthenticationOperation.Register => "register", AuthenticationOperation.Login => "login", AuthenticationOperation.Logout => "logout",
        _ => throw new ArgumentException("Operación de cuotas inválida.")
    };
    public static string ScopeToken(AuthenticationQuotaScopeType scope) => scope switch
    {
        AuthenticationQuotaScopeType.Email => "email", AuthenticationQuotaScopeType.Origin => "origin",
        AuthenticationQuotaScopeType.OriginEmail => "origin-email", AuthenticationQuotaScopeType.Account => "account",
        _ => throw new ArgumentException("Ámbito de cuotas inválido.")
    };
    public static string Member(AuthenticationOperation operation, AuthenticationQuotaScopeType scope, string opaqueId) =>
        $"{OperationToken(operation)}:{ScopeToken(scope)}:{opaqueId}";
    public static bool IsAttemptId(string? value) => value is { Length: 32 } && value.Any(c => c != '0') &&
        value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');
    public static bool IsOpaqueId(string? value)
    {
        if (value is not { Length: 43 } || value.Any(c => !(c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_')))
            return false;
        try
        {
            var bytes = Convert.FromBase64String(value.Replace('-', '+').Replace('_', '/') + "=");
            return bytes.Length == 32 && Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_') == value;
        }
        catch (FormatException) { return false; }
    }
    public override string ToString() => nameof(RedisAuthenticationQuotaKeyBuilder);
}
