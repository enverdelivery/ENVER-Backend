using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;

namespace Enver.API.UnitTests.RedisQuotas;

public sealed class RedisAuthenticationQuotaKeyBuilderTests
{
    [Fact]
    public void Keys_are_deterministic_separated_and_share_one_hash_tag()
    {
        var keys = new RedisAuthenticationQuotaKeyBuilder("testing");
        var bucket = keys.Bucket(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Email, QuotaTestData.Opaque(1));
        Assert.Equal($"enver:{{testing:auth:ratelimit}}:bucket:register:email:{QuotaTestData.Opaque(1)}", bucket);
        foreach (var key in new[] { bucket, keys.Registry, keys.Metadata }) Assert.Contains("{testing:auth:ratelimit}", key);
        Assert.Equal(bucket, keys.CandidateBucket($"register:email:{QuotaTestData.Opaque(1)}"));
        Assert.DoesNotContain("academic", bucket);
        Assert.DoesNotContain("@", bucket);
        Assert.DoesNotContain("203.0.113.1", bucket);
        Assert.NotEqual(bucket, new RedisAuthenticationQuotaKeyBuilder("other").Bucket(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Email, QuotaTestData.Opaque(1)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Testing")]
    [InlineData("test:other")]
    [InlineData("test{other}")]
    [InlineData("test\n")]
    public void Invalid_environments_cannot_inject_slots(string value) => Assert.Throws<ArgumentException>(() => new RedisAuthenticationQuotaKeyBuilder(value));

    [Theory]
    [InlineData("raw@uttt.edu.mx")]
    [InlineData("203.0.113.1")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    public void Invalid_opaque_ids_are_rejected(string value)
    {
        Assert.False(RedisAuthenticationQuotaKeyBuilder.IsOpaqueId(value));
        Assert.Throws<ArgumentException>(() => new RedisAuthenticationQuotaKeyBuilder("testing").Bucket(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Email, value));
    }

    [Theory]
    [InlineData("00000000000000000000000000000000")]
    [InlineData("ABCDEF0123456789ABCDEF0123456789")]
    [InlineData("00000000-0000-0000-0000-000000000001")]
    public void Invalid_attempts_are_rejected(string value) => Assert.False(RedisAuthenticationQuotaKeyBuilder.IsAttemptId(value));

    [Fact]
    public void Duplicate_or_incomplete_scopes_are_not_representable()
    {
        var scope = QuotaTestData.Request().Scopes[0];
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeRequest(AuthenticationOperation.Register, Guid.NewGuid().ToString("N"), "academic-initial", [scope]));
        Assert.Throws<ArgumentException>(() => new AuthenticationQuotaConsumeRequest(AuthenticationOperation.Register, Guid.NewGuid().ToString("N"), "academic-initial", [scope, scope]));
    }
}
