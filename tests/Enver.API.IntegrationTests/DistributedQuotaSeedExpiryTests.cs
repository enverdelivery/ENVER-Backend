using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class DistributedQuotaSeedExpiryTests(AuthenticationDependenciesFixture fixture)
{
    [Fact]
    public async Task Expiry_observer_records_applied_values_and_direct_server_readings_without_identifiers()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var request = s.Request();
        var diagnostic = new DistributedQuotaSeedExpiryDiagnostics();
        var now = await s.NowAsync();
        foreach (var scope in request.Scopes)
        {
            diagnostic.Begin(scope.ScopeType.ToString(), now);
            await s.SeedAsync(request, scope, [now], diagnostic);
        }
        var origin = request.Scopes.Single(item => item.ScopeType == AuthenticationQuotaScopeType.Origin);
        await diagnostic.BeforeEvalAsync(s.Database, s.Configuration.Keys.Registry, s.Key(request, origin));
        var evidence = diagnostic.Evidence();
        Assert.Equal(4, evidence.GetProperty("assignments").GetArrayLength());
        foreach (var assignment in evidence.GetProperty("assignments").EnumerateArray())
        {
            Assert.Equal(1, assignment.GetProperty("Result").GetInt64());
            Assert.True(assignment.GetProperty("RedisAfterUs").GetInt64() >= assignment.GetProperty("RedisBeforeUs").GetInt64());
        }
        Assert.Equal(2, evidence.GetProperty("afterSeed").GetArrayLength());
        foreach (var sample in evidence.GetProperty("afterSeed").EnumerateArray())
        {
            Assert.True(sample.GetProperty("RegistryPttl").GetInt64() > 0);
            Assert.True(sample.GetProperty("BucketPttl").GetInt64() > 0);
            var scope = request.Scopes.Single(item => item.ScopeType.ToString() == sample.GetProperty("Scope").GetString());
            var expiry = (now + RedisAuthenticationQuotaConfiguration.WindowMicroseconds(scope.Definition.Window) + 999) / 1000;
            Assert.Equal(expiry, sample.GetProperty("BucketPExpireTime").GetInt64());
        }
        var preEval = evidence.GetProperty("preEval");
        Assert.True(preEval.GetProperty("RegistryPttl").GetInt64() > 0);
        Assert.True(preEval.GetProperty("BucketPttl").GetInt64() > 0);
        var text = evidence.GetRawText();
        Assert.True(text.Length < 3200);
        Assert.DoesNotContain(s.Configuration.Keys.Registry.ToString(), text);
        Assert.DoesNotContain(s.Key(request, origin), text);
    }
}
