using System.Text.Json;
using Amazon.Runtime;

namespace Enver.RuntimeConfig.Materializer;

internal enum DeliveryPhase
{
    Arguments, TmpfsCheck, AwsCredentials, RedisSecretRead, MongodbSecretRead, HmacSecretRead, JwtSecretRead,
    RedisParse, RedisMap, MongodbParse, MongodbMap, HmacParse, HmacMap, JwtParse, JwtMap,
    NonsecretConfig, JsonBuild, ConfigParseValidation, ConfigBindingValidation, TempFileCreate,
    FileWrite, FileFlush, FilePermissions, FinalFileCheck, DirectoryFlush, AtomicRename, TempCleanup,
    TemporaryConfigParseValidation, TemporaryConfigBindingValidation, FinalConfigParseValidation, FinalConfigBindingValidation
}

internal enum ValidationTarget { Memory, TemporaryFile, FinalFile }

internal sealed class DeliveryTrace(Action<DeliveryPhase>? before = null)
{
    internal DeliveryPhase Current { get; private set; } = DeliveryPhase.Arguments;
    internal ValidationTarget? Target { get; private set; }
    internal void Enter(DeliveryPhase phase) { Target = null; Current = phase; before?.Invoke(phase); }
    internal void EnterValidation(DeliveryPhase phase, ValidationTarget target)
    { Target = target; Current = phase; before?.Invoke(phase); }
    internal void WriteFailure(TextWriter output, Exception error, CancellationToken cancellationToken)
    {
        var safe = SafeException.Capture(error);
        output.WriteLine(JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["phase"] = Label(Current), ["exception"] = safe.Type, ["inner"] = safe.Inner,
            ["cancelled"] = safe.Cancelled || cancellationToken.IsCancellationRequested,
            ["category"] = safe.Category, ["code"] = safe.Code,
            ["validation-target"] = Target switch
            {
                ValidationTarget.Memory => "memory",
                ValidationTarget.TemporaryFile => "temporary-file",
                ValidationTarget.FinalFile => "final-file",
                _ => "not-applicable"
            }
        }));
    }

    internal static string Label(DeliveryPhase phase) => phase switch
    {
        DeliveryPhase.Arguments => "arguments",
        DeliveryPhase.TmpfsCheck => "tmpfs-check",
        DeliveryPhase.AwsCredentials => "aws-credentials",
        DeliveryPhase.RedisSecretRead => "redis-secret-read",
        DeliveryPhase.MongodbSecretRead => "mongodb-secret-read",
        DeliveryPhase.HmacSecretRead => "hmac-secret-read",
        DeliveryPhase.JwtSecretRead => "jwt-secret-read",
        DeliveryPhase.RedisParse => "redis-parse",
        DeliveryPhase.RedisMap => "redis-map",
        DeliveryPhase.MongodbParse => "mongodb-parse",
        DeliveryPhase.MongodbMap => "mongodb-map",
        DeliveryPhase.HmacParse => "hmac-parse",
        DeliveryPhase.HmacMap => "hmac-map",
        DeliveryPhase.JwtParse => "jwt-parse",
        DeliveryPhase.JwtMap => "jwt-map",
        DeliveryPhase.NonsecretConfig => "nonsecret-config",
        DeliveryPhase.JsonBuild => "json-build",
        DeliveryPhase.ConfigParseValidation => "config-parse-validation",
        DeliveryPhase.ConfigBindingValidation => "config-binding-validation",
        DeliveryPhase.TempFileCreate => "temp-file-create",
        DeliveryPhase.FileWrite => "file-write",
        DeliveryPhase.FileFlush => "file-flush",
        DeliveryPhase.FilePermissions => "file-permissions",
        DeliveryPhase.FinalFileCheck => "final-file-check",
        DeliveryPhase.DirectoryFlush => "directory-flush",
        DeliveryPhase.AtomicRename => "atomic-rename",
        DeliveryPhase.TempCleanup => "temp-cleanup",
        DeliveryPhase.TemporaryConfigParseValidation => "temp-config-parse-validation",
        DeliveryPhase.TemporaryConfigBindingValidation => "temp-config-binding-validation",
        DeliveryPhase.FinalConfigParseValidation => "final-config-parse-validation",
        DeliveryPhase.FinalConfigBindingValidation => "final-config-binding-validation",
        _ => "arguments"
    };
}

// Only allowlisted, bounded metadata is retained. Never retain the original exception,
// Message, Data, StackTrace, endpoint, request ID, payload, or dynamic type name.
internal sealed record SafeException(string Type, string[] Inner, bool Cancelled, string Category, string Code)
{
    internal static SafeException Capture(Exception error)
    {
        if (error is DeliveryFailure failure) return failure.Safe;
        var inner = new List<string>();
        var cancelled = error is OperationCanceledException;
        for (var current = error.InnerException; current is not null && inner.Count < 4; current = current.InnerException)
        {
            if (current is DeliveryFailure wrapped)
            {
                inner.Add(wrapped.Safe.Type);
                inner.AddRange(wrapped.Safe.Inner.Take(4 - inner.Count));
                cancelled |= wrapped.Safe.Cancelled;
                break;
            }
            inner.Add(TypeName(current));
            cancelled |= current is OperationCanceledException;
        }
        var (category, code) = error switch
        {
            AmazonServiceException aws => ("aws", AwsCode(aws.ErrorCode)),
            AmazonClientException => ("aws-client", "aws-client-failed"),
            OperationCanceledException => ("cancelled", "operation-cancelled"),
            UnauthorizedAccessException => ("filesystem", "access-denied"),
            IOException => ("filesystem", "io-failed"),
            JsonException => ("json", "json-invalid"),
            HttpRequestException => ("network", "http-failed"),
            _ => ("validation", "validation-failed")
        };
        return new(TypeName(error), inner.ToArray(), cancelled, category, code);
    }


    private static string TypeName(Exception error) => error switch
    {
        AmazonServiceException aws => aws.GetType().Name switch
        {
            "ResourceNotFoundException" => "ResourceNotFoundException",
            "AccessDeniedException" => "AccessDeniedException",
            "InvalidRequestException" => "InvalidRequestException",
            "DecryptionFailureException" => "DecryptionFailureException",
            "UnrecognizedClientException" => "UnrecognizedClientException",
            "AmazonSecretsManagerException" => "AmazonSecretsManagerException",
            _ => "AmazonServiceException"
        },
        AmazonClientException => "AmazonClientException",
        TaskCanceledException => "TaskCanceledException",
        OperationCanceledException => "OperationCanceledException",
        UnauthorizedAccessException => "UnauthorizedAccessException",
        FileNotFoundException => "FileNotFoundException",
        DirectoryNotFoundException => "DirectoryNotFoundException",
        IOException => "IOException",
        JsonException => "JsonException",
        HttpRequestException => "HttpRequestException",
        FormatException => "FormatException",
        ArgumentException => "ArgumentException",
        InvalidOperationException => "InvalidOperationException",
        Microsoft.Extensions.Options.OptionsValidationException => "OptionsValidationException",
        System.Security.Cryptography.CryptographicException => "CryptographicException",
        _ => "Exception"
    };

    private static string AwsCode(string? code) => code switch
    {
        "ResourceNotFoundException" => "ResourceNotFoundException",
        "AccessDenied" => "AccessDenied",
        "AccessDeniedException" => "AccessDeniedException",
        "InvalidRequestException" => "InvalidRequestException",
        "DecryptionFailure" => "DecryptionFailure",
        "DecryptionFailureException" => "DecryptionFailureException",
        "UnrecognizedClientException" => "UnrecognizedClientException",
        "InvalidClientTokenId" => "InvalidClientTokenId",
        "ExpiredToken" => "ExpiredToken",
        "ExpiredTokenException" => "ExpiredTokenException",
        "ThrottlingException" => "ThrottlingException",
        _ => "aws-error-unclassified"
    };

    internal static SafeException Controlled(string code) => new("DeliveryFailure", [], false, "validation", code switch
    {
        "usage-invalid" => "usage-invalid",
        "secret-envelope-invalid" => "secret-envelope-invalid",
        "credential-source-forbidden" => "credential-source-forbidden",
        "credential-isolation-required" => "credential-isolation-required",
        "instance-role-mismatch" => "instance-role-mismatch",
        "redis-mapping-invalid" => "redis-mapping-invalid",
        "mongodb-mapping-invalid" => "mongodb-mapping-invalid",
        "jwt-schema-invalid" => "jwt-schema-invalid",
        "schema-invalid" => "schema-invalid",
        "contract-invalid" => "contract-invalid",
        "linux-root-required" => "linux-root-required",
        "linux-required" => "linux-required",
        "swap-not-permitted" => "swap-not-permitted",
        "runtime-directory-invalid" => "runtime-directory-invalid",
        "tmpfs-required" => "tmpfs-required",
        "runtime-file-invalid" => "runtime-file-invalid",
        "runtime-owner-failed" => "runtime-owner-failed",
        "runtime-directory-sync-failed" => "runtime-directory-sync-failed",
        "runtime-permissions-failed" => "runtime-permissions-failed",
        "runtime-stat-failed" => "runtime-stat-failed",
        _ => "validation-failed"
    });
}

internal sealed class DeliveryFailure : Exception
{
    internal SafeException Safe { get; }
    internal DeliveryFailure(string code) : this(SafeException.Controlled(code)) { }
    private DeliveryFailure(SafeException safe) : base(safe.Code) { Safe = safe; }
    internal static DeliveryFailure Wrap(Exception error) => error as DeliveryFailure ?? new(SafeException.Capture(error));
}
