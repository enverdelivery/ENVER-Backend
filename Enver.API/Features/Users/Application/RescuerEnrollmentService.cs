using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Features.Users.Validation;
using Enver.API.Http;

namespace Enver.API.Features.Users.Application;

public enum EnrollmentOutcome { Unavailable, Enabled, Forbidden, PhoneRequired, VersionConflict, RateLimited }
public sealed record EnrollmentResult(EnrollmentOutcome Outcome, TimeSpan? RetryAfter = null);
public enum EnrollmentQuotaStatus { Unavailable, Allowed, Limited }
public sealed record EnrollmentQuotaResult(EnrollmentQuotaStatus Status, TimeSpan? RetryAfter = null);
public sealed record EnrollmentPolicy(string RulesVersion, string PolicyVersion);

public interface IRescuerEnrollmentQuota
{
    ValueTask<EnrollmentQuotaResult> ConsumeAsync(Guid id, RequestBudget budget);
}

public interface IRescuerEnrollment
{
    Task<EnrollmentResult> ExecuteAsync(Guid id, string rulesVersion, RequestBudget budget);
}

internal sealed class DisabledRescuerEnrollment : IRescuerEnrollment
{
    public Task<EnrollmentResult> ExecuteAsync(Guid id, string rulesVersion, RequestBudget budget) =>
        Task.FromResult(new EnrollmentResult(EnrollmentOutcome.Unavailable));
}

public sealed class RescuerEnrollmentService(IUserStore accounts, IUserProfileStore profiles,
    IRescuerEnrollmentStore grants, InstitutionalEmailNormalizer emails, IRescuerEnrollmentQuota quota,
    EnrollmentPolicy policy, TimeProvider clock) : IRescuerEnrollment
{
    public async Task<EnrollmentResult> ExecuteAsync(Guid id, string rulesVersion, RequestBudget budget)
    {
        if (budget.Cause != BudgetCancellationCause.None) return new(EnrollmentOutcome.Unavailable);
        var admission = await quota.ConsumeAsync(id, budget);
        if (budget.Cause != BudgetCancellationCause.None) return new(EnrollmentOutcome.Unavailable);
        if (admission.Status == EnrollmentQuotaStatus.Limited) return new(EnrollmentOutcome.RateLimited, admission.RetryAfter);
        if (admission.Status != EnrollmentQuotaStatus.Allowed) return new(EnrollmentOutcome.Unavailable);

        var account = await accounts.FindByIdAsync(id, budget);
        if (budget.Cause != BudgetCancellationCause.None) return new(EnrollmentOutcome.Unavailable);
        if (account.Status == UserReadStatus.NotFound) return new(EnrollmentOutcome.Forbidden);
        if (account is not { Status: UserReadStatus.Found, User: { } user } || user.Id != id)
            return new(EnrollmentOutcome.Unavailable);
        if (user.Status != UserStatus.Active) return new(EnrollmentOutcome.Forbidden);

        var current = await grants.ReadAsync(id, budget);
        if (budget.Cause != BudgetCancellationCause.None) return new(EnrollmentOutcome.Unavailable);
        if (current.Status != GrantReadStatus.Missing) return Project(current, id, rulesVersion);

        if (!RescuerEnrollmentInput.IsVersion(policy.RulesVersion) || !RescuerEnrollmentInput.IsVersion(policy.PolicyVersion))
            return new(EnrollmentOutcome.Unavailable);
        if (rulesVersion != policy.RulesVersion) return new(EnrollmentOutcome.VersionConflict);
        if (!emails.TryNormalize(user.NormalizedEmail, out _)) return new(EnrollmentOutcome.Forbidden);

        var profile = await profiles.ReadAsync(id, budget);
        if (budget.Cause != BudgetCancellationCause.None) return new(EnrollmentOutcome.Unavailable);
        if (profile.Status == ProfileStoreStatus.Missing) return new(EnrollmentOutcome.PhoneRequired);
        if (profile is not { Status: ProfileStoreStatus.Confirmed, Profile: { } saved }) return new(EnrollmentOutcome.Unavailable);
        if (!ProfileInput.IsValidPhone(saved.Phone)) return new(EnrollmentOutcome.PhoneRequired);

        var decision = new EnrollmentDecision(Guid.NewGuid(), id, rulesVersion, policy.PolicyVersion, clock.GetUtcNow().UtcDateTime);
        var written = await grants.InsertAsync(decision, budget);
        if (budget.Cause != BudgetCancellationCause.None || written == GrantWriteStatus.Unavailable)
            return new(EnrollmentOutcome.Unavailable);
        if (written == GrantWriteStatus.Confirmed) return new(EnrollmentOutcome.Enabled);

        var winner = await grants.ReadAsync(id, budget);
        return budget.Cause == BudgetCancellationCause.None ? Project(winner, id, rulesVersion) : new(EnrollmentOutcome.Unavailable);
    }

    private static EnrollmentResult Project(GrantRead result, Guid id, string version)
    {
        if (result is not { Status: GrantReadStatus.Confirmed, Grant: { } grant } || grant.Id != id)
            return new(EnrollmentOutcome.Unavailable);
        return grant.Status switch
        {
            GrantStatus.Revoked => new(EnrollmentOutcome.Forbidden),
            GrantStatus.Enabled when grant.RulesVersion == version => new(EnrollmentOutcome.Enabled),
            GrantStatus.Enabled => new(EnrollmentOutcome.VersionConflict),
            _ => new(EnrollmentOutcome.Unavailable)
        };
    }
}
