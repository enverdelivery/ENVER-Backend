using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Enver.API.Configuration;

/// <summary>Registro preparado para integración futura, no invocado por Program.cs.</summary>
public static class AuthenticationOptionsValidation
{
    public static IServiceCollection AddValidatedAuthenticationOptions(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddSingleton<IValidateOptions<MongoDbOptions>>(new MongoDbOptionsValidator(environment.EnvironmentName));
        services.AddSingleton<IValidateOptions<RedisOptions>>(new RedisOptionsValidator(environment.EnvironmentName));
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services.AddSingleton<IValidateOptions<AuthenticationOptions>, AuthenticationOptionsValidator>();
        services.AddSingleton<IValidateOptions<RateLimitingOptions>, RateLimitingOptionsValidator>();
        services.AddOptions<MongoDbOptions>().Configure(o => configuration.GetRequiredSection(MongoDbOptions.SectionName).Bind(o)).ValidateOnStart();
        services.AddOptions<RedisOptions>().Configure(o => configuration.GetRequiredSection(RedisOptions.SectionName).Bind(o)).ValidateOnStart();
        services.AddOptions<JwtOptions>().Configure(o => configuration.GetRequiredSection(JwtOptions.SectionName).Bind(o)).ValidateOnStart();
        services.AddOptions<AuthenticationOptions>().Configure(o => configuration.GetRequiredSection(AuthenticationOptions.SectionName).Bind(o)).ValidateOnStart();
        services.AddOptions<RateLimitingOptions>().Configure(o => configuration.GetRequiredSection(RateLimitingOptions.SectionName).Bind(o)).ValidateOnStart();
        return services;
    }
}

internal sealed class MongoDbOptionsValidator(string environment) : IValidateOptions<MongoDbOptions>
{
    public ValidateOptionsResult Validate(string? name, MongoDbOptions o) => Checks.Result(
    [
        (!string.IsNullOrWhiteSpace(o.ConnectionString), "MongoDb: conexión externa requerida."),
        (o.DatabaseName == $"enver_{environment.ToLowerInvariant()}" || o.DatabaseName.StartsWith($"enver_{environment.ToLowerInvariant()}_", StringComparison.Ordinal), "MongoDb: base aislada por ambiente requerida."),
        (o.UsersCollectionName == "users", "MongoDb: colección incompatible."),
        (o.OperationTimeoutMilliseconds > 0 && o.ConnectionTimeoutMilliseconds > 0, "MongoDb: timeouts positivos requeridos."),
        (o.WriteConcernCandidate == "majority" && o.ReadConcernCandidate == "majority" && o.ReadPreference == "primary", "MongoDb: candidatos incompatibles.")
    ]);
}

internal sealed class RedisOptionsValidator(string environment) : IValidateOptions<RedisOptions>
{
    public ValidateOptionsResult Validate(string? name, RedisOptions o) => Checks.Result(
    [
        (!string.IsNullOrWhiteSpace(o.ConnectionString), "Redis: conexión externa requerida."),
        (o.NamespacePrefix == $"enver:{environment.ToLowerInvariant()}", "Redis: namespace aislado por ambiente requerido."),
        (o.RevocationPurpose == "auth:revocation" && o.RateLimitingPurpose == "auth:ratelimit", "Redis: propósitos incompatibles."),
        (o.OperationTimeoutMilliseconds > 0 && o.ConnectionTimeoutMilliseconds > 0 && o.RevocationOperationalMarginSeconds > 0, "Redis: límites positivos requeridos."),
        (o.FailClosed && o.RequiredRevocationEvictionPolicy == "noeviction", "Redis: requisito fail-closed/noeviction requerido.")
    ]);
}

internal sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions o)
    {
        var checks = new List<(bool, string)>
        {
            (o.Algorithm == "HS256" && o.Issuer == "enver-api" && o.Audience == "enver-mobile", "Jwt: política incompatible."),
            (o.LifetimeSeconds == 900 && o.ClockSkewSeconds is >= 0 and <= 30, "Jwt: vida/skew incompatibles."),
            (!string.IsNullOrWhiteSpace(o.ActiveKid) && o.Keys.Count > 0, "Jwt: kid activo y claves externas requeridos.")
        };
        var kids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var key in o.Keys)
        {
            checks.Add((key is not null && !string.IsNullOrWhiteSpace(key.Kid) && kids.Add(key.Kid), "Jwt: kid ausente o duplicado."));
            checks.Add((Checks.Base64(key?.KeyBase64, 32), "Jwt: clave externa Base64 de al menos 32 bytes requerida; longitud no acredita entropía."));
        }
        checks.Add((kids.Contains(o.ActiveKid), "Jwt: kid activo inexistente."));
        return Checks.Result(checks);
    }
}

internal sealed class AuthenticationOptionsValidator(IOptions<MongoDbOptions> mongo, IOptions<RedisOptions> redis) : IValidateOptions<AuthenticationOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationOptions o) => Checks.Result(
    [
        (o.InstitutionalDomain == "uttt.edu.mx", "Authentication: dominio incompatible."),
        (o.FullNameMaxUnicodeScalars == 200 && o.EmailMaxAsciiBytes == 254 && o.EmailLocalPartMaxAsciiBytes == 64, "Authentication: límites de nombre/correo incompatibles."),
        (o.PasswordCreationMinUnicodeScalars == 12 && o.PasswordMaxUnicodeScalars == 128 && o.PasswordMaxUtf8Bytes == 512, "Authentication: límites de contraseña incompatibles."),
        (o.MaxRequestBodyBytes == 16384 && o.MaxJsonDepth == 4 && o.MaxJwtBytes == 4096, "Authentication: límites HTTP incompatibles."),
        (o.RegisterBudgetMilliseconds > 0 && o.LoginBudgetMilliseconds > 0 && o.LogoutBudgetMilliseconds > 0 && o.HashingConcurrencyLimit > 0, "Authentication: presupuestos/concurrencia positivos requeridos."),
        (mongo.Value.OperationTimeoutMilliseconds <= Math.Min(o.RegisterBudgetMilliseconds, Math.Min(o.LoginBudgetMilliseconds, o.LogoutBudgetMilliseconds)) && redis.Value.OperationTimeoutMilliseconds <= Math.Min(o.RegisterBudgetMilliseconds, Math.Min(o.LoginBudgetMilliseconds, o.LogoutBudgetMilliseconds)), "Authentication: timeout de dependencia excede presupuesto.")
    ]);
}

internal sealed class RateLimitingOptionsValidator(IOptions<JwtOptions> jwt) : IValidateOptions<RateLimitingOptions>
{
    public ValidateOptionsResult Validate(string? name, RateLimitingOptions o)
    {
        var quotas = new[] { o.RegisterEmail, o.RegisterOrigin, o.LoginOriginEmail, o.LoginEmail, o.LoginOrigin, o.LogoutAccount, o.LogoutOrigin };
        var keyValid = Checks.Base64(o.PseudonymizationKeyBase64, 1);
        return Checks.Result(
        [
            (quotas.All(q => q is { PermitLimit: > 0, WindowSeconds: > 0 }), "RateLimiting: las siete cuotas requieren máximo/ventana positivos."),
            (!string.IsNullOrWhiteSpace(o.PolicyRevision), "RateLimiting: revisión de política requerida."),
            (o.MaxActivePartitions is null or > 0, "RateLimiting: cardinalidad positiva si se declara."),
            (keyValid, "RateLimiting: clave externa HMAC Base64 requerida."),
            (!keyValid || !jwt.Value.Keys.Any(k => k?.KeyBase64 == o.PseudonymizationKeyBase64), "RateLimiting: no reutilizar clave JWT.")
        ]);
    }
}

internal static class Checks
{
    internal static ValidateOptionsResult Result(IEnumerable<(bool Valid, string Message)> checks)
    {
        var errors = checks.Where(c => !c.Valid).Select(c => c.Message).ToArray();
        return errors.Length == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    internal static bool Base64(string? value, int minimumBytes)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        byte[]? bytes = null;
        try { bytes = Convert.FromBase64String(value); return bytes.Length >= minimumBytes && Convert.ToBase64String(bytes) == value; }
        catch (FormatException) { return false; }
        finally { if (bytes is not null) CryptographicOperations.ZeroMemory(bytes); }
    }
}
