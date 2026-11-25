using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Enver.API.Features.Media;
using Enver.API.Infrastructure.Aws;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class MediaSigningTests
{
    [Fact]
    public async Task Sdk_constructs_bounded_exact_POST_policy_locally()
    {
        const string syntheticSecret = "synthetic-secret-used-only-for-local-cryptography";
        using var sdk = new AmazonS3Client(new SessionAWSCredentials("synthetic-identifier", syntheticSecret, "synthetic-token"), RegionEndpoint.USEast1);
        var created = DateTime.UtcNow;
        var reservation = new MediaReservation(Guid.NewGuid(), Guid.NewGuid(), "staging/orders/synthetic/image.png", "image/png",
            MediaStatus.Pending, created, created.AddSeconds(300));
        var request = S3MediaUploadSigner.BuildRequest(reservation, "enver-offline-signing-fixture");
        var post = await sdk.CreatePresignedPostAsync(request); // Solo CPU local; credenciales sintéticas, sin request S3.
        var fields = new Dictionary<string, string>(post.Fields, StringComparer.OrdinalIgnoreCase);
        using var document = JsonDocument.Parse(Convert.FromBase64String(fields["policy"]));
        var root = document.RootElement;
        var conditions = root.GetProperty("conditions").EnumerateArray().ToArray();
        Assert.Contains(conditions, x => Matches(x, "key", reservation.ObjectKey));
        Assert.Contains(conditions, x => Matches(x, "Content-Type", "image/png"));
        Assert.Contains(conditions, x => x.ValueKind == JsonValueKind.Array && x.GetArrayLength() == 3 &&
            x[0].GetString() == "content-length-range" && x[1].GetInt32() == 1 && x[2].GetInt32() == 5 * 1024 * 1024);
        Assert.InRange(root.GetProperty("expiration").GetDateTime() - created, TimeSpan.FromSeconds(299), TimeSpan.FromSeconds(300));
        Assert.All(new[] { "key", "Content-Type", "x-amz-credential", "x-amz-date", "x-amz-algorithm", "x-amz-security-token", "x-amz-signature", "policy" },
            field => Assert.True(fields.ContainsKey(field), $"Missing required field {field}"));
        Assert.DoesNotContain(fields.Values, value => value.Contains(syntheticSecret, StringComparison.Ordinal));
        var configured = Options.Create(new MediaOptions { BucketName = request.BucketName, Region = "us-east-1" });
        var signer = new S3MediaUploadSigner(configured, new Credentials(new("synthetic-identifier", syntheticSecret, "synthetic-token")));
        var signed = Assert.IsType<SignedMediaUpload>(await signer.SignAsync(reservation, default));
        Assert.DoesNotContain(signed.Fields.Values, value => value.Contains(syntheticSecret, StringComparison.Ordinal));
        Assert.Null(await new S3MediaUploadSigner(configured, new Credentials(new("synthetic-identifier", syntheticSecret, null))).SignAsync(reservation, default));
        Assert.Null(await new S3MediaUploadSigner(configured, new Credentials(null)).SignAsync(reservation, default));
    }

    private static bool Matches(JsonElement condition, string name, string expected) => condition.ValueKind == JsonValueKind.Object &&
        condition.EnumerateObject().Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && p.Value.GetString() == expected);

    private sealed class Credentials(ImmutableCredentials? credentials) : IMediaSigningCredentials
    {
        public Task<ImmutableCredentials?> GetAsync(CancellationToken token) => Task.FromResult(credentials);
    }
}
