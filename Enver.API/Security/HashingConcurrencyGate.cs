using Enver.API.Configuration;
using Enver.API.Http;
using Microsoft.Extensions.Options;

namespace Enver.API.Security;

/// <summary>Admisión local inmediata sin cola. El permiso pertenece al trabajo real, no a su consumidor.</summary>
public sealed class HashingConcurrencyGate
{
    private readonly SemaphoreSlim semaphore;
    public HashingConcurrencyGate(IOptions<AuthenticationOptions> options)
    {
        Limit = options.Value.HashingConcurrencyLimit;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Limit);
        semaphore = new(Limit, Limit);
    }
    public int Limit { get; }

    public IDisposable? TryAcquire(RequestBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);
        if (budget.Cause != BudgetCancellationCause.None || !semaphore.Wait(0)) return null;
        if (budget.Cause == BudgetCancellationCause.None) return new Lease(semaphore);
        semaphore.Release();
        return null;
    }

    private sealed class Lease(SemaphoreSlim semaphore) : IDisposable
    {
        private int disposed;
        public void Dispose() { if (Interlocked.Exchange(ref disposed, 1) == 0) semaphore.Release(); }
    }
}
