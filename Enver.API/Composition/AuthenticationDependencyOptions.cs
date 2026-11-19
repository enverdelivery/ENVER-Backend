using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Microsoft.Extensions.Options;

namespace Enver.API.Composition;

/// <summary>Referencias de control EXTERNAS. No se generan generaciones ni se activan cuotas al arrancar.</summary>
public sealed class AuthenticationDependencyOptions
{
    public const string SectionName = "AuthenticationComposition";
    public string AuthorityRegion { get; set; } = string.Empty;
    public string QuotaConfigurationVersion { get; set; } = string.Empty;
    public string QuotaIdentityReference { get; set; } = string.Empty;
    public string QuotaGeneration { get; set; } = string.Empty;

    public RedisAuthenticationQuotaConfiguration Capture(string environment, RateLimitingOptions rate)
    {
        static AuthenticationQuotaDefinition Definition(RateLimitQuotaOptions? value) => value is null
            ? throw new ArgumentException("Cuota requerida.") : new(value.PermitLimit, TimeSpan.FromSeconds(value.WindowSeconds));
        var policy = new AuthenticationQuotaPolicyDescriptor(rate.PolicyRevision,
        [
            new(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Email, Definition(rate.RegisterEmail)),
            new(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Origin, Definition(rate.RegisterOrigin)),
            new(AuthenticationOperation.Login, AuthenticationQuotaScopeType.OriginEmail, Definition(rate.LoginOriginEmail)),
            new(AuthenticationOperation.Login, AuthenticationQuotaScopeType.Email, Definition(rate.LoginEmail)),
            new(AuthenticationOperation.Login, AuthenticationQuotaScopeType.Origin, Definition(rate.LoginOrigin)),
            new(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Account, Definition(rate.LogoutAccount)),
            new(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Origin, Definition(rate.LogoutOrigin))
        ]);
        return new(new(environment.ToLowerInvariant(), QuotaConfigurationVersion, QuotaIdentityReference, policy,
            AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(rate.MaxActivePartitions)), QuotaGeneration);
    }

    public override string ToString() => nameof(AuthenticationDependencyOptions);
}

internal sealed class AuthenticationDependencyValidator(IHostEnvironment environment, IOptions<RateLimitingOptions> rate) : IValidateOptions<AuthenticationDependencyOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationDependencyOptions value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value.AuthorityRegion) || value.AuthorityRegion.Length > 64 ||
                value.AuthorityRegion.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-')) throw new ArgumentException();
            _ = value.Capture(environment.EnvironmentName, rate.Value);
            return ValidateOptionsResult.Success;
        }
        catch { return ValidateOptionsResult.Fail("AuthenticationComposition: referencias externas/capacidad incompatibles."); }
    }
}
