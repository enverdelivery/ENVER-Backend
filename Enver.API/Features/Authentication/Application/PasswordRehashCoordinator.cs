using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application;

public enum PasswordRehashStatus { Verified, Rejected, Unavailable }

/// <summary>Resultado interno sin usuario, secreto o hash. No representa emisión ni respuesta HTTP.</summary>
public sealed class PasswordRehashResult(PasswordRehashStatus status, bool updated = false, bool raceLost = false, bool uncertain = false)
{
    public PasswordRehashStatus Status { get; } = status;
    public bool Updated { get; } = updated;
    public bool RaceLost { get; } = raceLost;
    public bool Uncertain { get; } = uncertain;
    public override string ToString() => nameof(PasswordRehashResult);
}

/// <summary>Un CAS; solo un Conflict confirmado permite una relectura y una reverificación. Sin JWT/cuotas/retry.</summary>
public sealed class PasswordRehashCoordinator(IUserStore users, IPasswordService passwords, TimeProvider? clock = null)
{
    public async ValueTask<PasswordRehashResult> ResolveAsync(User snapshot, PasswordVerificationStatus verified,
        string secret, RequestBudget budget)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(budget);
        if (snapshot.Status != UserStatus.Active || verified != PasswordVerificationStatus.SuccessRehashNeeded ||
            budget.Cause != BudgetCancellationCause.None) return new(PasswordRehashStatus.Unavailable);
        var hash = passwords.Rehash(snapshot, secret, budget);
        if (hash.Status != PasswordHashStatus.Completed || hash.Hash is null || budget.Cause != BudgetCancellationCause.None)
            return new(PasswordRehashStatus.Unavailable);
        var changed = await users.CompareExchangePasswordHashAsync(snapshot.Id, snapshot.PasswordHash, UserStatus.Active,
            hash.Hash, (clock ?? TimeProvider.System).GetUtcNow().UtcDateTime, budget).ConfigureAwait(false);
        if (budget.Cause != BudgetCancellationCause.None)
            return new(PasswordRehashStatus.Unavailable, updated: changed.Status == UserCredentialUpdateStatus.Updated,
                uncertain: changed.Status == UserCredentialUpdateStatus.Uncertain);
        if (changed.Status == UserCredentialUpdateStatus.Updated) return new(PasswordRehashStatus.Verified, updated: true);
        if (changed.Status != UserCredentialUpdateStatus.Conflict)
            return new(PasswordRehashStatus.Unavailable, uncertain: changed.Status == UserCredentialUpdateStatus.Uncertain);

        var read = await users.FindByIdAsync(snapshot.Id, budget).ConfigureAwait(false);
        if (budget.Cause != BudgetCancellationCause.None || read.Status == UserReadStatus.Unavailable)
            return new(PasswordRehashStatus.Unavailable, raceLost: true);
        if (read.Status == UserReadStatus.NotFound) return new(PasswordRehashStatus.Rejected, raceLost: true);
        var current = read.User!;
        if (current.Id != snapshot.Id) return new(PasswordRehashStatus.Unavailable, raceLost: true);
        var check = passwords.Verify(current, secret, budget);
        if (budget.Cause != BudgetCancellationCause.None || check == PasswordVerificationStatus.Unavailable)
            return new(PasswordRehashStatus.Unavailable, raceLost: true);
        if (current.Status == UserStatus.Disabled || check == PasswordVerificationStatus.Failed)
            return new(PasswordRehashStatus.Rejected, raceLost: true);
        return new(check == PasswordVerificationStatus.Success ? PasswordRehashStatus.Verified : PasswordRehashStatus.Unavailable,
            raceLost: true); // SuccessRehashNeeded otra vez: 503 conservador, jamás un segundo CAS.
    }
}
