using System.Security.Cryptography;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Enver.QuotaControl.PipeProducer;

// Proyección tipada, no logs/raw stdout. No nombres de buckets, campos HGETALL o errores del servidor.
public sealed record InventoryEvidence(string RunId, DateTimeOffset StartedUtc, DateTimeOffset FinishedUtc,
    int Pages, int UniqueKeys, int DuplicateKeys, string? FinalCursor, bool Completed, long? ServerTimeMicroseconds,
    bool SessionCoherent, bool? MetadataPresent, bool? RegistryPresent, int? BucketCount, int? Discrepancies,
    int? RegistryMemberCount, bool? IterationCompleted, DateTimeOffset? ScanStartedUtc, DateTimeOffset? ScanFinishedUtc,
    string? ResolvedTarget, string? TlsProtocol, string? Cipher)
{
    internal static byte[] ReadBounded(Stream pipe)
    {
        var buffer = new byte[65537];
        var count = 0;
        try
        {
            while (true)
            {
                var n = pipe.Read(buffer, count, buffer.Length - count);
                if (n == 0) break;
                count += n;
                if (count > 65536) throw new ProducerException(ProducerFailure.InvalidOutput);
            }
            return buffer.AsSpan(0, count).ToArray();
        }
        catch { throw new ProducerException(ProducerFailure.InvalidOutput); }
        finally { CryptographicOperations.ZeroMemory(buffer); }
    }

    internal static InventoryEvidence Parse(byte[] bytes, int exit, ReadOnlySpan<byte> secret)
    {
        try
        {
            CredentialProducer.NoExposure(secret, new[] { Encoding.UTF8.GetString(bytes) });
            using var doc = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 8 });
            RejectDecodedSecret(doc.RootElement, secret);
            var root = doc.RootElement;
            if (root.GetProperty("schema").GetString() != "enver.quota-control/2" ||
                root.GetProperty("mode").GetString() != "inventory-remote" || root.GetProperty("command").GetString() != "inventory" ||
                root.GetProperty("logicalEndpoint").GetString() != "master.enver-dev-valkey.kb2qe0.use1.cache.amazonaws.com" ||
                root.GetProperty("database").GetInt32() != 0 || root.GetProperty("exitCode").GetInt32() != exit ||
                exit is not (0 or 10 or 20 or 30 or 40 or 50 or 60 or 70) ||
                root.GetProperty("operationallyApproved").GetBoolean() || root.GetProperty("admissionOpened").GetBoolean() ||
                root.GetProperty("exclusionAccreditedByTool").GetBoolean()) Bad();
            var trace = root.GetProperty("execution");
            if (trace.GetProperty("initialCursor").GetString() != "0") Bad();
            var id = trace.GetProperty("runId").GetString()!;
            if (id.Length > 80 || !Regex.IsMatch(id, "^control-[0-9]{8}T[0-9]{13}Z-[1-9][0-9]*$", RegexOptions.CultureInvariant)) Bad();
            var cursor = trace.GetProperty("finalCursor");
            var finalCursor = cursor.ValueKind == JsonValueKind.Null ? null : cursor.GetString();
            if (finalCursor is not null && (finalCursor.Length > 20 || !ulong.TryParse(finalCursor,
                System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out _))) Bad();
            var summary = root.GetProperty("inventory");
            var present = summary.ValueKind != JsonValueKind.Null;
            var time = trace.GetProperty("serverTimeMicroseconds");
            long? serverTime = time.ValueKind == JsonValueKind.Null ? null : time.GetInt64();
            if (serverTime <= 0) Bad();
            var connection = root.GetProperty("connection");
            string? resolved = null, protocol = null, cipher = null;
            if (connection.ValueKind != JsonValueKind.Null)
            {
                if (connection.GetProperty("logicalEndpoint").GetString() != "master.enver-dev-valkey.kb2qe0.use1.cache.amazonaws.com" ||
                    connection.GetProperty("topologyScope").GetString() != "SINGLE_PINNED_TLS_CONNECTION_NOT_INDEPENDENT_PRIMARY_ATTESTATION" ||
                    !IPEndPoint.TryParse(connection.GetProperty("resolvedTarget").GetString() ?? string.Empty, out var target) ||
                    !IPAddress.IsLoopback(target.Address) || target.Port != 16379) Bad();
                resolved = connection.GetProperty("resolvedTarget").GetString();
                protocol = connection.GetProperty("tlsProtocol").GetString();
                cipher = connection.GetProperty("cipher").GetString();
                if (protocol is not ("Tls12" or "Tls13") || !Enum.TryParse<TlsCipherSuite>(cipher, out var suite) || !Enum.IsDefined(suite)) Bad();
            }
            var result = new InventoryEvidence(id, trace.GetProperty("startedUtc").GetDateTimeOffset(), trace.GetProperty("finishedUtc").GetDateTimeOffset(),
                Count(trace, "pages"), Count(trace, "uniqueKeys"), Count(trace, "duplicateKeys"), finalCursor,
                trace.GetProperty("completed").GetBoolean(), serverTime, root.GetProperty("sessionCoherent").GetBoolean(),
                present ? summary.GetProperty("metadataPresent").GetBoolean() : null,
                present ? summary.GetProperty("registryPresent").GetBoolean() : null,
                present ? Count(summary, "bucketCount") : null, present ? Count(summary, "discrepancies") : null,
                present ? Count(summary, "registryMemberCount") : null, present ? summary.GetProperty("iterationCompleted").GetBoolean() : null,
                NullableDate(trace.GetProperty("scanStartedUtc")), NullableDate(trace.GetProperty("scanFinishedUtc")), resolved, protocol, cipher);
            if (result.FinishedUtc < result.StartedUtc || (exit == 0 &&
                (result.Pages < 1 || result.DuplicateKeys != 0 || result.FinalCursor != "0" || !result.Completed ||
                 !result.SessionCoherent || result.MetadataPresent != false || result.RegistryPresent != false ||
                 result.BucketCount != 0 || result.Discrepancies != 0 || result.RegistryMemberCount != 0 ||
                 result.IterationCompleted != true || result.ServerTimeMicroseconds is null || resolved is null ||
                 result.ScanStartedUtc is null || result.ScanFinishedUtc is null))) Bad();
            return result;
        }
        catch { throw new ProducerException(ProducerFailure.InvalidOutput); }
    }

    private static int Count(JsonElement value, string key)
    {
        var n = value.GetProperty(key).GetInt32();
        if (n < 0) Bad();
        return n;
    }
    private static DateTimeOffset? NullableDate(JsonElement value) => value.ValueKind == JsonValueKind.Null ? null : value.GetDateTimeOffset();
    private static void RejectDecodedSecret(JsonElement value, ReadOnlySpan<byte> secret)
    {
        if (value.ValueKind == JsonValueKind.String) CredentialProducer.NoExposure(secret, new[] { value.GetString()! });
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (var item in value.EnumerateArray()) RejectDecodedSecret(item, secret);
        else if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in value.EnumerateObject())
            {
                if (!names.Add(item.Name)) Bad();
                CredentialProducer.NoExposure(secret, new[] { item.Name });
                RejectDecodedSecret(item.Value, secret);
            }
        }
    }
    private static void Bad() => throw new ProducerException(ProducerFailure.InvalidOutput);
}
