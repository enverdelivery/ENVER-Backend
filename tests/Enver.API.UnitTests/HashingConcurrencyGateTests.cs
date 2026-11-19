using Enver.API.Http;
using Enver.API.Security;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class HashingConcurrencyGateTests
{
    [Fact]
    public void Immediate_admission_saturation_and_idempotent_release()
    {
        var options = PasswordPolicyTests.PolicyOptions(); options.HashingConcurrencyLimit = 2;
        var gate = new HashingConcurrencyGate(Options.Create(options));
        using var budget = new RequestBudget(8000, default);
        var first = gate.TryAcquire(budget); var second = gate.TryAcquire(budget);
        Assert.Equal(2, gate.Limit);
        Assert.NotNull(first); Assert.NotNull(second);
        Assert.Null(gate.TryAcquire(budget));
        first.Dispose(); first.Dispose();
        using var third = gate.TryAcquire(budget); Assert.NotNull(third);
        second.Dispose();
    }

    [Fact]
    public void Canceled_budget_never_acquires()
    {
        var options = PasswordPolicyTests.PolicyOptions(); options.HashingConcurrencyLimit = 1;
        var gate = new HashingConcurrencyGate(Options.Create(options));
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        using var canceled = new RequestBudget(8000, cancellation.Token);
        Assert.Null(gate.TryAcquire(canceled));
        using var active = new RequestBudget(8000, default);
        using var lease = gate.TryAcquire(active); Assert.NotNull(lease);
    }
}
