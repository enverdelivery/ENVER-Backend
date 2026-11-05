using Enver.API.Configuration;

namespace Enver.API.Http;

public enum AuthenticationOperation { Register, Login, Logout }
public enum BudgetCancellationCause { None, Client, RequestBudget, DependencyTimeout }

/// <summary>Cancelar espera no prueba ausencia de efecto remoto.</summary>
public sealed class BudgetCanceledException(BudgetCancellationCause cause, bool resultUncertain, CancellationToken token)
    : OperationCanceledException("Se canceló el trabajo dentro del presupuesto autorizado.", token)
{
    public BudgetCancellationCause Cause { get; } = cause;
    public bool ResultUncertain { get; } = resultUncertain;
}

public sealed class RequestBudget : IDisposable
{
    private readonly TimeProvider clock;
    private readonly long started;
    private readonly TimeSpan total;
    private readonly CancellationToken client;
    private readonly CancellationTokenSource deadline;
    private readonly CancellationTokenSource linked;

    public RequestBudget(int milliseconds, CancellationToken client, TimeProvider? clock = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(milliseconds);
        this.clock = clock ?? TimeProvider.System;
        this.client = client;
        total = TimeSpan.FromMilliseconds(milliseconds);
        started = this.clock.GetTimestamp();
        deadline = new CancellationTokenSource(total, this.clock);
        linked = CancellationTokenSource.CreateLinkedTokenSource(client, deadline.Token);
    }

    public static RequestBudget Create(AuthenticationOperation operation, AuthenticationOptions options, CancellationToken client, TimeProvider? clock = null) =>
        new(operation switch
        {
            AuthenticationOperation.Register => options.RegisterBudgetMilliseconds,
            AuthenticationOperation.Login => options.LoginBudgetMilliseconds,
            AuthenticationOperation.Logout => options.LogoutBudgetMilliseconds,
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        }, client, clock);

    internal static TimeSpan Minimum(TimeSpan first, TimeSpan second) => first <= second ? first : second;
    internal static TimeSpan NonNegative(TimeSpan value) => value > TimeSpan.Zero ? value : TimeSpan.Zero;
    public TimeSpan Remaining => NonNegative(total - clock.GetElapsedTime(started));
    public BudgetCancellationCause Cause => client.IsCancellationRequested ? BudgetCancellationCause.Client
        : deadline.IsCancellationRequested || Remaining == TimeSpan.Zero ? BudgetCancellationCause.RequestBudget
        : BudgetCancellationCause.None;
    public CancellationToken Token { get { if (Remaining == TimeSpan.Zero) deadline.Cancel(); return linked.Token; } }
    public void ThrowIfCancellationRequested()
    {
        if (Cause != BudgetCancellationCause.None) throw new BudgetCanceledException(Cause, false, Token);
    }
    public DependencyBudget BeginDependency(int timeoutMilliseconds) => new(this, timeoutMilliseconds, clock);
    public void Dispose() { linked.Dispose(); deadline.Dispose(); }
}

public sealed class DependencyBudget : IDisposable
{
    private readonly RequestBudget request;
    private readonly TimeProvider clock;
    private readonly long started;
    private readonly TimeSpan total;
    private readonly CancellationTokenSource deadline;
    private readonly CancellationTokenSource linked;
    private bool dispatched;

    internal DependencyBudget(RequestBudget request, int timeoutMilliseconds, TimeProvider clock)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeoutMilliseconds);
        this.request = request;
        this.clock = clock;
        started = clock.GetTimestamp();
        total = RequestBudget.Minimum(request.Remaining, TimeSpan.FromMilliseconds(timeoutMilliseconds));
        deadline = new CancellationTokenSource(total, clock);
        linked = CancellationTokenSource.CreateLinkedTokenSource(request.Token, deadline.Token);
    }

    public TimeSpan Remaining => RequestBudget.Minimum(request.Remaining, RequestBudget.NonNegative(total - clock.GetElapsedTime(started)));
    public BudgetCancellationCause Cause => request.Cause != BudgetCancellationCause.None ? request.Cause
        : deadline.IsCancellationRequested || Remaining == TimeSpan.Zero ? BudgetCancellationCause.DependencyTimeout
        : BudgetCancellationCause.None;
    public bool ResultUncertain => dispatched && Cause != BudgetCancellationCause.None;
    public void MarkDispatched() => dispatched = true;
    public CancellationToken Token { get { if (Remaining == TimeSpan.Zero) deadline.Cancel(); return linked.Token; } }
    public void ThrowIfCancellationRequested()
    {
        if (Cause != BudgetCancellationCause.None) throw new BudgetCanceledException(Cause, ResultUncertain, Token);
    }
    public void Dispose() { linked.Dispose(); deadline.Dispose(); }
}
