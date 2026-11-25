using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Microsoft.Extensions.Options;

namespace Enver.API.Features.Media;

public sealed class MediaOptions
{
    public int? MaxReservationsPerWindow { get; set; }
    public int? ReservationWindowMinutes { get; set; }
    public string? BucketName { get; set; }
    public string? Region { get; set; }
    public string? UploadSigningInstanceRole { get; set; }
    public bool HasQuota => MaxReservationsPerWindow is > 0 && ReservationWindowMinutes is >= 1 and <= 1440;
}

public enum MediaStatus { Pending, Ready, Rejected }
public sealed record MediaReservation(Guid MediaId, Guid OwnerId, string ObjectKey, string ContentType,
    MediaStatus Status, DateTime CreatedAt, DateTime UploadExpiresAt);
public enum QuotaOutcome { Granted, Exceeded, Unavailable }
public interface IMediaReservationStore
{
    Task<QuotaOutcome> ChargeAsync(Guid owner, DateTime windowStart, DateTime windowEnd, int limit, RequestBudget budget);
    Task<bool> ReserveAsync(MediaReservation reservation, RequestBudget budget);
}

// No almacenar ni registrar el resultado de firma.
public sealed class SignedMediaUpload(string url, IReadOnlyDictionary<string, string> fields)
{
    public string Url { get; } = url;
    public IReadOnlyDictionary<string, string> Fields { get; } = fields;
}
public interface IMediaUploadSigner
{
    Task<SignedMediaUpload?> SignAsync(MediaReservation reservation, CancellationToken token);
}

public sealed class MediaPresignResponse(Guid mediaId, SignedMediaUpload upload)
{
    public Guid MediaId { get; } = mediaId;
    public string UploadUrl => upload.Url;
    public IReadOnlyDictionary<string, string> UploadFields => upload.Fields;
    public int ExpiresIn => MediaPresignService.ExpiresIn;
    public string? ImageUrl => null;
}

public sealed class MediaPresignService(IUserStore accounts, IMediaReservationStore store, IMediaUploadSigner signer,
    IOptions<MediaOptions> options, TimeProvider clock)
{
    public const int ExpiresIn = 300;
    public const int MaxFileBytes = 5 * 1024 * 1024;
    public static string? Extension(string mime) => mime switch { "image/jpeg" => "jpg", "image/png" => "png", _ => null };

    public async Task<(int Status, MediaPresignResponse? Body, TimeSpan? RetryAfter)> IssueAsync(Guid owner, string mime, RequestBudget budget)
    {
        var extension = Extension(mime);
        if (extension is null) return (415, null, null);
        var config = options.Value;
        if (!config.HasQuota) return (503, null, null);
        var account = await accounts.FindByIdAsync(owner, budget);
        if (account.Status == UserReadStatus.Unavailable || budget.Cause != BudgetCancellationCause.None) return (503, null, null);
        if (account.Status != UserReadStatus.Found || account.User is not { Status: UserStatus.Active } user || user.Id != owner)
            return (403, null, null);

        var now = clock.GetUtcNow();
        var windowSeconds = config.ReservationWindowMinutes!.Value * 60L;
        var seconds = now.ToUnixTimeSeconds();
        var start = DateTimeOffset.FromUnixTimeSeconds(seconds - seconds % windowSeconds);
        var end = start.AddSeconds(windowSeconds);
        var charge = await store.ChargeAsync(owner, start.UtcDateTime, end.UtcDateTime, config.MaxReservationsPerWindow!.Value, budget);
        if (charge == QuotaOutcome.Exceeded) return (429, null, end - now);
        if (charge != QuotaOutcome.Granted || budget.Cause != BudgetCancellationCause.None) return (503, null, null);

        var id = Guid.NewGuid();
        now = clock.GetUtcNow();
        var reservation = new MediaReservation(id, owner, $"staging/orders/{owner:D}/{id:D}.{extension}", mime,
            MediaStatus.Pending, now.UtcDateTime, now.AddSeconds(ExpiresIn).UtcDateTime);
        // Una reserva consumida no se devuelve ante un resultado incierto. Nunca entregar un POST antes del ACK Mongo.
        if (!await store.ReserveAsync(reservation, budget) || budget.Cause != BudgetCancellationCause.None) return (503, null, null);
        var upload = await signer.SignAsync(reservation, budget.Token).WaitAsync(budget.Token);
        return upload is null || budget.Cause != BudgetCancellationCause.None ? (503, null, null) : (201, new(id, upload), null);
    }
}
