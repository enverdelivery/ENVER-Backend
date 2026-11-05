using Enver.API.Http;

namespace Enver.API.UnitTests;

public sealed class RequestBudgetTests
{
    [Fact]
    public void Remaining_budget_is_monotonic_and_never_negative_across_phases()
    {
        var time = new ManualTimeProvider();
        using var budget = new RequestBudget(1000, CancellationToken.None, time);

        Assert.Equal(TimeSpan.FromMilliseconds(1000), budget.Remaining);
        time.Advance(TimeSpan.FromMilliseconds(250));
        Assert.Equal(TimeSpan.FromMilliseconds(750), budget.Remaining);

        using var dependency = budget.BeginDependency(900);
        Assert.Equal(TimeSpan.FromMilliseconds(750), dependency.Remaining);
        time.Advance(TimeSpan.FromMilliseconds(500));
        Assert.Equal(TimeSpan.FromMilliseconds(250), budget.Remaining);
        Assert.Equal(TimeSpan.FromMilliseconds(250), dependency.Remaining);

        time.Advance(TimeSpan.FromMilliseconds(1000));
        Assert.Equal(TimeSpan.Zero, budget.Remaining);
        Assert.Equal(TimeSpan.Zero, dependency.Remaining);
    }

    [Fact]
    public void Exhausted_request_budget_is_deterministic_without_sleeping()
    {
        var time = new ManualTimeProvider();
        using var budget = new RequestBudget(100, CancellationToken.None, time);
        time.Advance(TimeSpan.FromMilliseconds(100));

        var error = Assert.Throws<BudgetCanceledException>(budget.ThrowIfCancellationRequested);

        Assert.Equal(BudgetCancellationCause.RequestBudget, error.Cause);
        Assert.False(error.ResultUncertain);
        Assert.Equal(TimeSpan.Zero, budget.Remaining);
        Assert.True(budget.Token.IsCancellationRequested);
    }

    [Fact]
    public void Dependency_budget_clamps_to_remaining_request_budget()
    {
        var time = new ManualTimeProvider();
        using var request = new RequestBudget(500, CancellationToken.None, time);
        time.Advance(TimeSpan.FromMilliseconds(350));
        using var dependency = request.BeginDependency(1000);

        Assert.Equal(TimeSpan.FromMilliseconds(150), dependency.Remaining);
    }

    [Fact]
    public void Dependency_timeout_preserves_uncertainty_after_dispatch()
    {
        var time = new ManualTimeProvider();
        using var request = new RequestBudget(1000, CancellationToken.None, time);
        using var dependency = request.BeginDependency(250);
        dependency.MarkDispatched();
        time.Advance(TimeSpan.FromMilliseconds(250));

        var error = Assert.Throws<BudgetCanceledException>(dependency.ThrowIfCancellationRequested);

        Assert.Equal(BudgetCancellationCause.DependencyTimeout, error.Cause);
        Assert.True(error.ResultUncertain);
        Assert.Equal(TimeSpan.Zero, dependency.Remaining);
    }

    [Fact]
    public void Client_cancellation_has_priority_over_simultaneous_budget_signals()
    {
        var time = new ManualTimeProvider();
        using var client = new CancellationTokenSource();
        using var request = new RequestBudget(100, client.Token, time);
        using var dependency = request.BeginDependency(100);
        dependency.MarkDispatched();
        time.Advance(TimeSpan.FromMilliseconds(100));
        client.Cancel();

        var error = Assert.Throws<BudgetCanceledException>(dependency.ThrowIfCancellationRequested);

        Assert.Equal(BudgetCancellationCause.Client, error.Cause);
        Assert.True(error.ResultUncertain);
        Assert.True(request.Token.IsCancellationRequested);
        Assert.True(dependency.Token.IsCancellationRequested);
    }

    [Fact]
    public void Client_cancellation_before_dispatch_is_linked_but_not_uncertain()
    {
        var time = new ManualTimeProvider();
        using var client = new CancellationTokenSource();
        using var request = new RequestBudget(500, client.Token, time);
        using var dependency = request.BeginDependency(250);
        client.Cancel();

        var error = Assert.Throws<BudgetCanceledException>(dependency.ThrowIfCancellationRequested);

        Assert.Equal(BudgetCancellationCause.Client, error.Cause);
        Assert.False(error.ResultUncertain);
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private long timestamp;

        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => timestamp;
        public void Advance(TimeSpan duration) => timestamp += duration.Ticks;
    }
}
