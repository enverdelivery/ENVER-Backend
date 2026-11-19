using System.Text;
using Enver.API.Configuration;
using Enver.API.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class AuthenticationOptionsAndBudgetTests
{
    [Fact]
    public void Valid_synthetic_options_accepts_kids_keys_and_all_seven_quotas()
    {
        using var provider = CreateOptionsProvider();

        var jwt = provider.GetRequiredService<IOptions<JwtOptions>>().Value;
        var quotas = provider.GetRequiredService<IOptions<RateLimitingOptions>>().Value;

        Assert.Equal("current", jwt.ActiveKid);
        Assert.Single(jwt.Keys);
        Assert.All(AllQuotas(quotas), quota => Assert.True(quota is { PermitLimit: > 0, WindowSeconds: > 0 }));
    }

    [Fact]
    public void Duplicate_kid_is_rejected_by_prepared_validation()
    {
        using var provider = CreateOptionsProvider(values =>
        {
            values["Jwt:Keys:1:Kid"] = "current";
            values["Jwt:Keys:1:KeyBase64"] = Key(32, 2);
        });

        var error = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<JwtOptions>>().Value);

        Assert.Contains(error.Failures, failure => failure.Contains("duplicado", StringComparison.Ordinal));
    }

    [Fact]
    public void Client_cancellation_is_linked_to_the_request_budget_without_sleeping()
    {
        using var client = new CancellationTokenSource();
        using var budget = new RequestBudget(1000, client.Token, TimeProvider.System);
        client.Cancel();

        var error = Assert.Throws<BudgetCanceledException>(budget.ThrowIfCancellationRequested);

        Assert.Equal(BudgetCancellationCause.Client, error.Cause);
        Assert.False(error.ResultUncertain);
    }

    [Fact]
    public void Dispatched_dependency_preserves_uncertainty_when_client_cancels()
    {
        using var client = new CancellationTokenSource();
        using var budget = new RequestBudget(1000, client.Token, TimeProvider.System);
        using var dependency = budget.BeginDependency(500);
        dependency.MarkDispatched();
        client.Cancel();

        var error = Assert.Throws<BudgetCanceledException>(dependency.ThrowIfCancellationRequested);

        Assert.Equal(BudgetCancellationCause.Client, error.Cause);
        Assert.True(error.ResultUncertain);
    }

    private static ServiceProvider CreateOptionsProvider(Action<Dictionary<string, string?>>? change = null)
    {
        var values = ValidValues();
        change?.Invoke(values);
        var services = new ServiceCollection();
        var environment = new TestHostEnvironment();
        services.AddSingleton<IHostEnvironment>(environment);
        services.AddValidatedAuthenticationOptions(new ConfigurationBuilder().AddInMemoryCollection(values).Build(), environment);
        return services.BuildServiceProvider(validateScopes: true);
    }

    private static Dictionary<string, string?> ValidValues() => new()
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
        ["RateLimiting:PseudonymizationKeyBase64"] = Key(32, 3),
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

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = nameof(TestHostEnvironment);
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
