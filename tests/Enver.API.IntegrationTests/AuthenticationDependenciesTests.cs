using Enver.Testing;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class AuthenticationDependenciesTests(AuthenticationDependenciesFixture dependencies)
{
    [Fact]
    public async Task Mongo_and_Redis_dependencies_are_real_isolated_and_cleaned_without_global_deletion()
    {
        await dependencies.VerifyMongoAsync();
        await dependencies.VerifyRedisAsync();

        Assert.Equal("rs0", AuthenticationDependenciesFixture.ReplicaSetName);
        Assert.StartsWith("enver_t015_", dependencies.MongoDatabase, StringComparison.Ordinal);
        Assert.StartsWith("enver:t015_", dependencies.RedisNamespace, StringComparison.Ordinal);
        Assert.Equal(64L * 1024 * 1024, AuthenticationDependenciesFixture.RedisMaxMemoryBytes);

        await dependencies.CleanupOwnDataAsync();
        await dependencies.AssertOwnDataRemovedAsync();
    }
}
