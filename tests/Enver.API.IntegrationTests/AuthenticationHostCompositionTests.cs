using System.Net;
using Enver.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class AuthenticationHostCompositionTests(AuthenticationDependenciesFixture dependencies)
{
    [Fact]
    public async Task Two_independent_hosts_share_the_same_test_dependencies_without_sharing_local_state()
    {
        await using var hostA = new AuthenticationApiFactory(dependencies);
        await using var hostB = new AuthenticationApiFactory(dependencies);
        using var clientA = hostA.CreateClient();
        using var clientB = hostB.CreateClient();

        var healthA = await clientA.GetAsync("/health");
        var healthB = await clientB.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, healthA.StatusCode);
        Assert.Equal(HttpStatusCode.OK, healthB.StatusCode);
        Assert.NotSame(hostA.Server, hostB.Server);
        Assert.NotSame(hostA.Services, hostB.Services);
        Assert.Equal(dependencies.EnvironmentName, hostA.Services.GetRequiredService<IHostEnvironment>().EnvironmentName);
        Assert.Equal(dependencies.EnvironmentName, hostB.Services.GetRequiredService<IHostEnvironment>().EnvironmentName);

        var stateA = hostA.Services.GetRequiredService<TestHostLocalState>();
        var stateB = hostB.Services.GetRequiredService<TestHostLocalState>();
        Assert.NotSame(stateA, stateB);
        Assert.NotEqual(stateA.InstanceId, stateB.InstanceId);
        stateA.Values["host-only"] = "A";
        Assert.False(stateB.Values.ContainsKey("host-only"));

        var dependenciesA = hostA.Services.GetRequiredService<TestHostExternalDependencyProbe>();
        var dependenciesB = hostB.Services.GetRequiredService<TestHostExternalDependencyProbe>();
        Assert.NotSame(dependenciesA, dependenciesB);
        Assert.Same(dependencies, hostA.Dependencies);
        Assert.Same(dependencies, hostB.Dependencies);
        Assert.Equal(hostA.Dependencies.MongoDatabase, hostB.Dependencies.MongoDatabase);
        Assert.Equal(hostA.Dependencies.RedisNamespace, hostB.Dependencies.RedisNamespace);

        await dependenciesA.WriteMongoAsync("host-a");
        await dependenciesA.WriteRedisAsync("host-a");
        Assert.True(await dependenciesB.ContainsMongoAsync("host-a"));
        Assert.True(await dependenciesB.ContainsRedisAsync("host-a"));

        await dependencies.CleanupOwnDataAsync();
        await dependencies.AssertOwnDataRemovedAsync();
    }
}
