namespace Enver.QuotaControl.Bootstrap;

internal sealed record BootstrapArguments(
    string Endpoint,
    string Generation,
    string AuthorityTable,
    string Region,
    string? Profile,
    int TimeoutSeconds)
{
    public bool PreflightOnly { get; init; }
    public BootstrapRequest ToRequest() => new(Endpoint, Generation, AuthorityTable, Region, TimeoutSeconds);
}

internal sealed record BootstrapParseResult(BootstrapArguments? Arguments, string? Error)
{
    public bool Success => Arguments is not null && Error is null;
}

internal static class BootstrapCommandLine
{
    private static readonly string[] ForbiddenSecretArguments =
    [
        "--password", "--username", "--secret", "--secret-string", "--connection-string",
        "--token", "--api-key", "--credential", "--redis-password"
    ];

    public static BootstrapParseResult Parse(IReadOnlyList<string> args)
    {
        foreach (var argument in args)
        {
            if (ForbiddenSecretArguments.Contains(argument, StringComparer.OrdinalIgnoreCase))
            {
                return new(null, "secret-argument-forbidden");
            }
        }

        string? endpoint = null, generation = null, authorityTable = null, region = null, profile = null;
        var execute = false;
        var preflightOnly = false;
        var secretStdin = false;
        var timeoutSeconds = 30;

        for (var index = 0; index < args.Count; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--execute-initialize":
                    if (execute) return new(null, "duplicate-argument");
                    execute = true;
                    break;
                case "--preflight-only":
                    if (preflightOnly) return new(null, "duplicate-argument");
                    preflightOnly = true;
                    break;
                case "--secret-stdin":
                    if (secretStdin) return new(null, "duplicate-argument");
                    secretStdin = true;
                    break;
                case "--endpoint":
                    if (!TryReadValue(args, ref index, out endpoint)) return new(null, "endpoint-required");
                    break;
                case "--generation":
                    if (!TryReadValue(args, ref index, out generation)) return new(null, "generation-required");
                    break;
                case "--authority-table":
                    if (!TryReadValue(args, ref index, out authorityTable)) return new(null, "authority-table-required");
                    break;
                case "--region":
                    if (!TryReadValue(args, ref index, out region)) return new(null, "region-required");
                    break;
                case "--profile":
                    if (!TryReadValue(args, ref index, out profile)) return new(null, "profile-required");
                    break;
                case "--timeout-seconds":
                    if (!TryReadValue(args, ref index, out var timeoutText) ||
                        !int.TryParse(timeoutText, out timeoutSeconds) ||
                        timeoutSeconds is < 5 or > 300)
                    {
                        return new(null, "timeout-invalid");
                    }
                    break;
                default:
                    return new(null, "unknown-argument");
            }
        }

        if (execute && preflightOnly) return new(null, "execution-mode-conflict");
        if (!execute && !preflightOnly) return new(null, "execute-initialize-required");
        if (!secretStdin) return new(null, "secret-stdin-required");
        if (endpoint is null) return new(null, "endpoint-required");
        if (generation is null) return new(null, "generation-required");
        if (authorityTable is null) return new(null, "authority-table-required");
        if (region is null) return new(null, "region-required");

        if (!IsNewV2Endpoint(endpoint)) return new(null, "endpoint-not-new-v2");
        try
        {
            BootstrapContract.ValidateGeneration(generation);
        }
        catch (BootstrapValidationException)
        {
            return new(null, "generation-invalid");
        }

        if (!string.Equals(authorityTable, BootstrapContract.AuthorityTable, StringComparison.Ordinal))
        {
            return new(null, "authority-table-not-approved");
        }

        if (!string.Equals(region, BootstrapContract.Region, StringComparison.Ordinal))
        {
            return new(null, "region-not-approved");
        }

        if (profile is not null && !IsSafeProfile(profile)) return new(null, "profile-invalid");

        return new(new BootstrapArguments(endpoint, generation, authorityTable, region, profile, timeoutSeconds)
            { PreflightOnly = preflightOnly }, null);
    }

    private static bool TryReadValue(IReadOnlyList<string> args, ref int index, out string? value)
    {
        if (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            value = null;
            return false;
        }

        value = args[++index];
        return true;
    }

    private static bool IsNewV2Endpoint(string endpoint)
    {
        if (endpoint.Length is 0 or > 255 || endpoint.Any(char.IsWhiteSpace) ||
            endpoint.Any(c => c is ':' or '/' or '@' or '?' or '#'))
        {
            return false;
        }

        if (string.Equals(endpoint, BootstrapContract.OldEndpoint, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!endpoint.EndsWith(".cache.amazonaws.com", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var parts = endpoint.Split('.');
        return parts.Length >= 4 &&
            string.Equals(parts[0], "master", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(parts[1], BootstrapContract.NewReplicationGroupId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSafeProfile(string profile) =>
        profile.Length is > 0 and <= 128 && profile.All(c =>
            char.IsAsciiLetterOrDigit(c) || c is '_' or '+' or '=' or ',' or '.' or '@' or '-');
}
