using Enver.API.Configuration;
using Microsoft.Extensions.Options;

namespace Enver.API.Features.Authentication.Validation;

/// <summary>Reglas puras. No conoce stores, hashing, HTTP ni cuotas y nunca conserva el secreto.</summary>
public sealed class AuthenticationInputValidator(InstitutionalEmailNormalizer email, PasswordPolicy password, IOptions<AuthenticationOptions> options)
{
    private readonly int maxName = options.Value.FullNameMaxUnicodeScalars;

    public bool TryNormalizeFullName(string? input, out string? fullName)
    {
        fullName = null;
        if (input is null) return false;
        // Rechazar controles también en los extremos: Trim no debe ocultar un carácter prohibido.
        if (input.Any(char.IsControl)) return false;
        var value = input.Trim();
        if (value.Length == 0 || !UnicodeInput.Fits(value, maxName, maxName * 4, rejectControls: true)) return false;
        fullName = value;
        return true;
    }

    public AuthenticationInputAssessment AssessCreation(string? fullName, string? inputEmail, string? secret)
    {
        var invalid = new List<AuthenticationValidationField>(3);
        var identity = email.TryNormalize(inputEmail, out _);
        if (!identity) invalid.Add(AuthenticationValidationField.Email);
        if (!TryNormalizeFullName(fullName, out _)) invalid.Add(AuthenticationValidationField.FullName);
        if (!password.IsValidForCreation(secret)) invalid.Add(AuthenticationValidationField.Password);
        return new(identity, invalid.AsReadOnly());
    }
}
