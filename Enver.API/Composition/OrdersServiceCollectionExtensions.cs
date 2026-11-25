using Enver.API.Configuration;
using Enver.API.Features.Orders.Application;
using Enver.API.Infrastructure.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Enver.API.Composition;

internal static class OrdersServiceCollectionExtensions
{
    internal static void AddOrders(this IServiceCollection services)
    {
        services.AddSingleton<IOrderStore>(sp => new MongoOrderStore(sp.GetRequiredService<IMongoDatabase>(),
            sp.GetRequiredService<IOptions<MongoDbOptions>>().Value));
        // La raíz runtime ya es común a las réplicas; el cursor usa su propia derivación, no claves JWT.
        services.AddSingleton(sp => new OrderCursor(sp.GetRequiredService<IOptions<RateLimitingOptions>>().Value.PseudonymizationKeyBase64));
        services.AddScoped(sp => new OrderService(sp.GetRequiredService<Features.Authentication.Application.Ports.IUserStore>(),
            sp.GetRequiredService<Features.Users.Application.IUserProfileStore>(), sp.GetRequiredService<IOrderStore>(), TimeProvider.System));
        services.AddScoped(sp => new OrderDeliveryService(sp.GetRequiredService<IOrderStore>(),
            sp.GetRequiredService<Features.Users.Application.IRescuerEligibility>(), TimeProvider.System));
    }
}
