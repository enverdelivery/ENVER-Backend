using System.Globalization;
using System.Text.Json;
using Enver.API.Features.Orders.Core;

namespace Enver.API.Features.Orders.Contracts;

public static class OrderInput
{
    public static bool TryId(string? text, out Guid id) => Guid.TryParseExact(text, "D", out id) && text!.Length == 36 && id != Guid.Empty;

    public static OrderDraft? Parse(JsonElement json)
    {
        if (json.ValueKind != JsonValueKind.Object) return null;
        OrderCategory? category = null;
        DeliveryPreference? delivery = null;
        string? title = null, instructions = null, detail = null, address = null, habitual = null;
        decimal amount = 0;
        double? lat = null, lng = null;
        Guid? media = null;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in json.EnumerateObject())
        {
            if (!seen.Add(property.Name)) return null;
            var value = property.Value;
            switch (property.Name)
            {
                case "selectedCategory" when value.ValueKind == JsonValueKind.String:
                    category = Enum.GetValues<OrderCategory>().SingleOrDefault(x => OrderLabels.Category(x) == value.GetString()); break;
                case "deliveryTime" when value.ValueKind == JsonValueKind.String:
                    delivery = Enum.GetValues<DeliveryPreference>().SingleOrDefault(x => OrderLabels.Delivery(x) == value.GetString()); break;
                case "orderTitle" when Text(value, 120, out title): break;
                case "specialInstructions" when Text(value, 500, out instructions): break;
                case "locationSearch" when Text(value, 200, out _): break; // Solo entrada de UI.
                case "locationDetail" when Text(value, 200, out detail): break;
                case "selectedAddressLabel" when Text(value, 200, out address): break;
                case "selectedHabitualPlace" when Text(value, 200, out habitual): break;
                case "tarifa" when value.ValueKind == JsonValueKind.String && TryAmount(value.GetString(), out amount): break;
                case "selectedLat" when Coordinate(value, 90, out lat): break;
                case "selectedLng" when Coordinate(value, 180, out lng): break;
                case "mediaId" when value.ValueKind == JsonValueKind.Null: break;
                case "mediaId" when value.ValueKind == JsonValueKind.String && TryId(value.GetString(), out var id): media = id; break;
                default: return null;
            }
        }
        if (category is not { } c || c == 0 || delivery is not { } d || d == 0 || title is null || amount <= 0 ||
            (detail is null && address is null) || lat.HasValue != lng.HasValue) return null;
        return new(c, title, instructions, amount, new(detail, lat, lng, address, habitual), d, media);
    }

    private static bool Text(JsonElement value, int limit, out string? text)
    {
        text = null;
        if (value.ValueKind == JsonValueKind.Null) return true;
        if (value.ValueKind != JsonValueKind.String) return false;
        var raw = value.GetString()!;
        if (raw.Any(char.IsControl)) return false;
        var normalized = raw.Trim();
        if (normalized.EnumerateRunes().Count() > limit) return false;
        text = normalized.Length == 0 ? null : normalized;
        return true;
    }

    private static bool Coordinate(JsonElement value, double bound, out double? coordinate)
    {
        coordinate = null;
        if (value.ValueKind == JsonValueKind.Null) return true;
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out var number) || !double.IsFinite(number) ||
            number < -bound || number > bound) return false;
        coordinate = number == 0 ? 0 : number; // Normaliza -0 para idempotencia.
        return true;
    }

    private static bool TryAmount(string? text, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrEmpty(text)) return false;
        var parts = text.Split('.');
        if (parts.Length > 2 || parts[0].Length == 0 || (parts.Length == 2 && parts[1].Length is < 1 or > 2) ||
            parts.Any(part => !part.All(char.IsAsciiDigit))) return false;
        var whole = parts[0].TrimStart('0');
        var canonical = (whole.Length == 0 ? "0" : whole) + "." + (parts.Length == 2 ? parts[1].PadRight(2, '0') : "00");
        // Decimal.TryParse puede redondear entradas grandes: rechazar cualquier pérdida de precisión.
        return decimal.TryParse(canonical, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount) &&
            amount > 0 && OrderLabels.Cost(amount) == canonical;
    }
}
