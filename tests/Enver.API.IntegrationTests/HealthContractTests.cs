using System.Net;
using Enver.Testing;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class HealthContractTests(AuthenticationDependenciesFixture fixture)
{
    [Fact]
    public async Task Health_has_exact_plaintext_contract()
    {
        await using var factory = new AuthenticationApiFactory(fixture);
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
    }
}
