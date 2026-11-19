using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace Enver.RuntimeConfig.Materializer;

internal sealed record SecretPin(string Name, string Arn, string Version);
internal static class Pins
{
    private const string Prefix = "arn:aws:secretsmanager:us-east-1:926925605666:secret:enver/dev/api/";
    internal static readonly SecretPin[] All =
    [
        new("redis", Prefix + "redis-FlY8aP", "2dfc79eb-625a-4f1b-aca7-b1575b5142c1"),
        new("mongodb", Prefix + "mongodb-Iuqeur", "c9eb0b6c-d8c0-4ec1-b3fe-05809a0f33f4"),
        new("hmac", Prefix + "hmac-s9TSqb", "fecf274e-12d7-4c58-b0d8-06d2149c015a"),
        new("jwt", Prefix + "jwt-AweYNs", "fe44b116-0c69-41c8-8daf-7c82be31d181")
    ];
}

internal interface ISecretReader
{
    Task<string> Read(SecretPin pin, CancellationToken cancellationToken);
}

internal sealed class PinnedSecretReader(IAmazonSecretsManager client) : ISecretReader
{
    public async Task<string> Read(SecretPin pin, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.GetSecretValueAsync(new GetSecretValueRequest
            { SecretId = pin.Arn, VersionId = pin.Version }, cancellationToken);
            if (response.ARN != pin.Arn || response.VersionId != pin.Version ||
                string.IsNullOrWhiteSpace(response.SecretString) || response.SecretBinary is not null)
                throw new DeliveryFailure("secret-envelope-invalid");
            return response.SecretString;
        }
        catch (Exception error) { throw DeliveryFailure.Wrap(error); }
    }
}

internal static class InstanceIdentity
{
    private static readonly string[] AllowedInstanceIds =
    [
        "i-0a66085b7b2411393",
        "i-00de11a3f3c1a8aa5"
    ];

    internal static void RequireAllowedIdentity(string? instanceId, string? role)
    {
        if (instanceId is null || !AllowedInstanceIds.Contains(instanceId, StringComparer.Ordinal) ||
            !string.Equals(role, "enver-dev-api-runtime", StringComparison.Ordinal))
            throw new DeliveryFailure("instance-role-mismatch");
    }

    internal static async Task Require(CancellationToken cancellationToken)
    {
        foreach (var key in new[] { "AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY", "AWS_SESSION_TOKEN", "AWS_SECURITY_TOKEN",
            "AWS_PROFILE", "AWS_DEFAULT_PROFILE", "AWS_WEB_IDENTITY_TOKEN_FILE", "AWS_ROLE_ARN",
            "AWS_CONTAINER_CREDENTIALS_RELATIVE_URI", "AWS_CONTAINER_CREDENTIALS_FULL_URI", "AWS_EC2_METADATA_SERVICE_ENDPOINT" })
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key))) throw new DeliveryFailure("credential-source-forbidden");
        if (Environment.GetEnvironmentVariable("AWS_EC2_METADATA_V1_DISABLED") != "true" ||
            Environment.GetEnvironmentVariable("AWS_EC2_METADATA_DISABLED") != "false" ||
            Environment.GetEnvironmentVariable("AWS_CONFIG_FILE") != "/dev/null" ||
            Environment.GetEnvironmentVariable("AWS_SHARED_CREDENTIALS_FILE") != "/dev/null" ||
            Environment.GetEnvironmentVariable("HOME") != "/nonexistent") throw new DeliveryFailure("credential-isolation-required");
        using var handler = new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false };
        using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(3) };
        using var request = new HttpRequestMessage(HttpMethod.Put, "http://169.254.169.254/latest/api/token");
        request.Headers.Add("X-aws-ec2-metadata-token-ttl-seconds", "60");
        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadAsStringAsync(cancellationToken);
        async Task<string> Read(string path)
        {
            using var get = new HttpRequestMessage(HttpMethod.Get, "http://169.254.169.254/latest/meta-data/" + path);
            get.Headers.Add("X-aws-ec2-metadata-token", token);
            using var answer = await http.SendAsync(get, cancellationToken);
            answer.EnsureSuccessStatusCode();
            return (await answer.Content.ReadAsStringAsync(cancellationToken)).Trim();
        }
        RequireAllowedIdentity(await Read("instance-id"), await Read("iam/security-credentials/"));
    }

    internal static async Task<AmazonSecretsManagerClient> CreateClient(CancellationToken cancellationToken)
    {
        AWSConfigs.LoggingConfig.LogTo = LoggingOptions.None;
        AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.Never;
        AWSConfigs.LoggingConfig.LogMetrics = false;
        var credentials = new InstanceProfileAWSCredentials();
        // Resolve here rather than lazily in the first secret request, so failures are
        // attributed to aws-credentials. The returned credential material is never inspected.
        _ = await credentials.GetCredentialsAsync().WaitAsync(cancellationToken);
        return new(credentials, new AmazonSecretsManagerConfig
        { RegionEndpoint = RegionEndpoint.USEast1, MaxErrorRetry = 0, Timeout = TimeSpan.FromSeconds(15), LogResponse = false, LogMetrics = false });
    }
}
