using System.Text.Json;

namespace Enver.API.Features.Users.Validation;

public sealed record RescuerEnrollmentInput(string RulesVersion)
{
    public static RescuerEnrollmentInput? Parse(JsonElement input)
    {
        if (input.ValueKind != JsonValueKind.Object) return null;
        var accepted = false;
        string? version = null;
        foreach (var property in input.EnumerateObject())
        {
            switch (property.Name)
            {
                case "acceptedRules" when !accepted && property.Value.ValueKind == JsonValueKind.True:
                    accepted = true;
                    break;
                case "rulesVersion" when version is null && property.Value.ValueKind == JsonValueKind.String:
                    version = property.Value.GetString();
                    if (!IsVersion(version)) return null;
                    break;
                default: return null;
            }
        }
        return accepted && version is not null ? new(version) : null;
    }

    internal static bool IsVersion(string? value) =>
        value is { Length: >= 1 and <= 64 } && char.IsAsciiLetterOrDigit(value[0]) &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-');
}
