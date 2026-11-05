namespace Enver.API.Configuration;

/// <summary>Configuración estructural; no emite ni valida JWT.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Algorithm { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int LifetimeSeconds { get; set; }
    public int? ClockSkewSeconds { get; set; }
    public string ActiveKid { get; set; } = string.Empty;
    public List<JwtSigningKeyOptions?> Keys { get; set; } = [];
}

public sealed class JwtSigningKeyOptions
{
    public string Kid { get; set; } = string.Empty;
    public string KeyBase64 { get; set; } = string.Empty;
}
