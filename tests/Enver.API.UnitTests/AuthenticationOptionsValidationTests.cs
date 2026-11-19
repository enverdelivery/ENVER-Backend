using System.Text;
using Enver.API.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class AuthenticationOptionsValidationTests
{
    [Fact]
    public void Valid_synthetic_configuration_validates_all_five_options_without_exposing_secrets()
    {
        const string secret = "options-synthetic-secret";
        using var provider = CreateProvider(values =>
        {
            values["MongoDb:ConnectionString"] = $"mongodb://{secret}.invalid";
            values["Redis:ConnectionString"] = $"{secret}.invalid:6379";
            values["Jwt:Keys:0:KeyBase64"] = Key(32, 1);
            values["RateLimiting:PseudonymizationKeyBase64"] = Key(32, 2);
        });

        Assert.NotNull(provider.GetRequiredService<IOptions<MongoDbOptions>>().Value);
        Assert.NotNull(provider.GetRequiredService<IOptions<RedisOptions>>().Value);
        Assert.NotNull(provider.GetRequiredService<IOptions<JwtOptions>>().Value);
        Assert.NotNull(provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value);
        var quotas = provider.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
        Assert.All(AllQuotas(quotas), quota => Assert.True(quota is { PermitLimit: > 0, WindowSeconds: > 0 }));
        Assert.DoesNotContain(secret, provider.GetRequiredService<IOptions<JwtOptions>>().Value.Keys.Where(key => key is not null).Select(key => key!.KeyBase64));
    }

    [Theory]
    [InlineData("MongoDb:ConnectionString", "", "MongoDb:")]
    [InlineData("MongoDb:DatabaseName", "enver_other", "MongoDb:")]
    [InlineData("MongoDb:UsersCollectionName", "other", "MongoDb:")]
    [InlineData("MongoDb:OperationTimeoutMilliseconds", "0", "MongoDb:")]
    public void Mongo_options_reject_required_or_incoherent_values(string key, string value, string expected)
    {
        using var provider = CreateProvider(values => values[key] = value);

        var error = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<MongoDbOptions>>().Value);

        Assert.Contains(expected, error.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Redis:ConnectionString", "", "Redis:")]
    [InlineData("Redis:NamespacePrefix", "enver:other", "Redis:")]
    [InlineData("Redis:RevocationPurpose", "other", "Redis:")]
    [InlineData("Redis:OperationTimeoutMilliseconds", "0", "Redis:")]
    [InlineData("Redis:FailClosed", "false", "Redis:")]
    public void Redis_options_reject_required_values_and_wrong_environment_isolation(string key, string value, string expected)
    {
        using var provider = CreateProvider(values => values[key] = value);

        var error = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<RedisOptions>>().Value);

        Assert.Contains(expected, error.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Jwt:Issuer", "other")]
    [InlineData("Jwt:Audience", "other")]
    [InlineData("Jwt:LifetimeSeconds", "899")]
    [InlineData("Jwt:ClockSkewSeconds", "31")]
    [InlineData("Jwt:ActiveKid", "unknown")]
    [InlineData("Jwt:Keys:0:Kid", "")]
    [InlineData("Jwt:Keys:0:KeyBase64", "")]
    public void Jwt_options_reject_invalid_policy_kid_or_key(string key, string value)
    {
        using var provider = CreateProvider(values => values[key] = value);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<JwtOptions>>().Value);
    }

    [Fact]
    public void Jwt_options_reject_duplicate_kids()
    {
        using var provider = CreateProvider(values =>
        {
            values["Jwt:Keys:1:Kid"] = "current";
            values["Jwt:Keys:1:KeyBase64"] = Key(32, 3);
        });

        var error = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<JwtOptions>>().Value);

        Assert.Contains("duplicado", error.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Authentication:InstitutionalDomain", "example.invalid")]
    [InlineData("Authentication:MaxRequestBodyBytes", "16385")]
    [InlineData("Authentication:MaxJsonDepth", "5")]
    [InlineData("Authentication:MaxJwtBytes", "4097")]
    [InlineData("Authentication:RegisterBudgetMilliseconds", "0")]
    public void Authentication_options_reject_incompatible_limits_or_budgets(string key, string value)
    {
        using var provider = CreateProvider(values => values[key] = value);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value);
    }

    [Theory]
    [InlineData("RateLimiting:RegisterEmail:PermitLimit")]
    [InlineData("RateLimiting:RegisterOrigin:PermitLimit")]
    [InlineData("RateLimiting:LoginOriginEmail:PermitLimit")]
    [InlineData("RateLimiting:LoginEmail:PermitLimit")]
    [InlineData("RateLimiting:LoginOrigin:PermitLimit")]
    [InlineData("RateLimiting:LogoutAccount:PermitLimit")]
    [InlineData("RateLimiting:LogoutOrigin:PermitLimit")]
    public void Rate_limiting_rejects_zero_in_each_configured_quota(string key)
    {
        using var provider = CreateProvider(values => values[key] = "0");

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<RateLimitingOptions>>().Value);
    }

    [Theory]
    [InlineData("RateLimiting:PolicyRevision", "")]
    [InlineData("RateLimiting:PseudonymizationKeyBase64", "not-base64")]
    public void Rate_limiting_rejects_invalid_policy_or_hmac_configuration(string key, string value)
    {
        using var provider = CreateProvider(values => values[key] = value);

        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<RateLimitingOptions>>().Value);
    }

    [Fact]
    public void Rate_limiting_rejects_reusing_the_jwt_key_as_hmac_key_without_echoing_it()
    {
        var shared = Key(32, 9);
        using var provider = CreateProvider(values =>
        {
            values["Jwt:Keys:0:KeyBase64"] = shared;
            values["RateLimiting:PseudonymizationKeyBase64"] = shared;
        });

        var error = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<RateLimitingOptions>>().Value);

        Assert.DoesNotContain(shared, error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Missing_required_section_fails_without_reflecting_a_synthetic_secret()
    {
        const string secret = "missing-section-secret";
        using var provider = CreateProvider(values =>
        {
            values.Remove("MongoDb:ConnectionString");
            values["Redis:ConnectionString"] = secret;
        });

        var error = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<MongoDbOptions>>().Value);

        Assert.DoesNotContain(secret, error.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-base64")]
    public void Pseudonymization_key_missing_or_invalid_fails_isolated_startup_without_value(string? value)
    {
        using var provider = CreateProvider(values =>
        {
            // Este caso invoca TODOS los ValidateOnStart, a diferencia de las lecturas Options aisladas previas.
            values["AuthenticationSecurityAuthority:TableName"] = "t053_synthetic";
            values["AuthenticationSecurityAuthority:OperationTimeoutMilliseconds"] = "100";
            values["RateLimiting:PseudonymizationKeyBase64"] = value;
        });
        var error = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());
        if (!string.IsNullOrEmpty(value)) Assert.DoesNotContain(value, error.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(31, false)]
    [InlineData(32, true)]
    [InlineData(64, true)]
    public void Pseudonymization_key_requires_at_least_32_bytes(int length, bool valid)
    {
        using var provider = CreateProvider(values => values["RateLimiting:PseudonymizationKeyBase64"] = Key(length, 7));
        if (valid) Assert.NotNull(provider.GetRequiredService<IOptions<RateLimitingOptions>>().Value);
        else Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<RateLimitingOptions>>().Value);
    }

    [Theory]
    [InlineData("whitespace")]
    [InlineData("missing-padding")]
    [InlineData("base64url")]
    [InlineData("noncanonical-pad-bits")]
    public void Pseudonymization_key_requires_exact_canonical_standard_base64(string variant)
    {
        var canonical = Key(32, 255);
        var value = variant switch
        {
            "whitespace" => " " + canonical + "\n",
            "missing-padding" => canonical.TrimEnd('='),
            "base64url" => canonical.Replace('/', '_'),
            _ => canonical[..^2] + "9="
        };
        using var provider = CreateProvider(values => values["RateLimiting:PseudonymizationKeyBase64"] = value);
        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<RateLimitingOptions>>().Value);
    }

    private static ServiceProvider CreateProvider(Action<Dictionary<string, string?>>? change = null)
    {
        var values = Values();
        change?.Invoke(values);
        var services = new ServiceCollection();
        var environment = new TestEnvironment();
        services.AddSingleton<IHostEnvironment>(environment);
        services.AddValidatedAuthenticationOptions(new ConfigurationBuilder().AddInMemoryCollection(values).Build(), environment);
        return services.BuildServiceProvider(validateScopes: true);
    }

    private static Dictionary<string, string?> Values() => new()
    {
        ["MongoDb:ConnectionString"] = "mongodb://synthetic.invalid",
        ["MongoDb:DatabaseName"] = "enver_testing",
        ["MongoDb:UsersCollectionName"] = "users",
        ["MongoDb:OperationTimeoutMilliseconds"] = "100",
        ["MongoDb:ConnectionTimeoutMilliseconds"] = "100",
        ["MongoDb:WriteConcernCandidate"] = "majority",
        ["MongoDb:ReadConcernCandidate"] = "majority",
        ["MongoDb:ReadPreference"] = "primary",
        ["Redis:ConnectionString"] = "synthetic.invalid:6379",
        ["Redis:NamespacePrefix"] = "enver:testing",
        ["Redis:RevocationPurpose"] = "auth:revocation",
        ["Redis:RateLimitingPurpose"] = "auth:ratelimit",
        ["Redis:OperationTimeoutMilliseconds"] = "100",
        ["Redis:ConnectionTimeoutMilliseconds"] = "100",
        ["Redis:RevocationOperationalMarginSeconds"] = "60",
        ["Redis:FailClosed"] = "true",
        ["Redis:RequiredRevocationEvictionPolicy"] = "noeviction",
        ["Jwt:Algorithm"] = "HS256",
        ["Jwt:Issuer"] = "enver-api",
        ["Jwt:Audience"] = "enver-mobile",
        ["Jwt:LifetimeSeconds"] = "900",
        ["Jwt:ClockSkewSeconds"] = "30",
        ["Jwt:ActiveKid"] = "current",
        ["Jwt:Keys:0:Kid"] = "current",
        ["Jwt:Keys:0:KeyBase64"] = Key(32, 1),
        ["Authentication:InstitutionalDomain"] = "uttt.edu.mx",
        ["Authentication:FullNameMaxUnicodeScalars"] = "200",
        ["Authentication:EmailMaxAsciiBytes"] = "254",
        ["Authentication:EmailLocalPartMaxAsciiBytes"] = "64",
        ["Authentication:PasswordCreationMinUnicodeScalars"] = "12",
        ["Authentication:PasswordMaxUnicodeScalars"] = "128",
        ["Authentication:PasswordMaxUtf8Bytes"] = "512",
        ["Authentication:MaxRequestBodyBytes"] = "16384",
        ["Authentication:MaxJsonDepth"] = "4",
        ["Authentication:MaxJwtBytes"] = "4096",
        ["Authentication:RegisterBudgetMilliseconds"] = "500",
        ["Authentication:LoginBudgetMilliseconds"] = "500",
        ["Authentication:LogoutBudgetMilliseconds"] = "500",
        ["Authentication:HashingConcurrencyLimit"] = "1",
        ["RateLimiting:PolicyRevision"] = "academic-initial",
        ["RateLimiting:PseudonymizationKeyBase64"] = Key(32, 2),
        ["RateLimiting:RegisterEmail:PermitLimit"] = "3",
        ["RateLimiting:RegisterEmail:WindowSeconds"] = "900",
        ["RateLimiting:RegisterOrigin:PermitLimit"] = "20",
        ["RateLimiting:RegisterOrigin:WindowSeconds"] = "900",
        ["RateLimiting:LoginOriginEmail:PermitLimit"] = "5",
        ["RateLimiting:LoginOriginEmail:WindowSeconds"] = "60",
        ["RateLimiting:LoginEmail:PermitLimit"] = "10",
        ["RateLimiting:LoginEmail:WindowSeconds"] = "60",
        ["RateLimiting:LoginOrigin:PermitLimit"] = "60",
        ["RateLimiting:LoginOrigin:WindowSeconds"] = "60",
        ["RateLimiting:LogoutAccount:PermitLimit"] = "30",
        ["RateLimiting:LogoutAccount:WindowSeconds"] = "60",
        ["RateLimiting:LogoutOrigin:PermitLimit"] = "300",
        ["RateLimiting:LogoutOrigin:WindowSeconds"] = "60"
    };

    private static IEnumerable<RateLimitQuotaOptions?> AllQuotas(RateLimitingOptions options) =>
        [options.RegisterEmail, options.RegisterOrigin, options.LoginOriginEmail, options.LoginEmail, options.LoginOrigin, options.LogoutAccount, options.LogoutOrigin];

    private static string Key(int length, byte value) => Convert.ToBase64String(Enumerable.Repeat(value, length).ToArray());

    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = nameof(TestEnvironment);
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
