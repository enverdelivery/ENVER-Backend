using Microsoft.AspNetCore.Authorization;

namespace Enver.API.Security;

/// <summary>Políticas activadas por 058; no publican recursos ni conceden permisos de negocio.</summary>
public static class AuthenticationPolicies
{
    public const string Scheme = "Bearer";
    public const string Normal = "Authentication.Normal";
    public const string Logout = "Authentication.Logout";

    public static void Configure(AuthorizationOptions options)
    {
        var normal = Build(false);
        options.DefaultPolicy = normal; options.FallbackPolicy = normal;
        options.AddPolicy(Normal, normal); options.AddPolicy(Logout, Build(true));
    }
    private static AuthorizationPolicy Build(bool logout) => new AuthorizationPolicyBuilder(Scheme)
        .RequireAuthenticatedUser().AddRequirements(new RevocationRequirement(logout)).Build();
}

internal sealed record RevocationRequirement(bool AllowRevokedForLogout) : IAuthorizationRequirement;
