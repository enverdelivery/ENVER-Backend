using Enver.API.Configuration;
using Enver.API.Features.Orders.Application;
using Enver.API.Features.Ratings;
using Enver.API.Infrastructure.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Enver.API.Composition;

internal static class RatingsServiceCollectionExtensions
{
    internal static void AddRatings(this IServiceCollection services)
    {
        services.AddSingleton<IRatingStore>(sp => new MongoRatingStore(sp.GetRequiredService<IMongoDatabase>(),
            sp.GetRequiredService<IOptions<MongoDbOptions>>().Value));
        services.AddScoped(sp => new RatingService(sp.GetRequiredService<OrderService>(), sp.GetRequiredService<IRatingStore>(), TimeProvider.System));
    }
}
