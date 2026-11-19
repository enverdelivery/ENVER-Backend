using System.Buffers;
using System.Text;

namespace Enver.API.Features.Authentication.Validation;

/// <summary>Medición estricta y acotada; no reemplaza surrogates ni modifica el string.</summary>
public static class UnicodeInput
{
    public static bool Fits(string value, int maxScalars, int maxUtf8Bytes, bool rejectControls = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var scalars = 0;
        var bytes = 0;
        var remaining = value.AsSpan();
        while (!remaining.IsEmpty)
        {
            if (Rune.DecodeFromUtf16(remaining, out var rune, out var consumed) != OperationStatus.Done) return false;
            if (++scalars > maxScalars || (bytes += rune.Utf8SequenceLength) > maxUtf8Bytes || rejectControls && Rune.IsControl(rune)) return false;
            remaining = remaining[consumed..];
        }
        return true;
    }

    public static int ScalarCount(string value)
    {
        var remaining = value.AsSpan();
        var count = 0;
        while (!remaining.IsEmpty)
        {
            if (Rune.DecodeFromUtf16(remaining, out _, out var consumed) != OperationStatus.Done)
                throw new ArgumentException("Unicode inválido.", nameof(value));
            count++;
            remaining = remaining[consumed..];
        }
        return count;
    }
}
