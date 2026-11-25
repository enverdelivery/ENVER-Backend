using Enver.API.Configuration;
using System.Globalization;
using Enver.API.Features.Media;
using Enver.API.Infrastructure.Aws;
using Enver.API.Infrastructure.MongoDB;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Enver.API.Composition;

internal static class MediaServiceCollectionExtensions
{
    internal static void AddMedia(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MediaOptions>(options =>
        {
            var section = configuration.GetSection("Media");
            options.MaxReservationsPerWindow = Parse(section[nameof(MediaOptions.MaxReservationsPerWindow)]);
            options.ReservationWindowMinutes = Parse(section[nameof(MediaOptions.ReservationWindowMinutes)]);
            options.BucketName = section[nameof(MediaOptions.BucketName)];
            options.Region = section[nameof(MediaOptions.Region)];
            options.UploadSigningInstanceRole = section[nameof(MediaOptions.UploadSigningInstanceRole)];
        });
        services.AddSingleton<IMediaReservationStore>(sp => new MongoMediaReservationStore(sp.GetRequiredService<IMongoDatabase>(),
            sp.GetRequiredService<IOptions<MongoDbOptions>>().Value));
        services.AddSingleton<IMediaSigningCredentials, InstanceRoleMediaSigningCredentials>();
        services.AddSingleton<IMediaUploadSigner, S3MediaUploadSigner>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<MediaPresignService>();
    }

    private static int? Parse(string? text) => int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : null;
}
