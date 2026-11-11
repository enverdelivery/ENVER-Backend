using System.Net;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace Enver.API.Composition;

/// <summary>Política externa de transporte; no describe un LB ni acredita TLS publicado.</summary>
public sealed class AuthenticationTransportOptions
{
    public const string SectionName = "AuthenticationTransport";
    public bool DirectConnectionApproved { get; set; }
    public bool AllowLocalHttp { get; set; }
    public int HttpsPort { get; set; } = 443;
    public string[] KnownProxies { get; set; } = [];
}

internal sealed class AuthenticationTransportValidator(IHostEnvironment environment) : IValidateOptions<AuthenticationTransportOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationTransportOptions options)
    {
        var local = environment.IsDevelopment() || environment.IsEnvironment("Testing") ||
            environment.EnvironmentName.StartsWith("T015_", StringComparison.Ordinal);
        return options.HttpsPort is > 0 and <= 65535 && options.KnownProxies.All(p => IPAddress.TryParse(p, out _)) &&
            !(options.DirectConnectionApproved && options.KnownProxies.Length > 0) &&
            (!options.AllowLocalHttp || local && options.DirectConnectionApproved)
            ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail("AuthenticationTransport: política incompatible.");
    }
}

public static class AuthenticationTransport
{
    public static IApplicationBuilder UseAuthenticationTransport(this IApplicationBuilder app)
    {
        var policy = app.ApplicationServices.GetRequiredService<IOptions<AuthenticationTransportOptions>>().Value;
        var forwarding = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            ForwardLimit = 1,
            RequireHeaderSymmetry = true
        };
        forwarding.KnownProxies.Clear();
        forwarding.KnownIPNetworks.Clear();
        foreach (var proxy in policy.KnownProxies) forwarding.KnownProxies.Add(IPAddress.Parse(proxy));

        if (policy.KnownProxies.Length > 0)
        {
            // Captura solo datos del peer/socket antes del middleware nativo. Nunca autoridad desde headers solos.
            app.Use(async (context, next) =>
            {
                var peer = context.Connection.RemoteIpAddress;
                var fromKnownPeer = peer is not null && forwarding.KnownProxies.Contains(peer.IsIPv4MappedToIPv6 ? peer.MapToIPv4() : peer);
                var forHeader = context.Request.Headers["X-Forwarded-For"];
                var proto = context.Request.Headers["X-Forwarded-Proto"];
                IPAddress? origin = null;
                var candidate = fromKnownPeer && forHeader.Count == 1 &&
                    IPAddress.TryParse(forHeader[0], out origin) && proto.Count == 1 && proto[0] is "http" or "https";
                context.Features.Set(new ForwardingCandidate(candidate ? origin : null));
                await next(context);
            });
            app.UseForwardedHeaders(forwarding);
        }
        app.Use(async (context, next) =>
        {
            AuthenticationOriginTrustContext trust;
            if (policy.DirectConnectionApproved) trust = AuthenticationOriginTrustContext.ApproveDirectConnection();
            else
            {
                var candidate = context.Features.Get<ForwardingCandidate>()?.Origin;
                var verified = candidate is not null && candidate.Equals(context.Connection.RemoteIpAddress) &&
                    context.Request.Headers["X-Forwarded-For"].Count == 0 && context.Request.Headers["X-Forwarded-Proto"].Count == 0;
                trust = AuthenticationOriginTrustContext.RequireVerifiedForwarding(verified ? AuthenticationCanonicalOrigin.FromAddress(candidate!) : null);
            }
            context.Features.Set(trust);
            await next(context);
        });
        var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
        if (!policy.AllowLocalHttp)
        {
            if (!environment.IsDevelopment()) app.UseHsts();
            app.UseHttpsRedirection();
        }
        return app;
    }

    private sealed record ForwardingCandidate(IPAddress? Origin);
}
