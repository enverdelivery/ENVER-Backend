using Enver.API.Configuration;
using Microsoft.Extensions.Options;

namespace Enver.API.Features.Authentication.Validation;

public sealed class InstitutionalEmailNormalizer(IOptions<AuthenticationOptions> options)
{
    private readonly string domain = options.Value.InstitutionalDomain;
    private readonly int maxTotal = options.Value.EmailMaxAsciiBytes;
    private readonly int maxLocal = options.Value.EmailLocalPartMaxAsciiBytes;

    public bool TryNormalize(string? input, out string? normalizedEmail)
    {
        normalizedEmail = null;
        if (input is null || input.Any(char.IsControl)) return false;
        var value = input.AsSpan().Trim();
        if (value.Length == 0 || value.Length > maxTotal) return false;
        var at = value.IndexOf('@');
        if (at <= 0 || at > maxLocal || !value[(at + 1)..].Equals(domain, StringComparison.OrdinalIgnoreCase)) return false;
        var local = value[..at];
        if (local[0] == '.' || local[^1] == '.') return false;
        for (var i = 0; i < local.Length; i++)
        {
            var c = local[i];
            // Dot-atom ASCII no entrecomillado, sin parser permisivo MailAddress.
            if (!(char.IsAsciiLetterOrDigit(c) || "!#$%&'*+-/=?^_`{|}~.".Contains(c)) || c == '.' && i > 0 && local[i - 1] == '.') return false;
        }
        normalizedEmail = value.ToString().ToLowerInvariant();
        return true;
    }
}
