using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisSecurityStateGuardTests(AuthenticationDependenciesFixture dependencies)
{
    [Fact]
    public async Task Available_Redis_allows_a_bounded_security_state_check()
    {
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options());
        using var request = new RequestBudget(1000, CancellationToken.None);

        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Verifiable, decision.Admission);
        Assert.True(decision.CanContinue);
    }

    [Fact]
    public async Task Two_independent_consumers_fail_closed_and_do_not_reopen_after_a_controlled_outage()
    {
        await using var firstConnections = new RedisConnections(Options());
        await using var secondConnections = new RedisConnections(Options());
        var first = new RedisSecurityStateGuard(firstConnections, Options());
        var second = new RedisSecurityStateGuard(secondConnections, Options());

        await dependencies.StopRedisAsync();
        try
        {
            using var firstRequest = new RequestBudget(1000, CancellationToken.None);
            using var secondRequest = new RequestBudget(1000, CancellationToken.None);
            var decisions = await Task.WhenAll(first.EvaluateAsync(firstRequest).AsTask(), second.EvaluateAsync(secondRequest).AsTask());

            Assert.All(decisions, decision => Assert.False(decision.CanContinue));
        }
        finally
        {
            await dependencies.StartRedisAsync();
        }

        using var afterRestart = new RequestBudget(1000, CancellationToken.None);
        var afterRestartDecision = await first.EvaluateAsync(afterRestart);
        Assert.False(afterRestartDecision.CanContinue);
    }

    [Fact]
    public async Task Timeout_during_a_real_dispatched_Redis_operation_is_uncertain_and_closes_admission()
    {
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options());
        _ = await connections.GetMultiplexerAsync();
        var controlOptions = ConfigurationOptions.Parse(dependencies.RedisConnectionString);
        controlOptions.AllowAdmin = true;
        using var control = await ConnectionMultiplexer.ConnectAsync(controlOptions);
        var pause = await control.GetDatabase().ExecuteAsync("CLIENT", "PAUSE", "1000", "ALL");
        Assert.Equal("OK", pause.ToString());
        using var request = new RequestBudget(1000, CancellationToken.None);

        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Uncertain, decision.Admission);
        Assert.True(decision.ResultUncertain);

        using var nextRequest = new RequestBudget(1000, CancellationToken.None);
        Assert.False((await guard.EvaluateAsync(nextRequest)).CanContinue);
    }

    [Fact]
    public async Task Cancelled_caller_does_not_dispose_redis_connections_or_bypass_admission()
    {
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options());
        var multiplexer = await connections.GetMultiplexerAsync();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var request = new RequestBudget(1000, cancellation.Token);

        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, decision.Admission);
        Assert.Equal(BudgetCancellationCause.Client, decision.CancellationCause);
        Assert.Same(multiplexer, await connections.GetMultiplexerAsync());
    }

    private RedisOptions Options() => new()
    {
        ConnectionString = dependencies.RedisConnectionString,
        ConnectionTimeoutMilliseconds = 250,
        OperationTimeoutMilliseconds = 250
    };
}
