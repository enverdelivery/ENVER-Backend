using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Users.Core;
using Enver.API.Http;

namespace Enver.API.Features.Users.Application;

public enum OwnProfileStatus { Confirmed, Forbidden, Unavailable }
public sealed record OwnProfile(Guid Id, string FullName, string Email, string? Phone, ActiveRole ActiveRole,
    RescuerEligibility Eligibility);
public sealed record OwnProfileResult(OwnProfileStatus Status, OwnProfile? Profile = null, RescuerEligibility? Eligibility = null);

public sealed class UserProfileService(IUserStore accounts, IUserProfileStore profiles, IRescuerEligibility eligibility)
{
    public async Task<OwnProfileResult> ExecuteAsync(Guid id, RequestBudget budget, ProfileChange? change = null,
        bool eligibilityOnly = false)
    {
        var account = await accounts.FindByIdAsync(id, budget);
        if (account.Status == UserReadStatus.Unavailable || budget.Cause != BudgetCancellationCause.None)
            return new(OwnProfileStatus.Unavailable);
        if (account.Status != UserReadStatus.Found || account.User is not { Status: UserStatus.Active } user || user.Id != id)
            return new(OwnProfileStatus.Forbidden);

        var status = await eligibility.ReadAsync(id, budget);
        if (status is null || budget.Cause != BudgetCancellationCause.None) return new(OwnProfileStatus.Unavailable);
        if (eligibilityOnly) return new(OwnProfileStatus.Confirmed, Eligibility: status);

        var result = change is null ? await profiles.ReadAsync(id, budget) : await profiles.UpdateAsync(id, change, budget);
        if (budget.Cause != BudgetCancellationCause.None ||
            !(result is { Status: ProfileStoreStatus.Confirmed, Profile: not null } ||
              (change is null && result is { Status: ProfileStoreStatus.Missing, Profile: null })))
            return new(OwnProfileStatus.Unavailable);

        var profile = result.Profile;
        return new(OwnProfileStatus.Confirmed, new(id, profile?.FullName ?? user.FullName, user.NormalizedEmail,
            profile?.Phone, profile?.ActiveRole ?? ActiveRole.Solicitante, status));
    }
}
