namespace Enver.API.Configuration;

/// <summary>Las siete cuotas declaradas; sin buckets o middleware distribuido.</summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    public RateLimitQuotaOptions? RegisterEmail { get; set; }
    public RateLimitQuotaOptions? RegisterOrigin { get; set; }
    public RateLimitQuotaOptions? LoginOriginEmail { get; set; }
    public RateLimitQuotaOptions? LoginEmail { get; set; }
    public RateLimitQuotaOptions? LoginOrigin { get; set; }
    public RateLimitQuotaOptions? LogoutAccount { get; set; }
    public RateLimitQuotaOptions? LogoutOrigin { get; set; }
    public string PolicyRevision { get; set; } = string.Empty;
    public string PseudonymizationKeyBase64 { get; set; } = string.Empty;
    public int? MaxActivePartitions { get; set; }
}

public sealed class RateLimitQuotaOptions
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
}
