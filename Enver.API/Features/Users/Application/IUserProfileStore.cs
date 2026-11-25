using Enver.API.Features.Users.Core;
using Enver.API.Http;

namespace Enver.API.Features.Users.Application;

public enum ProfileStoreStatus { Confirmed, Missing, Unavailable }
public sealed record ProfileStoreResult(ProfileStoreStatus Status, UserProfile? Profile = null);

public interface IUserProfileStore
{
    Task<ProfileStoreResult> ReadAsync(Guid id, RequestBudget budget);
    Task<ProfileStoreResult> UpdateAsync(Guid id, ProfileChange change, RequestBudget budget);
}

// null denotes an unavailable source, not permission. The MVP has no approved source.
public sealed record RescuerEligibility(bool Enabled, string? Level);
public interface IRescuerEligibility
{
    ValueTask<RescuerEligibility?> ReadAsync(Guid id, RequestBudget budget);
}

internal sealed class DisabledRescuerEligibility : IRescuerEligibility
{
    public ValueTask<RescuerEligibility?> ReadAsync(Guid id, RequestBudget budget) =>
        ValueTask.FromResult<RescuerEligibility?>(new(false, null));
}
