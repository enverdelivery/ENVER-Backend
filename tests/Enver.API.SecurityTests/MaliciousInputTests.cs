using Enver.API.Configuration;
using Enver.API.Features.Authentication.Validation;
using Microsoft.Extensions.Options;

namespace Enver.API.SecurityTests;

/// <summary>Validadores puros sin puertos de escritura/hash/emisión; no acredita transporte ni endpoint.</summary>
public sealed class MaliciousInputTests
{
    private static AuthenticationInputValidator Validator()
    {
        var options = Options.Create(new AuthenticationOptions
        {
            InstitutionalDomain = "uttt.edu.mx", EmailMaxAsciiBytes = 254, EmailLocalPartMaxAsciiBytes = 64,
            FullNameMaxUnicodeScalars = 200, PasswordCreationMinUnicodeScalars = 12, PasswordMaxUnicodeScalars = 128, PasswordMaxUtf8Bytes = 512
        });
        return new(new(options), new(options), options);
    }

    [Theory]
    [InlineData("{\"$ne\":null}")]
    [InlineData("a@uttt.edu.mx\r\nInjected: x")]
    [InlineData("a@uttt.edu.mx.evil.invalid")]
    [InlineData("a\u200b@uttt.edu.mx")]
    [InlineData("a@uttt。edu.mx")]
    [InlineData("a@uttt.edu.mx,b@uttt.edu.mx")]
    public void Malicious_identity_is_rejected_without_application_collaborators(string email)
    {
        var result = Validator().AssessCreation("Nombre", email, "synthetic-password");
        Assert.False(result.HasQuotaIdentity);
        Assert.Contains(AuthenticationValidationField.Email, result.InvalidFields);
        Assert.DoesNotContain(email, result.ToString());
    }

    [Fact]
    public void Controls_invalid_scalars_and_oversized_fields_are_rejected_together()
    {
        var invalid = Validator().AssessCreation("name\r\n", new string('x', 10000), new string('x', 128) + "\ud800");
        Assert.Equal(3, invalid.InvalidFields.Count);
        Assert.False(invalid.HasQuotaIdentity);
    }

    [Fact]
    public void Secrecy_policy_does_not_mistake_special_characters_for_injection()
    {
        var result = Validator().AssessCreation("Ána  María", " A.B+Tag@UTTT.EDU.MX ", "{\"$ne\":null}  ");
        Assert.Empty(result.InvalidFields);
        Assert.True(result.HasQuotaIdentity);
    }
}
