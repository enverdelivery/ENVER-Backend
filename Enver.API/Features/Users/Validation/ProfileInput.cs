using System.Text.Json;
using Enver.API.Features.Authentication.Validation;
using Enver.API.Features.Users.Core;

namespace Enver.API.Features.Users.Validation;

internal static class ProfileInput
{
    internal static ProfileChange? Parse(JsonElement input, bool selectMode, AuthenticationInputValidator names)
    {
        if (input.ValueKind != JsonValueKind.Object) return null;
        string? name = null, phone = null;
        bool hasPhone = false;
        ActiveRole? role = null;
        foreach (var property in input.EnumerateObject())
        {
            switch (property.Name)
            {
                case "activeRole" when selectMode && property.Value.ValueKind == JsonValueKind.String:
                    role = property.Value.GetString() switch
                    {
                        "Solicitante" => ActiveRole.Solicitante,
                        "Rescatista" => ActiveRole.Rescatista,
                        _ => null
                    };
                    if (role is null) return null;
                    break;
                case "fullName" when !selectMode && property.Value.ValueKind == JsonValueKind.String:
                    if (!names.TryNormalizeFullName(property.Value.GetString(), out name)) return null;
                    break;
                case "phone" when !selectMode:
                    hasPhone = true;
                    if (property.Value.ValueKind == JsonValueKind.Null) break;
                    if (property.Value.ValueKind != JsonValueKind.String) return null;
                    phone = property.Value.GetString();
                    if (!IsValidPhone(phone)) return null;
                    break;
                default: return null;
            }
        }
        return name is null && !hasPhone && role is null ? null : new(name, hasPhone, phone, role);
    }

    internal static bool IsValidPhone(string? phone) =>
        phone is { Length: >= 3 and <= 16 } && phone[0] == '+' &&
        phone[1] is >= '1' and <= '9' && phone.Skip(2).All(char.IsAsciiDigit);
}
