namespace Enver.API.Configuration;

/// <summary>Configuración declarativa; no configura ni verifica Redis.</summary>
public sealed class RedisOptions
{
    public const string SectionName = "Redis";
    public string ConnectionString { get; set; } = string.Empty;
    public string NamespacePrefix { get; set; } = string.Empty;
    public string RevocationPurpose { get; set; } = string.Empty;
    public string RateLimitingPurpose { get; set; } = string.Empty;
    public int OperationTimeoutMilliseconds { get; set; }
    public int ConnectionTimeoutMilliseconds { get; set; }
    public int RevocationOperationalMarginSeconds { get; set; }
    public bool FailClosed { get; set; }
    public string RequiredRevocationEvictionPolicy { get; set; } = string.Empty;
}
