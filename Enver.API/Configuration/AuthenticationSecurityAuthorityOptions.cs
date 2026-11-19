namespace Enver.API.Configuration;

/// <summary>Configuración declarativa de la autoridad compartida; no contiene región ni credenciales.</summary>
public sealed class AuthenticationSecurityAuthorityOptions
{
    public const string SectionName = "AuthenticationSecurityAuthority";
    public string TableName { get; set; } = string.Empty;
    public int OperationTimeoutMilliseconds { get; set; }
}
