using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enver.QuotaControl;

public enum Failure
{
    InvalidArguments, InvalidDescriptor, InvalidGeneration, InvalidTime,
    UnexpectedResponse, ResidueFound, InventoryIncomplete, ReadbackMismatch,
    MetadataExists, RegistryExists, ApplyDisabled, RemoteTransportDisabled, ResultUncertain,
    AuthTlsFailure, ProtocolFailure, PolicyFailure, CredentialFailure, LocalOperationFailed
}

// No interpolar datos externos, excepciones del transporte ni respuestas en mensajes.
public sealed class ControlException(Failure failure) : Exception(failure.ToString())
{
    public Failure Failure { get; } = failure;
}

public sealed record QuotaRule(string Name, int Limit, long WindowMicroseconds);

public sealed record ApprovedDescriptor(
    string Environment, string ConfigurationVersion, string IdentityReference,
    string PolicyRevision, int Capacity, long MemoryBudgetBytes, QuotaRule[] Rules)
{
    public static ApprovedDescriptor Baseline => new(
        "development", "quota-development-academic-initial-v1", "hmac-development-fecf274e-v1",
        "academic-initial", 5000, 67108864,
        [new("register/email", 3, 900000000), new("register/origin", 20, 900000000),
         new("login/email", 10, 60000000), new("login/origin", 60, 60000000),
         new("login/origin-email", 5, 60000000), new("logout/origin", 300, 60000000),
         new("logout/account", 30, 60000000)]);

    public void Validate()
    {
        var b = Baseline;
        if (Environment != b.Environment || ConfigurationVersion != b.ConfigurationVersion ||
            IdentityReference != b.IdentityReference || PolicyRevision != b.PolicyRevision ||
            Capacity != b.Capacity || MemoryBudgetBytes != b.MemoryBudgetBytes ||
            Rules is null || !Rules.SequenceEqual(b.Rules))
            throw new ControlException(Failure.InvalidDescriptor);
    }

    public string Snapshot()
    {
        Validate();
        return string.Join('|', new[] { Environment, ConfigurationVersion, IdentityReference, PolicyRevision,
            Contract.Decimal(Capacity), Contract.Decimal(MemoryBudgetBytes) }
            .Concat(Rules.SelectMany(r => new[] { Contract.Decimal(r.Limit), Contract.Decimal(r.WindowMicroseconds) })));
    }

    public static ApprovedDescriptor ParseJson(string json)
    {
        try
        {
            if (json.Length > 16384) throw new ControlException(Failure.InvalidDescriptor);
            using var document = JsonDocument.Parse(json, new() { MaxDepth = 8 });
            RejectDuplicateProperties(document.RootElement);
            var value = JsonSerializer.Deserialize<ApprovedDescriptor>(json, new JsonSerializerOptions
            {
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                RespectRequiredConstructorParameters = true,
                MaxDepth = 8
            }) ?? throw new ControlException(Failure.InvalidDescriptor);
            value.Validate();
            return value;
        }
        catch (ControlException) { throw; }
        catch { throw new ControlException(Failure.InvalidDescriptor); }
    }

    private static void RejectDuplicateProperties(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw new ControlException(Failure.InvalidDescriptor);
                RejectDuplicateProperties(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var item in element.EnumerateArray()) RejectDuplicateProperties(item);
    }
}

public static class Contract
{
    public const string Prefix = "enver:{development:auth:ratelimit}:";
    public const string Metadata = Prefix + "metadata";
    public const string Registry = Prefix + "registry";
    public const string BucketPrefix = Prefix + "bucket:";
    public const string BucketPattern = BucketPrefix + "*";
    public const long MaximumExactInteger = 9007199254740991;
    public const long MaximumWindow = 900000000;
    public const string Source = "078130627bc50a9dc2feee5fb699cedbea87dbbb";
    public static IReadOnlyList<string> FieldNames { get; } = Array.AsReadOnly(new[]
    {
        "protocol", "snapshot", "generation", "state", "count", "horizon", "dirty", "version",
        "fence", "drain", "last", "maximumWindow"
    });

    public static string Decimal(long value) => value.ToString(CultureInfo.InvariantCulture);

    public static void ValidateGeneration(string? generation)
    {
        if (generation is not { Length: 32 } || generation.All(c => c == '0') ||
            generation.Any(c => !(c is >= '0' and <= '9' or >= 'a' and <= 'f')))
            throw new ControlException(Failure.InvalidGeneration);
    }

    public static long ParseUnsigned(string value, long maximum = MaximumExactInteger)
    {
        if (value.Length == 0 || (value.Length > 1 && value[0] == '0') ||
            value.Any(c => c is < '0' or > '9') ||
            !long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number > maximum)
            throw new ControlException(Failure.UnexpectedResponse);
        return number;
    }

    public static void ValidateTime(long time)
    {
        if (time <= 0 || time > MaximumExactInteger - MaximumWindow)
            throw new ControlException(Failure.InvalidTime);
    }

    public static bool ValidMember(string member)
    {
        var parts = member.Split(':');
        if (parts.Length != 3 || !ApprovedDescriptor.Baseline.Rules.Any(r => r.Name == parts[0] + "/" + parts[1])) return false;
        var id = parts[2];
        if (id.Length != 43 || id.Any(c => !(c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_'))) return false;
        try
        {
            var bytes = Convert.FromBase64String(id.Replace('-', '+').Replace('_', '/') + "=");
            return bytes.Length == 32 && Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_') == id;
        }
        catch { return false; }
    }
}

public sealed class ClosedPlan
{
    public IReadOnlyDictionary<string, string> Fields { get; }
    public string BaseGenerationStatus => "UNKNOWN";
    public bool OperationallyApproved => false;
    public long ServerTimeMicroseconds { get; }

    private ClosedPlan(Dictionary<string, string> fields, long time)
    {
        Fields = new ReadOnlyDictionary<string, string>(fields);
        ServerTimeMicroseconds = time;
    }

    public static ClosedPlan Build(ApprovedDescriptor descriptor, string generation, long serverTimeMicroseconds)
    {
        descriptor.Validate();
        Contract.ValidateGeneration(generation);
        Contract.ValidateTime(serverTimeMicroseconds);
        string[] values = ["1", descriptor.Snapshot(), generation, "closed", "0", "0", "0", "1",
            Contract.Decimal(serverTimeMicroseconds), Contract.Decimal(serverTimeMicroseconds + Contract.MaximumWindow),
            Contract.Decimal(serverTimeMicroseconds), Contract.Decimal(Contract.MaximumWindow)];
        return new(Contract.FieldNames.Zip(values).ToDictionary(p => p.First, p => p.Second, StringComparer.Ordinal), serverTimeMicroseconds);
    }

    public static ClosedPlan ValidateReadback(ApprovedDescriptor descriptor, string generation,
        IReadOnlyDictionary<string, string> fields, long pttl)
    {
        descriptor.Validate();
        Contract.ValidateGeneration(generation);
        try
        {
            if (pttl != -1 || fields.Count != 12 || Contract.FieldNames.Any(n => !fields.ContainsKey(n)))
                throw new ControlException(Failure.ReadbackMismatch);
            var time = Contract.ParseUnsigned(fields["fence"]);
            var expected = Build(descriptor, generation, time);
            if (expected.Fields.Any(p => fields[p.Key] != p.Value)) throw new ControlException(Failure.ReadbackMismatch);
            return expected;
        }
        catch { throw new ControlException(Failure.ReadbackMismatch); }
    }
}
