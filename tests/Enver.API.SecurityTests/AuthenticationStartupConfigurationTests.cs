using System.Net;
using Enver.Testing;

namespace Enver.API.SecurityTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class AuthenticationStartupConfigurationTests(AuthenticationDependenciesFixture dependencies)
{
    [Fact]
    public async Task Valid_synthetic_configuration_runs_ValidateOnStart_and_starts_a_test_host()
    {
        await using var factory = new AuthenticationApiFactory(dependencies);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(dependencies.ContainsSyntheticSecret(body));
    }

    [Theory]
    [InlineData("MongoDb:DatabaseName", "other_environment_database", "MongoDb:")]
    [InlineData("Redis:NamespacePrefix", "other:namespace", "Redis:")]
    [InlineData("Jwt:Algorithm", "none", "Jwt:")]
    [InlineData("Authentication:InstitutionalDomain", "example.invalid", "Authentication:")]
    [InlineData("RateLimiting:PolicyRevision", "", "RateLimiting:")]
    public void Invalid_synthetic_configuration_is_rejected_during_test_host_startup(string key, string value, string expectedValidator)
    {
        var overrides = new Dictionary<string, string?> { [key] = value };
        using var factory = new AuthenticationApiFactory(dependencies, overrides);

        var exception = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        var details = exception.ToString();

        Assert.Contains(expectedValidator, details, StringComparison.Ordinal);
        Assert.False(dependencies.ContainsSyntheticSecret(details));
    }
}
