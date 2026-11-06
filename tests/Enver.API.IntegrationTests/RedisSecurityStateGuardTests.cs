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
        using var detector = new RedisSecurityStateDetector(connections, guard);
        using var request = new RequestBudget(1000, CancellationToken.None);

        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Verifiable, decision.Admission);
        Assert.True(decision.CanContinue);
    }

    [Fact]
    public async Task Two_independent_consumers_observe_real_restart_signals_close_admission_and_do_not_reopen()
    {
        await using var firstConnections = new RedisConnections(Options());
        await using var secondConnections = new RedisConnections(Options());
        var first = new RedisSecurityStateGuard(firstConnections, Options());
        var second = new RedisSecurityStateGuard(secondConnections, Options());
        using var firstDetector = new RedisSecurityStateDetector(firstConnections, first);
        using var secondDetector = new RedisSecurityStateDetector(secondConnections, second);
        var firstDatabase = (await firstConnections.GetMultiplexerAsync()).GetDatabase();
        var secondDatabase = (await secondConnections.GetMultiplexerAsync()).GetDatabase();
        var clock = TimeProvider.System;
        var firstFailed = WaitForSignalAsync(firstDetector, RedisConnectionSignal.ConnectionFailed);
        var secondFailed = WaitForSignalAsync(secondDetector, RedisConnectionSignal.ConnectionFailed);
        var t0 = clock.GetTimestamp();

        await dependencies.StopRedisAsync();
        try
        {
            await Task.WhenAll(AssertRedisFailureAsync(firstDatabase), AssertRedisFailureAsync(secondDatabase));
            var failures = await Task.WhenAll(firstFailed, secondFailed);
            using var firstRequest = new RequestBudget(1000, CancellationToken.None);
            using var secondRequest = new RequestBudget(1000, CancellationToken.None);
            var decisions = await Task.WhenAll(first.EvaluateAsync(firstRequest).AsTask(), second.EvaluateAsync(secondRequest).AsTask());
            var t2 = clock.GetTimestamp();

            Assert.All(decisions, decision => Assert.False(decision.CanContinue));
            Assert.All(failures, observation => Assert.True(clock.GetElapsedTime(t0, observation.Timestamp) >= TimeSpan.Zero));
            Assert.All(failures, observation => Assert.True(clock.GetElapsedTime(observation.Timestamp, t2) >= TimeSpan.Zero));
        }
        finally
        {
            var firstRestored = TryWaitForSignalAsync(firstDetector, RedisConnectionSignal.ConnectionRestored);
            var secondRestored = TryWaitForSignalAsync(secondDetector, RedisConnectionSignal.ConnectionRestored);
            await dependencies.StartRedisAsync();
            var restored = await Task.WhenAll(firstRestored, secondRestored);
            if (restored.Any(observation => observation is not null))
            {
                using var restoredRequest = new RequestBudget(1000, CancellationToken.None);
                Assert.False((await first.EvaluateAsync(restoredRequest)).CanContinue);
            }
        }

        using var afterRestart = new RequestBudget(1000, CancellationToken.None);
        var afterRestartDecision = await first.EvaluateAsync(afterRestart);
        Assert.False(afterRestartDecision.CanContinue);
    }

    [Fact]
    public async Task Timeout_during_a_real_dispatched_Redis_operation_is_uncertain_and_closes_admission()
    {
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options(), async budget =>
        {
            var multiplexer = await connections.GetMultiplexerAsync(budget.Token);
            budget.ThrowIfCancellationRequested();
            budget.MarkDispatched();
            await multiplexer.GetDatabase().StringGetAsync(Key()).WaitAsync(budget.Token);
            budget.ThrowIfCancellationRequested();
        });
        using var detector = new RedisSecurityStateDetector(connections, guard);
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
        using var detector = new RedisSecurityStateDetector(connections, guard);
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

    private async Task AssertRedisFailureAsync(IDatabase database)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var failure = await Record.ExceptionAsync(async () => await database.StringGetAsync(Key()).WaitAsync(timeout.Token));
        Assert.NotNull(failure);
    }

    private static async Task<RedisSecurityStateObservation> WaitForSignalAsync(
        RedisSecurityStateDetector detector,
        RedisConnectionSignal signal)
    {
        var completion = new TaskCompletionSource<RedisSecurityStateObservation>(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler<RedisSecurityStateObservation>? handler = null;
        handler = (_, observation) =>
        {
            if (observation.Signal == signal)
            {
                completion.TrySetResult(observation);
            }
        };

        detector.SignalObserved += handler;
        try
        {
            return await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            detector.SignalObserved -= handler;
        }
    }

    private static async Task<RedisSecurityStateObservation?> TryWaitForSignalAsync(
        RedisSecurityStateDetector detector,
        RedisConnectionSignal signal)
    {
        var completion = new TaskCompletionSource<RedisSecurityStateObservation>(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler<RedisSecurityStateObservation>? handler = null;
        handler = (_, observation) =>
        {
            if (observation.Signal == signal)
            {
                completion.TrySetResult(observation);
            }
        };

        detector.SignalObserved += handler;
        try
        {
            return await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (TimeoutException)
        {
            return null;
        }
        finally
        {
            detector.SignalObserved -= handler;
        }
    }

    private string Key() => $"{dependencies.RedisNamespace}:t019:probe";
}
