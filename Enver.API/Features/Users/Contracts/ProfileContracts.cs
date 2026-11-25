using Enver.API.Features.Users.Application;

namespace Enver.API.Features.Users.Contracts;

public sealed record ProfileResponse(Guid Id, string FullName, string Email, string? Phone, string ActiveRole,
    bool RescuerEnabled, string? RescuerLevel, decimal? Rating)
{
    public static ProfileResponse From(OwnProfile profile) => new(profile.Id, profile.FullName, profile.Email,
        profile.Phone, profile.ActiveRole.ToString(), profile.Eligibility.Enabled, profile.Eligibility.Level, null);
}

public sealed record RescuerStatusResponse(bool RescuerEnabled, string? RescuerLevel);
