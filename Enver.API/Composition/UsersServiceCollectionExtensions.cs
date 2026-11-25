using Enver.API.Configuration;
using Enver.API.Features.Users.Application;
using Enver.API.Infrastructure.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Enver.API.Composition;

internal static class UsersServiceCollectionExtensions
{
    internal static void AddUsers(this IServiceCollection services)
    {
        services.AddSingleton<IUserProfileStore>(sp => new MongoUserProfileStore(sp.GetRequiredService<IMongoDatabase>(),
            sp.GetRequiredService<IOptions<MongoDbOptions>>().Value));
        services.AddSingleton<IRescuerEligibility, DisabledRescuerEligibility>();
        services.AddSingleton<IRescuerEnrollment, DisabledRescuerEnrollment>();
        services.AddScoped<UserProfileService>();
    }
}
