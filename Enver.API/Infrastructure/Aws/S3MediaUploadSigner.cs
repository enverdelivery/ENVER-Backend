using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Enver.API.Features.Media;
using Microsoft.Extensions.Options;

namespace Enver.API.Infrastructure.Aws;

public interface IMediaSigningCredentials
{
    Task<ImmutableCredentials?> GetAsync(CancellationToken token);
}

// IMDS para un rol de instancia de Media explícito. Sin archivos de perfiles ni cadena global.
// Un proveedor STS dedicado puede reemplazar este registro sin cambiar el firmante.
internal sealed class InstanceRoleMediaSigningCredentials(IOptions<MediaOptions> options) : IMediaSigningCredentials, IDisposable
{
    private readonly Lazy<InstanceProfileAWSCredentials?> provider = new(() =>
        string.IsNullOrWhiteSpace(options.Value.UploadSigningInstanceRole) ? null : new(options.Value.UploadSigningInstanceRole));

    public async Task<ImmutableCredentials?> GetAsync(CancellationToken token) => provider.Value is { } role
        ? await role.GetCredentialsAsync().WaitAsync(token) : null;

    public void Dispose() { if (provider.IsValueCreated) provider.Value?.Dispose(); }
}

public sealed class S3MediaUploadSigner(IOptions<MediaOptions> options, IMediaSigningCredentials credentials) : IMediaUploadSigner
{
    public static CreatePresignedPostRequest BuildRequest(MediaReservation reservation, string bucket) => new()
    {
        BucketName = bucket, Key = reservation.ObjectKey, Expires = reservation.UploadExpiresAt,
        Fields = new() { ["Content-Type"] = reservation.ContentType },
        Conditions = [S3PostCondition.ExactMatch("Content-Type", reservation.ContentType),
            S3PostCondition.ContentLengthRange(1, MediaPresignService.MaxFileBytes)]
    };

    public async Task<SignedMediaUpload?> SignAsync(MediaReservation reservation, CancellationToken token)
    {
        try
        {
            var config = options.Value;
            if (string.IsNullOrWhiteSpace(config.Region) || string.IsNullOrWhiteSpace(config.BucketName)) return null;
            var session = await credentials.GetAsync(token).WaitAsync(token);
            if (session is not { UseToken: true }) return null; // Nunca firmar con credenciales estáticas.
            using var client = new AmazonS3Client(new SessionAWSCredentials(session.AccessKey, session.SecretKey, session.Token),
                RegionEndpoint.GetBySystemName(config.Region));
            var signed = await client.CreatePresignedPostAsync(BuildRequest(reservation, config.BucketName)).WaitAsync(token);
            return Uri.TryCreate(signed.Url, UriKind.Absolute, out var url) && url.Scheme == Uri.UriSchemeHttps && signed.Fields is { Count: > 0 }
                ? new(signed.Url, signed.Fields) : null;
        }
        catch (Exception) when (!token.IsCancellationRequested) { return null; } // Nunca registrar material AWS.
    }
}
