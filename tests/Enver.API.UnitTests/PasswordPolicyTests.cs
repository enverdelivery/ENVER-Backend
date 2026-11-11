using Enver.API.Configuration;
using Enver.API.Features.Authentication.Validation;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class PasswordPolicyTests
{
    internal static AuthenticationOptions PolicyOptions() => new()
    {
        PasswordCreationMinUnicodeScalars = 12, PasswordMaxUnicodeScalars = 128, PasswordMaxUtf8Bytes = 512,
        FullNameMaxUnicodeScalars = 200, EmailMaxAsciiBytes = 254, EmailLocalPartMaxAsciiBytes = 64, InstitutionalDomain = "uttt.edu.mx"
    };
    private static PasswordPolicy Policy() => new(Options.Create(PolicyOptions()));

    [Theory]
    [InlineData(11, false)]
    [InlineData(12, true)]
    [InlineData(128, true)]
    [InlineData(129, false)]
    public void Creation_scalar_boundaries(int count, bool expected) => Assert.Equal(expected, Policy().IsValidForCreation(new string('x', count)));

    [Fact]
    public void Supplementary_scalars_and_512_513_bytes_are_counted_strictly()
    {
        var boundary = string.Concat(Enumerable.Repeat("\U0001F600", 128));
        Assert.Equal(256, boundary.Length);
        Assert.Equal(128, UnicodeInput.ScalarCount(boundary));
        Assert.True(Policy().IsValidForCreation(boundary));
        // 513 bytes exige al menos 129 escalares: se verifica también el límite de bytes aisladamente.
        Assert.False(Policy().IsValidForCreation(boundary + "x"));
        Assert.False(UnicodeInput.Fits(boundary + "x", 129, 512));
        Assert.True(UnicodeInput.Fits(boundary + "x", 129, 513));
    }

    [Fact]
    public void Invalid_surrogates_are_not_replaced()
    {
        foreach (var invalid in new[] { "\ud800", "\udc00", "\ud800x", "\udc00\ud800" })
        {
            Assert.False(Policy().IsValidForCreation(new string('a', 12) + invalid));
            Assert.False(Policy().IsBoundedForVerification(invalid));
            Assert.Throws<ArgumentException>(() => UnicodeInput.ScalarCount(invalid));
        }
    }

    [Theory]
    [InlineData("  Aaé𝄞 contraseña  ")]
    [InlineData("aaaaaaaaaaaa")]
    [InlineData("            ")]
    [InlineData("a\u0000aaaaaaaaaaa")]
    public void Secret_is_accepted_intact_without_composition_rules(string secret)
    {
        var snapshot = secret;
        Assert.True(Policy().IsValidForCreation(secret));
        Assert.Same(snapshot, secret);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("x", true)]
    public void Verification_does_not_reapply_creation_minimum(string? secret, bool expected) => Assert.Equal(expected, Policy().IsBoundedForVerification(secret));
}
