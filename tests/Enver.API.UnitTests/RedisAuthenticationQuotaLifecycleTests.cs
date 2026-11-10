using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using StackExchange.Redis;

namespace Enver.API.UnitTests.RedisQuotas;

public sealed class RedisAuthenticationQuotaLifecycleTests
{
    [Theory]
    [InlineData("initialize", "initialized")]
    [InlineData("fence", "fenced")]
    [InlineData("maintain", "maintained")]
    [InlineData("activate", "activated")]
    public async Task Lifecycle_uses_explicit_modes_and_generation_without_changing_authority(string mode, string status)
    {
        var calls = 0;
        await using var connections = new RedisConnections(QuotaTestData.Options());
        var lifecycle = new RedisAuthenticationQuotaLifecycle(connections, QuotaTestData.Options(), QuotaTestData.Configuration(), QuotaTestData.Open, QuotaTestData.Open,
            (_, invocation) =>
            {
                calls++;
                Assert.Equal(mode, invocation.Arguments[2].ToString());
                Assert.Equal(2, invocation.Keys.Length);
                return ValueTask.FromResult(QuotaTestData.Reply(status, "ok"));
            });
        using var budget = new RequestBudget(1000, CancellationToken.None);
        var result = mode switch
        {
            "initialize" => await lifecycle.InitializeVerifiedNewEnvironmentAsync(budget),
            "fence" => await lifecycle.FenceAsync("20000000000000000000000000000002", budget),
            "maintain" => await lifecycle.ReconcileBatchAsync([], budget),
            _ => await lifecycle.ActivateAfterDrainAsync(budget)
        };
        Assert.Equal(status, result.Status);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Activation_requires_global_trust_and_does_not_retry()
    {
        var calls = 0;
        await using var connections = new RedisConnections(QuotaTestData.Options());
        var lifecycle = new RedisAuthenticationQuotaLifecycle(connections, QuotaTestData.Options(), QuotaTestData.Configuration(), QuotaTestData.Open,
            new QuotaTestData.State(Enver.API.Features.Authentication.Application.Ports.AuthenticationSecurityAdmission.Unavailable),
            (_, _) => { calls++; throw new InvalidOperationException(); });
        using var budget = new RequestBudget(1000, CancellationToken.None);
        Assert.Equal("unavailable", (await lifecycle.ActivateAfterDrainAsync(budget)).Status);
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Same_generation_cannot_fence_and_batches_are_bounded()
    {
        var config = QuotaTestData.Configuration();
        Assert.Throws<ArgumentException>(() => RedisAuthenticationQuotaScriptProtocol.Encode(config, "fence", nextGeneration: config.Generation));
        var candidates = Enumerable.Range(1, 4).Select(i => $"login:email:{QuotaTestData.Opaque(i)}");
        Assert.Throws<ArgumentException>(() => RedisAuthenticationQuotaScriptProtocol.Encode(config, "maintain", candidates: candidates));
    }
}
