using Enver.API.Features.Authentication.Validation;
using Microsoft.Extensions.Options;

namespace Enver.API.UnitTests;

public sealed class ValidationTests
{
    private static InstitutionalEmailNormalizer Email() => new(Options.Create(PasswordPolicyTests.PolicyOptions()));
    private static AuthenticationInputValidator Validator()
    {
        var options = Options.Create(PasswordPolicyTests.PolicyOptions());
        return new(new(options), new(options), options);
    }

    [Theory]
    [InlineData("  A.B+Tag@UTTT.EDU.MX  ", "a.b+tag@uttt.edu.mx")]
    [InlineData("a@uttt.edu.mx", "a@uttt.edu.mx")]
    [InlineData("a!#$%&'*+-/=?^_`{|}~@uttt.edu.mx", "a!#$%&'*+-/=?^_`{|}~@uttt.edu.mx")]
    public void Institutional_dot_atom_normalization(string input, string expected)
    {
        Assert.True(Email().TryNormalize(input, out var normalized));
        Assert.Equal(expected, normalized);
        Assert.True(Email().TryNormalize(normalized, out var again));
        Assert.Equal(normalized, again);
    }

    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData("@uttt.edu.mx")]
    [InlineData("a b@uttt.edu.mx")] [InlineData("a\tb@uttt.edu.mx")]
    [InlineData("é@uttt.edu.mx")] [InlineData("a@üttt.edu.mx")]
    [InlineData("\"a\"@uttt.edu.mx")] [InlineData("A <a@uttt.edu.mx>")]
    [InlineData("a(comment)@uttt.edu.mx")] [InlineData("a@uttt.edu.mx,b@uttt.edu.mx")]
    [InlineData("a@uttt.edu.mx;b@uttt.edu.mx")]
    [InlineData(".a@uttt.edu.mx")] [InlineData("a.@uttt.edu.mx")] [InlineData("a..b@uttt.edu.mx")]
    [InlineData("a@sub.uttt.edu.mx")] [InlineData("a@uttt.edu.mx.evil.invalid")]
    [InlineData("a@uttt.edu.mx.")] [InlineData("a@@uttt.edu.mx")]
    [InlineData("a@uttt.edu.mx\u0000")]
    public void Invalid_email_has_no_normalized_identity(string? input)
    {
        Assert.False(Email().TryNormalize(input, out var normalized));
        Assert.Null(normalized);
    }

    [Fact]
    public void Email_lengths_and_equivalences()
    {
        Assert.True(Email().TryNormalize(new string('a', 64) + "@uttt.edu.mx", out _));
        Assert.False(Email().TryNormalize(new string('a', 65) + "@uttt.edu.mx", out _));
        Assert.False(Email().TryNormalize(new string('a', 243) + "@uttt.edu.mx", out _));
        Assert.False(Email().TryNormalize(new string('a', 244) + "@uttt.edu.mx", out _));
        // El dominio fijo + local-part <=64 hace inalcanzable un email institucional válido de 254 bytes.
        Email().TryNormalize(" A.B+Tag@UTTT.EDU.MX ", out var first);
        Email().TryNormalize("a.b+tag@uttt.edu.mx", out var second);
        Assert.Equal(first, second);
        Email().TryNormalize("ab+tag@uttt.edu.mx", out var noDot);
        Email().TryNormalize("a.b@uttt.edu.mx", out var noTag);
        Assert.NotEqual(first, noDot);
        Assert.NotEqual(first, noTag);
    }

    [Theory]
    [InlineData("  Ána  María  ", "Ána  María")]
    [InlineData("X", "X")]
    public void Name_retains_cultural_formatting(string input, string expected)
    {
        Assert.True(Validator().TryNormalizeFullName(input, out var name));
        Assert.Equal(expected, name);
    }

    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData("   ")]
    [InlineData("\tAna")] [InlineData("Ana\n")] [InlineData("Ana\u007f")]
    [InlineData("Ana\u0085")]
    public void Invalid_names(string? input) => Assert.False(Validator().TryNormalizeFullName(input, out _));

    [Fact]
    public void Name_scalar_boundary_and_invalid_unicode()
    {
        var name = string.Concat(Enumerable.Repeat("\U0001F600", 200));
        Assert.True(Validator().TryNormalizeFullName(name, out _));
        Assert.False(Validator().TryNormalizeFullName(name + "a", out _));
        Assert.False(Validator().TryNormalizeFullName("Ana\ud800", out _));
    }

    [Fact]
    public void Field_results_never_include_values_and_keep_quota_identity_independent()
    {
        var result = Validator().AssessCreation("", "a@uttt.edu.mx", "short");
        Assert.True(result.HasQuotaIdentity);
        Assert.Equal(new[] { AuthenticationValidationField.FullName, AuthenticationValidationField.Password }, result.InvalidFields);
        Assert.DoesNotContain("short", result.ToString());
        Assert.False(Validator().AssessCreation("Ana", "invalid", "aaaaaaaaaaaa").HasQuotaIdentity);
    }
}
