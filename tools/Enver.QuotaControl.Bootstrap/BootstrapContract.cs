using System.Globalization;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;

namespace Enver.QuotaControl.Bootstrap;

/// <summary>Contrato fijo del bootstrap controlado. No genera G ni descubre el endpoint.</summary>
internal static class BootstrapContract
{
    public const string Environment = "development";
    public const string ConfigurationVersion = "quota-development-academic-initial-v1";
    public const string IdentityReference = "hmac-development-fecf274e-v1";
    public const string PolicyRevision = "academic-initial";
    public const int MaxActivePartitions = 5000;
    public const long MemoryBudgetBytes = 67108864;
    public const string NewReplicationGroupId = "enver-dev-valkey-v2";
    public const string OldEndpoint = "master.enver-dev-valkey.kb2qe0.use1.cache.amazonaws.com";
    public const string AuthorityTable = "enver-dev-authentication-authority";
    public const string Region = "us-east-1";
    public const int Port = 6379;
    public const int OperationTimeoutMilliseconds = 750;
    public const int ConnectionTimeoutMilliseconds = 2000;
    public const int RevocationOperationalMarginSeconds = 60;
    public const int JwtLifetimeSeconds = 900;
    public const int JwtClockSkewSeconds = 30;
    public const int AuthorityTimeoutMilliseconds = 2000;

    private static readonly (AuthenticationOperation Operation, AuthenticationQuotaScopeType Scope)[] OrderedRules =
    [
        (AuthenticationOperation.Register, AuthenticationQuotaScopeType.Email),
        (AuthenticationOperation.Register, AuthenticationQuotaScopeType.Origin),
        (AuthenticationOperation.Login, AuthenticationQuotaScopeType.Email),
        (AuthenticationOperation.Login, AuthenticationQuotaScopeType.Origin),
        (AuthenticationOperation.Login, AuthenticationQuotaScopeType.OriginEmail),
        (AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Origin),
        (AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Account)
    ];

    public static AuthenticationQuotaPolicyDescriptor Policy => AuthenticationQuotaPolicyDescriptor.AcademicInitial;

    public static long MaximumWindowMicroseconds =>
        RedisAuthenticationQuotaConfiguration.WindowMicroseconds(Policy.MaximumWindow);

    /// <summary>Mismo orden de labels/ARGV que ConsumeAuthenticationQuota.lua.</summary>
    public static string SnapshotText
    {
        get
        {
            var parts = new List<string>
            {
                Environment,
                ConfigurationVersion,
                IdentityReference,
                PolicyRevision,
                MaxActivePartitions.ToString(CultureInfo.InvariantCulture),
                MemoryBudgetBytes.ToString(CultureInfo.InvariantCulture)
            };
            foreach (var (operation, scope) in OrderedRules)
            {
                var definition = Policy.GetDefinition(operation, scope);
                parts.Add(definition.PermitLimit.ToString(CultureInfo.InvariantCulture));
                parts.Add(RedisAuthenticationQuotaConfiguration.WindowMicroseconds(definition.Window)
                    .ToString(CultureInfo.InvariantCulture));
            }

            return string.Join('|', parts);
        }
    }

    public static void ValidateConfiguration(string configurationVersion, string identityReference)
    {
        if (!string.Equals(configurationVersion, ConfigurationVersion, StringComparison.Ordinal) ||
            !string.Equals(identityReference, IdentityReference, StringComparison.Ordinal))
        {
            throw new BootstrapValidationException("configuration-reference-mismatch");
        }
    }

    public static void ValidateGeneration(string generation)
    {
        if (!RedisAuthenticationQuotaKeyBuilder.IsAttemptId(generation))
        {
            throw new BootstrapValidationException("generation-invalid");
        }
    }

    public static RedisAuthenticationQuotaConfiguration CreateConfiguration(string generation)
    {
        ValidateConfiguration(ConfigurationVersion, IdentityReference);
        ValidateGeneration(generation);
        var snapshot = new AuthenticationQuotaConfigurationSnapshot(
            Environment,
            ConfigurationVersion,
            IdentityReference,
            Policy,
            AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(MaxActivePartitions));
        return new RedisAuthenticationQuotaConfiguration(snapshot, generation);
    }
}

internal sealed class BootstrapValidationException(string code) : Exception(code)
{
    public string Code { get; } = code;
}

internal sealed class RedisCredential
{
    public RedisCredential(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length > 128 ||
            username.Any(c => c is '\r' or '\n') ||
            string.IsNullOrEmpty(password) || password.Length > 512 ||
            password.Any(c => c is '\r' or '\n'))
        {
            throw new BootstrapValidationException("redis-credential-invalid");
        }

        Username = username;
        Password = password;
    }

    public string Username { get; }
    public string Password { get; }

    public override string ToString() => "[REDACTED]";
}
