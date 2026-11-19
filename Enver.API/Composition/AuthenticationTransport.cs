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
    public CloudFrontTransportOptions CloudFront { get; set; } = new();
}

/// <summary>Opt-in: secreto externo del origen; no depende de IPs de nodos ALB ni habilita HTTP público.</summary>
public sealed class CloudFrontTransportOptions
{
    public bool Enabled { get; set; }
    public string OriginSecret { get; set; } = "";

    internal static bool IsValidSecret(string? value) => value is { Length: >= 32 and <= 256 } &&
        value.All(c => c is >= '!' and <= '~' && c != ',');
}

internal sealed class AuthenticationTransportValidator(IHostEnvironment environment) : IValidateOptions<AuthenticationTransportOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationTransportOptions options)
    {
        var local = environment.IsDevelopment() || environment.IsEnvironment("Testing") ||
            environment.EnvironmentName.StartsWith("T015_", StringComparison.Ordinal);
        return options.HttpsPort is > 0 and <= 65535 && options.KnownProxies.All(p => IPAddress.TryParse(p, out _)) &&
            !(options.DirectConnectionApproved && options.KnownProxies.Length > 0) &&
            (!options.AllowLocalHttp || local && options.DirectConnectionApproved) &&
            (!options.CloudFront.Enabled || !options.AllowLocalHttp && !options.DirectConnectionApproved &&
                CloudFrontTransportOptions.IsValidSecret(options.CloudFront.OriginSecret) &&
                options.KnownProxies.All(p => IPAddress.TryParse(p, out var ip) &&
                    !(ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip).Equals(IPAddress.Any) && !ip.Equals(IPAddress.IPv6Any)))
            ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail("AuthenticationTransport: política incompatible.");
    }
}

public static class AuthenticationTransport
{
    public static IApplicationBuilder UseAuthenticationTransport(this IApplicationBuilder app)
    {
        var policy = app.ApplicationServices.GetRequiredService<IOptions<AuthenticationTransportOptions>>().Value;
        if (policy.CloudFront.Enabled)
        {
            // Este modo autentica el origen antes de interpretar datos CloudFront. No ejecuta
            // el forwarding legacy: ALB puede añadir una lista XFF y XFP=http describe ese salto.
            app.UseCloudFrontTrust(policy);
        }
        else
        {
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
        }
        var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
        if (!policy.AllowLocalHttp)
        {
            if (!environment.IsDevelopment()) app.UseHsts();
            // Solo GET /health exacto conserva liveness HTTP. No habilita HTTP de API,
            // subrutas, otros métodos, PathBase ni excepciones por ambiente/origen.
            app.UseWhen(context => !IsExactHealthRequest(context),
                branch => branch.UseHttpsRedirection());
        }
        return app;
    }

    internal static bool IsExactHealthRequest(HttpContext context) => HttpMethods.IsGet(context.Request.Method) &&
        !context.Request.PathBase.HasValue &&
        string.Equals(context.Request.Path.Value, "/health", StringComparison.Ordinal);

    private sealed record ForwardingCandidate(IPAddress? Origin);
}
