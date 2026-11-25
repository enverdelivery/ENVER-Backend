using System.Text;
using System.Text.Json;
using Enver.API.Controllers;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Features.Media;
using Enver.API.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Enver.API.SecurityTests;

public sealed partial class TokenValidationTests
{
    [Fact]
    public async Task Presign_authenticates_validates_and_denies_without_quota_or_signer()
    {
        var user = User.Create("Fixture", "media@uttt.edu.mx", Harness.SensitiveSentinel, DateTime.UtcNow);
        var fake = new MediaFixture();
        var options = new MediaOptions();
        using var host = new Harness(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(MediaController).Assembly);
            services.AddSingleton<IUserStore>(new AccountFixture(user));
            services.AddSingleton<IMediaReservationStore>(fake);
            services.AddSingleton<IMediaUploadSigner>(fake);
            services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
            services.AddSingleton(TimeProvider.System);
            services.AddScoped<MediaPresignService>();
        });
        var jwt = host.Issuer.Issue(user.Id).AccessToken!;
        const string jpeg = """{"fileName":"pedido.jpg","contentType":"image/jpeg"}""";
        foreach (var token in new[] { null, "invalid" }) await Problem(401, token, jpeg);
        await Problem(503, jwt, jpeg);
        options.MaxReservationsPerWindow = 10;
        options.ReservationWindowMinutes = 0;
        await Problem(503, jwt, jpeg);
        options.ReservationWindowMinutes = 10;
        await Problem(400, jwt, """{"fileName":"../pedido.jpg","contentType":"image/jpeg"}""");
        await Problem(400, jwt, jpeg.Insert(1, "\"ownerId\":\"other\","));
        await Problem(413, jwt, new string(' ', 4097));
        await Problem(415, jwt, jpeg.Replace("image/jpeg", "image/gif"));
        Assert.Equal(0, fake.Signs);
        Guid? previous = null;
        foreach (var (mime, extension) in new[] { ("image/jpeg", "jpg"), ("image/png", "png") })
        {
            using var response = await Send(jwt, jpeg.Replace("image/jpeg", mime));
            Assert.Equal(201, (int)response.StatusCode);
            Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var body = json.RootElement;
            var id = body.GetProperty("mediaId").GetGuid();
            Assert.NotEqual(previous, id);
            previous = id;
            Assert.Equal(5, body.EnumerateObject().Count());
            Assert.Equal(300, body.GetProperty("expiresIn").GetInt32());
            Assert.Equal(JsonValueKind.Null, body.GetProperty("imageUrl").ValueKind);
            var fields = body.GetProperty("uploadFields");
            Assert.Equal(2, fields.EnumerateObject().Count());
            Assert.Equal(mime, fields.GetProperty("Content-Type").GetString());
            Assert.Equal($"staging/orders/{user.Id:D}/{id:D}.{extension}", fields.GetProperty("key").GetString());
            Assert.Equal(user.Id, fake.Last!.OwnerId);
            Assert.Equal(MediaStatus.Pending, fake.Last.Status);
        }
        fake.Exceeded = true;
        await Problem(429, jwt, jpeg);
        Assert.Equal(2, fake.Signs);
        fake.Exceeded = false;
        fake.Unavailable = true;
        await Problem(503, jwt, jpeg);
        host.Store.Status = TokenRevocationReadStatus.Revoked;
        await Problem(401, jwt, jpeg);
        Assert.Equal(3, fake.Signs);

        Task<HttpResponseMessage> Send(string? token, string body) => host.SendAsync(token, "/api/v1/media/presign", HttpMethod.Post,
            new StringContent(body, Encoding.UTF8, "application/json"));
        async Task Problem(int status, string? token, string body)
        {
            using var response = await Send(token, body);
            if (status is 401 or 503) { await host.AssertProblemAsync(response, status, token ?? ""); return; }
            Assert.Equal(status, (int)response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(status switch { 400 => "validation_failed", 413 => "payload_too_large", 415 => "unsupported_media_type", _ => "rate_limited" },
                json.RootElement.GetProperty("code").GetString());
        }
    }

    private sealed class MediaFixture : IMediaReservationStore, IMediaUploadSigner
    {
        public MediaReservation? Last { get; private set; }
        public int Signs { get; private set; }
        public bool Exceeded { get; set; }
        public bool Unavailable { get; set; }
        public Task<QuotaOutcome> ChargeAsync(Guid owner, DateTime start, DateTime end, int limit, RequestBudget budget) =>
            Task.FromResult(Exceeded ? QuotaOutcome.Exceeded : QuotaOutcome.Granted);
        public Task<bool> ReserveAsync(MediaReservation reservation, RequestBudget budget)
        { Last = reservation; return Task.FromResult(true); }
        public Task<SignedMediaUpload?> SignAsync(MediaReservation reservation, CancellationToken token)
        {
            Signs++;
            Assert.Equal(Last, reservation);
            return Task.FromResult<SignedMediaUpload?>(Unavailable ? null : new("https://example.invalid/", new Dictionary<string, string>
            { ["key"] = reservation.ObjectKey, ["Content-Type"] = reservation.ContentType }));
        }
    }
}
