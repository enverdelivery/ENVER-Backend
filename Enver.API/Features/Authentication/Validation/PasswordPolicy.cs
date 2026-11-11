using Enver.API.Configuration;
using Microsoft.Extensions.Options;

namespace Enver.API.Features.Authentication.Validation;

public sealed class PasswordPolicy(IOptions<AuthenticationOptions> options)
{
    private readonly int minimum = options.Value.PasswordCreationMinUnicodeScalars;
    private readonly int maximum = options.Value.PasswordMaxUnicodeScalars;
    private readonly int bytes = options.Value.PasswordMaxUtf8Bytes;

    public bool IsValidForCreation(string? secret) => IsBoundedForVerification(secret) && UnicodeInput.ScalarCount(secret!) >= minimum;
    public bool IsBoundedForVerification(string? secret) => !string.IsNullOrEmpty(secret) && UnicodeInput.Fits(secret, maximum, bytes);
}
