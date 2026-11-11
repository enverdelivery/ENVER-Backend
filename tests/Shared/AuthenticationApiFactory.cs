using Enver.API.Configuration;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Enver.Testing;

public sealed class AuthenticationApiFactory : WebApplicationFactory<AuthenticationOptions>
{
    private readonly AuthenticationDependenciesFixture _dependencies;
    private readonly IReadOnlyDictionary<string, string?> _configuration;

    public AuthenticationApiFactory(AuthenticationDependenciesFixture dependencies, IReadOnlyDictionary<string, string?>? overrides = null)
    {
        _dependencies = dependencies;
        // Wiring sintético de 057; fixture/imágenes/topología intactas, sin recursos AWS reales.
        var values = new Dictionary<string, string?>
        {
            ["AuthenticationComposition:AuthorityRegion"] = "us-east-1",
            ["AuthenticationComposition:QuotaConfigurationVersion"] = "config-021",
            ["AuthenticationComposition:QuotaIdentityReference"] = "identity-053",
            ["AuthenticationComposition:QuotaGeneration"] = dependencies.RunId,
            ["RateLimiting:MaxActivePartitions"] = "5000",
            ["AuthenticationTransport:DirectConnectionApproved"] = "true",
            ["AuthenticationTransport:AllowLocalHttp"] = "true"
        };
        if (overrides is not null) foreach (var pair in overrides) values[pair.Key] = pair.Value;
        _configuration = dependencies.CreateConfiguration(values);
    }

    public AuthenticationDependenciesFixture Dependencies => _dependencies;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_dependencies.EnvironmentName);
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(_configuration));
        builder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IAmazonDynamoDB>(_ => _dependencies.CreateDynamoDbClient());
            services.AddSingleton<TestHostLocalState>();
            services.AddSingleton(new TestHostExternalDependencyProbe(_dependencies));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(_dependencies.EnvironmentName);
        return base.CreateHost(builder);
    }
}

public sealed class TestHostLocalState
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    public Dictionary<string, string> Values { get; } = new(StringComparer.Ordinal);
}

public sealed class TestHostExternalDependencyProbe(AuthenticationDependenciesFixture dependencies)
{
    public Task WriteMongoAsync(string marker, CancellationToken cancellationToken = default) => dependencies.WriteMongoAsync(marker, cancellationToken);
    public Task<bool> ContainsMongoAsync(string marker, CancellationToken cancellationToken = default) => dependencies.ContainsMongoAsync(marker, cancellationToken);
    public Task WriteRedisAsync(string marker, CancellationToken cancellationToken = default) => dependencies.WriteRedisAsync(marker, cancellationToken);
    public Task<bool> ContainsRedisAsync(string marker, CancellationToken cancellationToken = default) => dependencies.ContainsRedisAsync(marker, cancellationToken);
}
