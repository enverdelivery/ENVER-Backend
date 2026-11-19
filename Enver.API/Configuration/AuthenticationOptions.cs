namespace Enver.API.Configuration;

/// <summary>Parámetros aprobados, sin validación HTTP ni hashing funcional.</summary>
public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";
    public string InstitutionalDomain { get; set; } = string.Empty;
    public int FullNameMaxUnicodeScalars { get; set; }
    public int EmailMaxAsciiBytes { get; set; }
    public int EmailLocalPartMaxAsciiBytes { get; set; }
    public int PasswordCreationMinUnicodeScalars { get; set; }
    public int PasswordMaxUnicodeScalars { get; set; }
    public int PasswordMaxUtf8Bytes { get; set; }
    public int MaxRequestBodyBytes { get; set; }
    public int MaxJsonDepth { get; set; }
    public int MaxJwtBytes { get; set; }
    public int RegisterBudgetMilliseconds { get; set; }
    public int LoginBudgetMilliseconds { get; set; }
    public int LogoutBudgetMilliseconds { get; set; }
    public int HashingConcurrencyLimit { get; set; }
}
