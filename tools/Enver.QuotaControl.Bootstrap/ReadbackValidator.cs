using System.Globalization;

namespace Enver.QuotaControl.Bootstrap;

internal sealed record QuotaReadbackInput(
    string MetadataType,
    string? RegistryType,
    long? MetadataLength,
    long? MetadataTtl,
    IReadOnlyDictionary<string, string?> Metadata,
    long? RegistryTtl,
    long KeyspaceTotal,
    long Db0Keys);

/// <summary>Compara solo valores esperados no secretos; no corrige ni limpia Redis.</summary>
internal static class ReadbackValidator
{
    public static ReadbackResult Validate(QuotaReadbackInput input, string generation)
    {
        var mismatches = new List<string>();

        if (!string.Equals(input.MetadataType, "hash", StringComparison.Ordinal)) mismatches.Add("metadata-type");
        if (input.MetadataLength != 12) mismatches.Add("hlen");
        if (input.MetadataTtl != -1) mismatches.Add("metadata-pttl");

        foreach (var (field, expected) in ExpectedFields(generation))
        {
            if (field == "last")
            {
                if (!input.Metadata.TryGetValue(field, out var lastText) ||
                    !long.TryParse(lastText, NumberStyles.None, CultureInfo.InvariantCulture, out var last) ||
                    last <= 0)
                {
                    mismatches.Add("last");
                }

                continue;
            }

            if (!input.Metadata.TryGetValue(field, out var actual) ||
                !string.Equals(actual, expected, StringComparison.Ordinal))
            {
                mismatches.Add(field);
            }
        }

        if (!string.Equals(input.RegistryType, "none", StringComparison.Ordinal)) mismatches.Add("registry-type");
        if (input.RegistryTtl != -2) mismatches.Add("registry-pttl");
        if (input.KeyspaceTotal != 1 || input.Db0Keys != 1) mismatches.Add("keyspace");

        return mismatches.Count == 0 ? ReadbackResult.Success() : ReadbackResult.Failure(mismatches.ToArray());
    }

    internal static IReadOnlyList<(string Field, string? Expected)> ExpectedFields(string generation) =>
    [
        ("protocol", "1"),
        ("snapshot", BootstrapContract.SnapshotText),
        ("generation", generation),
        ("state", "ready"),
        ("count", "0"),
        ("horizon", "0"),
        ("dirty", "0"),
        ("version", "1"),
        ("fence", "0"),
        ("drain", "0"),
        ("last", null),
        ("maximumWindow", BootstrapContract.MaximumWindowMicroseconds.ToString(CultureInfo.InvariantCulture))
    ];
}
