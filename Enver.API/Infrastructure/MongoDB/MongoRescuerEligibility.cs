using Enver.API.Features.Users.Application;
using Enver.API.Http;

namespace Enver.API.Infrastructure.MongoDB;

public sealed class MongoRescuerEligibility(MongoRescuerEligibilityStore store) : IRescuerEligibility
{
    public async ValueTask<RescuerEligibility?> ReadAsync(Guid id, RequestBudget budget) =>
        await store.ReadAsync(id, budget) switch
        {
            { Status: GrantReadStatus.Missing } => new(false, null),
            { Status: GrantReadStatus.Confirmed, Grant.Status: GrantStatus.Revoked } => new(false, null),
            { Status: GrantReadStatus.Confirmed, Grant.Status: GrantStatus.Enabled } => new(true, null),
            _ => null
        };
}
