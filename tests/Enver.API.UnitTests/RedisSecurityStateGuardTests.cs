using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;

namespace Enver.API.UnitTests;

public sealed class RedisSecurityStateGuardTests
{
    [Fact]
    public async Task Verifiable_state_allows_the_caller_to_continue()
    {
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options(), _ => ValueTask.CompletedTask);
        using var request = new RequestBudget(1000, CancellationToken.None);

        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Verifiable, decision.Admission);
        Assert.True(decision.CanContinue);
    }

    [Fact]
    public async Task Unverifiable_Redis_denies_without_a_fallback()
    {
        var calls = 0;
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options(), _ =>
        {
            calls++;
            throw new InvalidOperationException("synthetic Redis failure");
        });
        using var first = new RequestBudget(1000, CancellationToken.None);
        using var second = new RequestBudget(1000, CancellationToken.None);

        var firstDecision = await guard.EvaluateAsync(first);
        var secondDecision = await guard.EvaluateAsync(second);

        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, firstDecision.Admission);
        Assert.False(firstDecision.CanContinue);
        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, secondDecision.Admission);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Connection_failed_signal_closes_admission()
    {
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options(), _ => ValueTask.CompletedTask);
        guard.ObserveConnectivitySignal(RedisConnectionSignal.ConnectionFailed);
        using var request = new RequestBudget(1000, CancellationToken.None);

        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, decision.Admission);
        Assert.False(decision.CanContinue);
    }

    [Fact]
    public async Task Connection_restored_does_not_reopen_admission_or_treat_connectivity_as_trust()
    {
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options(), _ => ValueTask.CompletedTask);
        guard.ObserveConnectivitySignal(RedisConnectionSignal.ConnectionFailed);
        guard.ObserveConnectivitySignal(RedisConnectionSignal.ConnectionRestored);
        using var request = new RequestBudget(1000, CancellationToken.None);

        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, decision.Admission);
    }

    [Fact]
    public async Task Timeout_before_dispatch_denies_without_uncertainty()
    {
        var time = new ManualTimeProvider();
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options(), budget =>
        {
            time.Advance(TimeSpan.FromMilliseconds(100));
            budget.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        });
        using var request = new RequestBudget(1000, CancellationToken.None, time);

        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, decision.Admission);
        Assert.Equal(BudgetCancellationCause.DependencyTimeout, decision.CancellationCause);
        Assert.False(decision.ResultUncertain);
    }

    [Fact]
    public async Task Timeout_after_dispatch_denies_with_uncertainty()
    {
        var time = new ManualTimeProvider();
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options(), budget =>
        {
            budget.MarkDispatched();
            time.Advance(TimeSpan.FromMilliseconds(100));
            budget.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        });
        using var request = new RequestBudget(1000, CancellationToken.None, time);

        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Uncertain, decision.Admission);
        Assert.Equal(BudgetCancellationCause.DependencyTimeout, decision.CancellationCause);
        Assert.True(decision.ResultUncertain);
    }

    [Fact]
    public async Task Client_cancellation_denies_without_retry_or_bypass()
    {
        var calls = 0;
        using var client = new CancellationTokenSource();
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options(), budget =>
        {
            calls++;
            client.Cancel();
            budget.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        });
        using var request = new RequestBudget(1000, client.Token);

        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, decision.Admission);
        Assert.Equal(BudgetCancellationCause.Client, decision.CancellationCause);
        Assert.False(decision.CanContinue);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Exhausted_request_budget_denies_without_dispatch()
    {
        var time = new ManualTimeProvider();
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options(), budget =>
        {
            time.Advance(TimeSpan.FromMilliseconds(100));
            budget.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        });
        using var request = new RequestBudget(50, CancellationToken.None, time);

        var decision = await guard.EvaluateAsync(request);

        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, decision.Admission);
        Assert.Equal(BudgetCancellationCause.RequestBudget, decision.CancellationCause);
        Assert.False(decision.ResultUncertain);
    }

    [Fact]
    public async Task Dependency_budget_is_limited_to_the_approved_operation_timeout()
    {
        TimeSpan remaining = TimeSpan.MaxValue;
        await using var connections = new RedisConnections(Options());
        var guard = new RedisSecurityStateGuard(connections, Options(), budget =>
        {
            remaining = budget.Remaining;
            return ValueTask.CompletedTask;
        });
        using var request = new RequestBudget(50, CancellationToken.None);

        var decision = await guard.EvaluateAsync(request);

        Assert.True(remaining <= TimeSpan.FromMilliseconds(50));
        Assert.True(decision.CanContinue);
    }

    private static RedisOptions Options() => new()
    {
        ConnectionString = "127.0.0.1:1",
        ConnectionTimeoutMilliseconds = 100,
        OperationTimeoutMilliseconds = 50
    };

    private sealed class ManualTimeProvider : TimeProvider
    {
        private long timestamp;

        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => timestamp;
        public void Advance(TimeSpan duration) => timestamp += duration.Ticks;
    }
}
