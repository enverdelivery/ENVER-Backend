using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Http;
using Microsoft.AspNetCore.Identity;

namespace Enver.API.Security;

/// <summary>Adaptador síncrono del framework. No consulta ni escribe usuarios ni coordina CAS.</summary>
public sealed class PasswordService(IPasswordHasher<User> hasher, PasswordPolicy policy, DummyPasswordHash dummy, HashingConcurrencyGate concurrency) : IPasswordService
{
    public PasswordHashResult Hash(User user, string secret, RequestBudget budget)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(budget);
        if (!policy.IsValidForCreation(secret)) return new(PasswordHashStatus.InvalidInput);
        using var lease = concurrency.TryAcquire(budget);
        if (lease is null) return new(PasswordHashStatus.Unavailable);
        var hash = hasher.HashPassword(user, secret);
        return budget.Cause == BudgetCancellationCause.None ? new(PasswordHashStatus.Completed, hash) : new(PasswordHashStatus.Unavailable);
    }

    // Una credencial existente no debe volver a satisfacer los mínimos de creación de Register.
    public PasswordHashResult Rehash(User user, string secret, RequestBudget budget)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(budget);
        if (!policy.IsBoundedForVerification(secret)) return new(PasswordHashStatus.InvalidInput);
        using var lease = concurrency.TryAcquire(budget);
        if (lease is null) return new(PasswordHashStatus.Unavailable);
        var hash = hasher.HashPassword(user, secret);
        return budget.Cause == BudgetCancellationCause.None ? new(PasswordHashStatus.Completed, hash) : new(PasswordHashStatus.Unavailable);
    }

    public PasswordVerificationStatus Verify(User user, string secret, RequestBudget budget)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(budget);
        if (!policy.IsBoundedForVerification(secret)) return PasswordVerificationStatus.Failed;
        using var lease = concurrency.TryAcquire(budget);
        if (lease is null) return PasswordVerificationStatus.Unavailable;
        var verified = hasher.VerifyHashedPassword(user, user.PasswordHash, secret);
        if (budget.Cause != BudgetCancellationCause.None) return PasswordVerificationStatus.Unavailable;
        return verified switch
        {
            PasswordVerificationResult.Success => PasswordVerificationStatus.Success,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordVerificationStatus.SuccessRehashNeeded,
            _ => PasswordVerificationStatus.Failed
        };
    }

    public PasswordVerificationStatus VerifyDummy(string secret, RequestBudget budget) => dummy.IsInitialized
        ? Verify(dummy.User, secret, budget) : PasswordVerificationStatus.Unavailable;
    public override string ToString() => nameof(PasswordService);
}
