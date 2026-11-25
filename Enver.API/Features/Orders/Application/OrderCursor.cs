using System.Buffers.Binary;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace Enver.API.Features.Orders.Application;

public sealed class OrderCursor
{
    private const int PositionSize = 24, TagSize = 32;
    private readonly byte[] key;

    public OrderCursor(string rootKeyBase64)
    {
        var root = Convert.FromBase64String(rootKeyBase64);
        try { key = HMACSHA256.HashData(root, "enver:orders:cursor:v1"u8); }
        finally { CryptographicOperations.ZeroMemory(root); }
    }

    public string Encode(OrderListing listing, Guid viewer, OrderPosition position)
    {
        Span<byte> bytes = stackalloc byte[PositionSize + TagSize];
        BinaryPrimitives.WriteInt64BigEndian(bytes, position.CreatedAt.Ticks);
        position.Id.TryWriteBytes(bytes[8..]);
        Sign(listing, viewer, bytes[..PositionSize], bytes[PositionSize..]);
        return WebEncoders.Base64UrlEncode(bytes);
    }

    public bool TryDecode(string cursor, OrderListing listing, Guid viewer, out OrderPosition? position)
    {
        position = null;
        if (cursor.Length != 75) return false;
        byte[] bytes;
        try { bytes = WebEncoders.Base64UrlDecode(cursor); }
        catch (FormatException) { return false; }
        if (bytes.Length != PositionSize + TagSize || WebEncoders.Base64UrlEncode(bytes) != cursor) return false;
        Span<byte> expected = stackalloc byte[TagSize];
        Sign(listing, viewer, bytes.AsSpan(0, PositionSize), expected);
        if (!CryptographicOperations.FixedTimeEquals(expected, bytes.AsSpan(PositionSize))) return false;
        var ticks = BinaryPrimitives.ReadInt64BigEndian(bytes);
        if (ticks < 0 || ticks > DateTime.MaxValue.Ticks) return false;
        position = new(new DateTime(ticks, DateTimeKind.Utc), new Guid(bytes.AsSpan(8, 16)));
        return true;
    }

    private void Sign(OrderListing listing, Guid viewer, ReadOnlySpan<byte> position, Span<byte> destination)
    {
        Span<byte> signed = stackalloc byte[1 + 16 + PositionSize];
        signed[0] = (byte)listing;
        viewer.TryWriteBytes(signed[1..]);
        position.CopyTo(signed[17..]);
        HMACSHA256.HashData(key, signed, destination);
    }
}
