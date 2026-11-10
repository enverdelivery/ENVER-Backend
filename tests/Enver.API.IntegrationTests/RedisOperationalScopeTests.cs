using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisOperationalScopeTests(
    AuthenticationDependenciesFixture fixture,
    ITestOutputHelper output)
{
    [Fact]
    public async Task Two_independent_hosts_share_real_redis_state_without_sharing_local_security_state()
    {
        await using var scope = await RedisOperationalScope.CreateAsync(fixture);

        Assert.NotSame(scope.A.Services, scope.B.Services);
        Assert.NotSame(scope.A.Options, scope.B.Options);
        Assert.NotSame(scope.A.Connections, scope.B.Connections);
        Assert.NotSame(scope.A.Guard, scope.B.Guard);
        Assert.NotSame(scope.A.Store, scope.B.Store);
        Assert.NotSame(
            await scope.A.Connections.GetMultiplexerAsync(),
            await scope.B.Connections.GetMultiplexerAsync());
        Assert.Equal(fixture.RedisConnectionString, scope.A.Options.ConnectionString);
        Assert.Equal(fixture.RedisConnectionString, scope.B.Options.ConnectionString);
        Assert.Equal(scope.A.Options.NamespacePrefix, scope.B.Options.NamespacePrefix);
        Assert.Equal($"enver:{scope.EnvironmentName}", scope.A.Options.NamespacePrefix);
        Assert.True(await scope.PingAsync());
        Assert.True((await scope.EvaluateGuardAsync(true)).CanContinue);
        Assert.True((await scope.EvaluateGuardAsync(false)).CanContinue);

        output.WriteLine(
            "HOSTS=2 HOST_KIND=LOGICAL_TEST SERVICE_PROVIDERS_DISTINCT=true REDIS_CONNECTIONS_DISTINCT=true " +
            "GUARDS_DISTINCT=true STORES_DISTINCT=true SHARED_REDIS=true SHARED_ENVIRONMENT_NAMESPACE=true " +
            "PHYSICAL_HOSTS=false AWS=false LOAD_BALANCER=false");
    }

    [Fact]
    public async Task Confirmed_revocation_on_host_A_is_observed_by_host_B_without_ttl_reduction_or_local_cache()
    {
        await using var scope = await RedisOperationalScope.CreateAsync(fixture);
        var jti = RedisOperationalScope.NewJti();
        var baseExpiry = DateTimeOffset.UtcNow.AddMinutes(8);

        var beforeWrite = await scope.CheckAsync(false, jti);
        Assert.Equal(TokenRevocationReadStatus.NotRevoked, beforeWrite.Status);

        var write = await scope.RevokeAsync(true, jti, baseExpiry);
        Assert.Equal(TokenRevocationWriteStatus.Confirmed, write.Status);

        var observed = await scope.CheckAsync(false, jti);
        Assert.Equal(TokenRevocationReadStatus.Revoked, observed.Status);

        var expireAfterFirst = await scope.ExpireTimeAsync(jti);
        var ttlAfterFirst = await scope.TimeToLiveAsync(jti);
        Assert.NotNull(expireAfterFirst);
        Assert.NotNull(ttlAfterFirst);
        Assert.True(ttlAfterFirst > TimeSpan.Zero);
        Assert.True(expireAfterFirst.Value >= baseExpiry.UtcDateTime.AddSeconds(-2));
        Assert.Equal("1", await scope.StringValueAsync(jti));

        var lower = await scope.RevokeAsync(true, jti, baseExpiry.AddMinutes(-2));
        Assert.Equal(TokenRevocationWriteStatus.Confirmed, lower.Status);

        var expireAfterLower = await scope.ExpireTimeAsync(jti);
        Assert.NotNull(expireAfterLower);
        Assert.True(expireAfterLower.Value >= expireAfterFirst.Value);

        var observedAfterLower = await scope.CheckAsync(false, jti);
        Assert.Equal(TokenRevocationReadStatus.Revoked, observedAfterLower.Status);

        output.WriteLine(
            "REVOCATION_CROSS_HOST PRE_WRITE_B=NotRevoked CONFIRMED_A=Confirmed OBSERVED_B=Revoked " +
            "TTL_PRESERVED=true TTL_REDUCED=false LOCAL_NEGATIVE_CACHE=false JWT_OR_ENDPOINT=false");
    }

    [Fact]
    public async Task Declared_environment_exposes_noeviction_memory_headroom_and_purpose_environment_isolation()
    {
        await using var scope = await RedisOperationalScope.CreateAsync(fixture);
        var profile = await scope.ReadProfileAsync();
        const long quotaBudgetBytes = 67_108_864;

        Assert.Equal("noeviction", profile.Policy);
        Assert.Equal("noeviction", profile.InfoPolicy);
        Assert.Equal(AuthenticationDependenciesFixture.RedisMaxMemoryBytes, profile.MaxMemoryBytes);
        Assert.Equal(67_108_864L, profile.MaxMemoryBytes);
        Assert.InRange(profile.UsedMemoryBytes, 1, profile.MaxMemoryBytes);
        Assert.InRange(profile.UsedMemoryPeakBytes, profile.UsedMemoryBytes, profile.MaxMemoryBytes);
        Assert.True(profile.HeadroomBytes > 0);

        var environment = scope.EnvironmentName;
        var revocationNamespace = scope.RevocationNamespace;
        var rateLimitingNamespace = scope.RateLimitingNamespace;
        Assert.Equal($"enver:{environment}:auth:revoked", revocationNamespace);
        Assert.Equal($"enver:{environment}:auth:ratelimit", rateLimitingNamespace);
        Assert.Equal("auth:revocation", scope.A.Options.RevocationPurpose);
        Assert.Equal("auth:ratelimit", scope.A.Options.RateLimitingPurpose);
        Assert.StartsWith($"enver:{environment}:", revocationNamespace);
        Assert.StartsWith($"enver:{environment}:", rateLimitingNamespace);
        Assert.NotEqual(revocationNamespace, rateLimitingNamespace);
        Assert.DoesNotContain(":auth:ratelimit", revocationNamespace);
        Assert.DoesNotContain(":auth:revoked", rateLimitingNamespace);

        var jti = RedisOperationalScope.NewJti();
        var revocationKey = scope.RevocationKey(jti);
        Assert.StartsWith(revocationNamespace + ":", revocationKey);
        Assert.False(revocationKey.StartsWith(rateLimitingNamespace + ":", StringComparison.Ordinal));

        var otherEnvironmentKey = RedisKeyBuilder.BuildRevocationKey(environment + "_other", "enver-api", jti);
        Assert.False(otherEnvironmentKey.StartsWith($"enver:{environment}:", StringComparison.Ordinal));
        Assert.NotEqual(revocationKey, otherEnvironmentKey);

        output.WriteLine(
            $"REDIS_PROFILE MAXMEMORY_POLICY={profile.Policy} INFO_MAXMEMORY_POLICY={profile.InfoPolicy} " +
            $"MAXMEMORY_TOTAL_BYTES={profile.MaxMemoryBytes} USED_MEMORY_BYTES={profile.UsedMemoryBytes} " +
            $"USED_MEMORY_PEAK_BYTES={profile.UsedMemoryPeakBytes} HEADROOM_BYTES={profile.HeadroomBytes} " +
            $"QUOTA_BUDGET_BYTES_CITED={quotaBudgetBytes} QUOTA_BUDGET_SCOPE=CUOTAS_ONLY " +
            "MAXMEMORY_SCOPE=FIXTURE_TOTAL REVOCATION_NAMESPACE_PATTERN=enver:{environment}:auth:revoked " +
            "RATE_LIMIT_NAMESPACE_PATTERN=enver:{environment}:auth:ratelimit " +
            "REVOCATION_PURPOSE_CONFIG=auth:revocation " +
            "PURPOSE_SEPARATED=true ENVIRONMENT_SEPARATED=true");
    }
}
