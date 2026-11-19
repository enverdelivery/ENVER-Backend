using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using Enver.API.Composition;
using Enver.API.Http;
using Enver.API.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Enver.API.UnitTests;

/// <summary>Pipeline ASP.NET real en memoria; terminal sintético, sin AUTH real, sockets ni AWS.</summary>
public sealed class CloudFrontTransportTests
{
    private const string OriginHeader = "X-Enver-Origin-Verify";
    private const string ProtoHeader = "CloudFront-Forwarded-Proto";
    private const string AddressHeader = "CloudFront-Viewer-Address";
    private const string OriginHost = "enver-dev-alb-1073601258.us-east-1.elb.amazonaws.com";
    private const string Peer = "192.0.2.10"; // Peer sintético, no descubrimiento/topología AWS.
    private const string RootKey = "AuthenticationTransport:";

    [Fact]
    public void Defaults_disable_cloudfront_and_do_not_ship_an_origin_secret()
    {
        var options = new AuthenticationTransportOptions();
        Assert.False(options.CloudFront.Enabled);
        Assert.Empty(options.CloudFront.OriginSecret);
        Assert.False(options.AllowLocalHttp);
        Assert.False(options.DirectConnectionApproved);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task Disabled_ignores_cloudfront_headers_and_preserves_http_redirect(string environment)
    {
        using var pipeline = new Pipeline(enabled: false, environment: environment, knownProxy: false);
        var context = await pipeline.Request();
        Assert.Equal(307, context.Response.StatusCode);
        Assert.Equal("http", context.Request.Scheme);
        Assert.Equal("https://" + OriginHost + "/api/v1/auth/register", context.Response.Headers.Location.ToString());
        Assert.False(pipeline.EndpointReached);
        Assert.Equal(AuthenticationOriginStatus.Unavailable, pipeline.Origin(context).Status);
    }

    [Theory]
    [InlineData("198.51.100.25", "https", 204, true)]
    [InlineData("198.51.100.25, 192.0.2.30", "https", 307, false)]
    [InlineData("198.51.100.25", "http", 307, true)]
    public async Task Disabled_preserves_existing_explicit_proxy_forwarding(string forwardedFor, string proto, int status, bool originAvailable)
    {
        using var pipeline = new Pipeline(enabled: false, knownProxy: true);
        var context = await pipeline.Request(change: c =>
        {
            c.Request.Headers[OriginHeader] = "untrusted-client-value";
            c.Request.Headers["X-Forwarded-For"] = forwardedFor;
            c.Request.Headers["X-Forwarded-Proto"] = proto;
        });
        Assert.Equal(status, context.Response.StatusCode);
        Assert.Equal(originAvailable ? AuthenticationOriginStatus.Available : AuthenticationOriginStatus.Unavailable,
            pipeline.Origin(context).Status);
        Assert.Equal(status == 204 ? "https" : "http", context.Request.Scheme);
    }

    [Theory]
    [InlineData("198.51.100.20:49152", "198.51.100.20", 49152)]
    [InlineData("[2001:db8::20]:49152", "2001:db8::20", 49152)]
    [InlineData("[::ffff:198.51.100.20]:65535", "198.51.100.20", 65535)]
    public async Task Authenticated_cloudfront_sets_scheme_and_real_viewer_before_redirect(string value, string ip, int port)
    {
        using var pipeline = new Pipeline();
        var context = await pipeline.Request(change: c => c.Request.Headers[AddressHeader] = value);
        Assert.Equal(204, context.Response.StatusCode);
        Assert.Equal("https", context.Request.Scheme);
        Assert.Equal(IPAddress.Parse(ip), context.Connection.RemoteIpAddress);
        Assert.Equal(port, context.Connection.RemotePort);
        Assert.Equal(OriginHost, context.Request.Host.Value);
        Assert.Equal(0, context.Response.Headers.Location.Count);
        Assert.Equal(AuthenticationOriginStatus.Available, pipeline.Origin(context).Status);
        Assert.False(context.Request.Headers.ContainsKey(OriginHeader));
        Assert.True(pipeline.EndpointReached);
        Assert.False(pipeline.Logs.ToString().Contains(pipeline.Secret, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("register")]
    [InlineData("login")]
    [InlineData("logout")]
    public async Task All_auth_routes_reach_synthetic_terminal_without_https_redirect(string route)
    {
        using var pipeline = new Pipeline();
        var context = await pipeline.Request(path: "/api/v1/auth/" + route);
        Assert.Equal(204, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Headers.Location.Count);
        Assert.True(pipeline.EndpointReached);
        Assert.False(pipeline.Options.AllowLocalHttp);
        Assert.False(pipeline.Options.DirectConnectionApproved);
        Assert.Contains("Strict-Transport-Security", context.Response.Headers.Keys);
    }

    [Theory]
    [InlineData("missing-secret")]
    [InlineData("empty-secret")]
    [InlineData("whitespace-secret")]
    [InlineData("wrong-secret")]
    [InlineData("duplicate-secret")]
    [InlineData("comma-secret")]
    [InlineData("oversized-secret")]
    [InlineData("missing-proto")]
    [InlineData("empty-proto")]
    [InlineData("http-proto")]
    [InlineData("uppercase-proto")]
    [InlineData("whitespace-proto")]
    [InlineData("duplicate-proto")]
    [InlineData("comma-proto")]
    [InlineData("missing-address")]
    [InlineData("duplicate-address")]
    public async Task Invalid_origin_fails_closed_before_endpoints_without_echoing_secrets(string invalid)
    {
        using var pipeline = new Pipeline();
        var context = await pipeline.Request(change: c => Invalidate(c, pipeline.Secret, invalid));
        await AssertRejected(pipeline, context);
        Assert.Equal("http", context.Request.Scheme);
    }

    [Theory]
    [InlineData("")]
    [InlineData("198.51.100.20")]
    [InlineData("198.51.100.20:0")]
    [InlineData("198.51.100.20:65536")]
    [InlineData("198.51.100.20:-1")]
    [InlineData("198.51.100.20:+443")]
    [InlineData("198.51.100.20:443x")]
    [InlineData("198.51.100.20:")]
    [InlineData("198.51.100.20:443:444")]
    [InlineData(" 198.51.100.20:443")]
    [InlineData("198.51.100.20:443 ")]
    [InlineData("198.51.100.20:443,198.51.100.21:443")]
    [InlineData("127.1:443")]
    [InlineData("0x7f000001:443")]
    [InlineData("010.0.0.1:443")]
    [InlineData("2130706433:443")]
    [InlineData("viewer.invalid:443")]
    [InlineData("[198.51.100.20]:443")]
    [InlineData("2001:db8::20:443")]
    [InlineData("[2001:db8::20]")]
    [InlineData("[2001:db8::20]443")]
    [InlineData("[fe80::1%12]:443")]
    [InlineData("[::ffff:198.51.100.20%0]:443")]
    [InlineData("[2001:db8::20]:443,https")]
    [InlineData("[2001:db8::20]:999999999999999999999")]
    public async Task Viewer_address_must_be_one_unambiguous_ip_and_port(string value)
    {
        using var pipeline = new Pipeline();
        var context = await pipeline.Request(change: c => c.Request.Headers[AddressHeader] = value);
        await AssertRejected(pipeline, context);
        Assert.Equal(IPAddress.Parse(Peer), context.Connection.RemoteIpAddress);
    }

    [Theory]
    [InlineData("192.0.2.10")]
    [InlineData("192.0.2.11")]
    [InlineData("2001:db8::10")]
    [InlineData(null)]
    public async Task Cloudfront_authentication_does_not_depend_on_socket_peer_or_known_proxy_ips(string? peer)
    {
        using var pipeline = new Pipeline();
        Assert.Empty(pipeline.Options.KnownProxies);
        var context = await pipeline.Request(change: c => c.Connection.RemoteIpAddress = peer is null ? null : IPAddress.Parse(peer));
        Assert.Equal(204, context.Response.StatusCode);
        Assert.Equal("https", context.Request.Scheme);
        Assert.Equal(IPAddress.Parse("198.51.100.20"), context.Connection.RemoteIpAddress);
        Assert.Equal(AuthenticationOriginStatus.Available, pipeline.Origin(context).Status);
        Assert.Equal(0, context.Response.Headers.Location.Count);
    }

    [Fact]
    public async Task Cloudfront_ignores_optional_legacy_proxy_entries_when_alb_peer_changes()
    {
        using var pipeline = new Pipeline(knownProxy: true);
        var context = await pipeline.Request(change: c => c.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.11"));
        Assert.Equal(204, context.Response.StatusCode);
        Assert.Equal("https", context.Request.Scheme);
        Assert.Equal(AuthenticationOriginStatus.Available, pipeline.Origin(context).Status);
    }

    [Fact]
    public async Task Disabled_mode_still_rejects_forwarding_from_a_peer_outside_its_explicit_allowlist()
    {
        using var pipeline = new Pipeline(enabled: false, knownProxy: true);
        var context = await pipeline.Request(change: c =>
        {
            c.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.11");
            c.Request.Headers["X-Forwarded-For"] = "198.51.100.25";
            c.Request.Headers["X-Forwarded-Proto"] = "https";
        });
        Assert.Equal(307, context.Response.StatusCode);
        Assert.Equal("http", context.Request.Scheme);
        Assert.Equal(AuthenticationOriginStatus.Unavailable, pipeline.Origin(context).Status);
        Assert.False(pipeline.EndpointReached);
    }

    [Theory]
    [InlineData("register", "missing-secret")]
    [InlineData("login", "missing-secret")]
    [InlineData("logout", "missing-secret")]
    [InlineData("register", "wrong-secret")]
    [InlineData("login", "wrong-secret")]
    [InlineData("logout", "wrong-secret")]
    [InlineData("register", "duplicate-secret")]
    [InlineData("login", "duplicate-secret")]
    [InlineData("logout", "duplicate-secret")]
    public async Task Direct_alb_spoof_without_one_valid_secret_fails_closed_even_with_forged_https_headers(string route, string invalid)
    {
        using var pipeline = new Pipeline();
        Assert.Empty(pipeline.Options.KnownProxies);
        var context = await pipeline.Request(path: "/api/v1/auth/" + route, change: c =>
        {
            Invalidate(c, pipeline.Secret, invalid);
            c.Request.Headers["X-Forwarded-Proto"] = "https";
            c.Request.Headers["X-Forwarded-For"] = "198.51.100.25";
            c.Request.Headers["X-Forwarded-Host"] = "attacker.invalid";
        });
        await AssertRejected(pipeline, context);
        Assert.Equal("http", context.Request.Scheme);
        Assert.Equal(OriginHost, context.Request.Host.Value);
    }

    [Fact]
    public async Task Valid_cloudfront_transport_preserves_authorization_without_logging_it()
    {
        using var pipeline = new Pipeline();
        var authorization = "Bearer " + NewSecret(); // Solo canario efímero, no token real.
        var context = await pipeline.Request(change: c => c.Request.Headers.Authorization = authorization);
        Assert.Equal(204, context.Response.StatusCode);
        Assert.True(context.Request.Headers.Authorization.ToString() == authorization);
        Assert.False(pipeline.Logs.ToString().Contains(authorization, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Ipv4_mapped_socket_peer_does_not_require_an_allowlist_entry_in_cloudfront_mode()
    {
        using var pipeline = new Pipeline();
        var context = await pipeline.Request(change: c => c.Connection.RemoteIpAddress = IPAddress.Parse("::ffff:" + Peer));
        Assert.Equal(204, context.Response.StatusCode);
        Assert.Equal(AuthenticationOriginStatus.Available, pipeline.Origin(context).Status);
    }

    [Fact]
    public async Task Legacy_forwarding_cannot_rescue_failed_cloudfront_even_on_https_socket()
    {
        using var pipeline = new Pipeline();
        var context = await pipeline.Request(change: c =>
        {
            c.Request.Scheme = "https";
            c.Request.Headers.Remove(OriginHeader);
            c.Request.Headers["X-Forwarded-Proto"] = "https";
            c.Request.Headers["X-Forwarded-For"] = "198.51.100.99";
        });
        await AssertRejected(pipeline, context);
    }

    [Fact]
    public async Task Cloudfront_origin_does_not_use_viewer_supplied_forwarded_host_proto_or_ip_chain()
    {
        using var pipeline = new Pipeline();
        var context = await pipeline.Request(change: c =>
        {
            c.Request.Headers["X-Forwarded-Host"] = "attacker.invalid";
            c.Request.Headers["X-Forwarded-Proto"] = new StringValues(["http", "https"]);
            c.Request.Headers["X-Forwarded-For"] = "203.0.113.99, 198.51.100.200, 192.0.2.30";
        });
        Assert.Equal(204, context.Response.StatusCode);
        Assert.Equal("https", context.Request.Scheme);
        Assert.Equal(OriginHost, context.Request.Host.Value);
        Assert.Equal(IPAddress.Parse("198.51.100.20"), context.Connection.RemoteIpAddress);
        Assert.Equal(0, context.Response.Headers.Location.Count);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Exact_get_health_remains_anonymous_without_an_origin_secret(bool enabled)
    {
        using var pipeline = new Pipeline(enabled: enabled);
        var context = await pipeline.Request(method: "GET", path: "/health", change: c =>
        {
            c.Request.Headers.Remove(OriginHeader);
            c.Request.QueryString = new QueryString("?probe=1");
        });
        Assert.Equal(200, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Headers.Location.Count);
        context.Response.Body.Position = 0;
        Assert.Equal("Healthy", await new StreamReader(context.Response.Body).ReadToEndAsync());
        Assert.Equal("http", context.Request.Scheme);
        Assert.Equal(AuthenticationOriginStatus.Unavailable, pipeline.Origin(context).Status);
    }

    [Theory]
    [InlineData("POST", "/health", "")]
    [InlineData("HEAD", "/health", "")]
    [InlineData("GET", "/health/", "")]
    [InlineData("GET", "/health/anything", "")]
    [InlineData("GET", "/HEALTH", "")]
    [InlineData("GET", "/health", "/prefix")]
    public async Task Invalid_cloudfront_cannot_expand_exact_health_exception(string method, string path, string pathBase)
    {
        using var pipeline = new Pipeline();
        var context = await pipeline.Request(method, path, c =>
        {
            c.Request.Headers.Remove(OriginHeader);
            c.Request.PathBase = pathBase;
        });
        await AssertRejected(pipeline, context);
    }

    [Theory]
    [InlineData("missing-secret")]
    [InlineData("short-secret")]
    [InlineData("newline-secret")]
    [InlineData("non-ascii-secret")]
    [InlineData("oversized-secret")]
    [InlineData("wildcard-ipv4-proxy")]
    [InlineData("wildcard-ipv6-proxy")]
    [InlineData("mapped-wildcard-proxy")]
    [InlineData("direct-connection")]
    [InlineData("allow-http")]
    public void Enabled_mode_rejects_incomplete_or_incompatible_configuration_without_secret_in_error(string invalid)
    {
        var secret = NewSecret();
        var configuration = Settings(secret, true, true);
        switch (invalid)
        {
            case "missing-secret": configuration.Remove(RootKey + "CloudFront:OriginSecret"); break;
            case "short-secret": configuration[RootKey + "CloudFront:OriginSecret"] = "short"; break;
            case "newline-secret": configuration[RootKey + "CloudFront:OriginSecret"] = secret + "\n"; break;
            case "non-ascii-secret": configuration[RootKey + "CloudFront:OriginSecret"] = secret + "é"; break;
            case "oversized-secret": configuration[RootKey + "CloudFront:OriginSecret"] = new string('a', 257); break;
            case "wildcard-ipv4-proxy": configuration[RootKey + "KnownProxies:0"] = "0.0.0.0"; break;
            case "wildcard-ipv6-proxy": configuration[RootKey + "KnownProxies:0"] = "::"; break;
            case "mapped-wildcard-proxy": configuration[RootKey + "KnownProxies:0"] = "::ffff:0.0.0.0"; break;
            case "direct-connection": configuration[RootKey + "DirectConnectionApproved"] = "true"; break;
            case "allow-http": configuration[RootKey + "AllowLocalHttp"] = "true"; break;
        }
        using var logs = new StringWriter();
        using var services = Services(configuration, "Production", logs);
        var error = Assert.Throws<OptionsValidationException>(() => services.GetRequiredService<IOptions<AuthenticationTransportOptions>>().Value);
        Assert.False(error.ToString().Contains(secret, StringComparison.Ordinal));
        Assert.False(logs.ToString().Contains(secret, StringComparison.Ordinal));
    }

    private static void Invalidate(DefaultHttpContext context, string secret, string invalid)
    {
        switch (invalid)
        {
            case "missing-secret": context.Request.Headers.Remove(OriginHeader); break;
            case "empty-secret": context.Request.Headers[OriginHeader] = ""; break;
            case "whitespace-secret": context.Request.Headers[OriginHeader] = " " + secret; break;
            case "wrong-secret": context.Request.Headers[OriginHeader] = NewSecret(); break;
            case "duplicate-secret": context.Request.Headers[OriginHeader] = new StringValues([secret, secret]); break;
            case "comma-secret": context.Request.Headers[OriginHeader] = secret + "," + secret; break;
            case "oversized-secret": context.Request.Headers[OriginHeader] = new string('a', 257); break;
            case "missing-proto": context.Request.Headers.Remove(ProtoHeader); break;
            case "empty-proto": context.Request.Headers[ProtoHeader] = ""; break;
            case "http-proto": context.Request.Headers[ProtoHeader] = "http"; break;
            case "uppercase-proto": context.Request.Headers[ProtoHeader] = "HTTPS"; break;
            case "whitespace-proto": context.Request.Headers[ProtoHeader] = " https"; break;
            case "duplicate-proto": context.Request.Headers[ProtoHeader] = new StringValues(["https", "https"]); break;
            case "comma-proto": context.Request.Headers[ProtoHeader] = "https,https"; break;
            case "missing-address": context.Request.Headers.Remove(AddressHeader); break;
            case "duplicate-address": context.Request.Headers[AddressHeader] = new StringValues(["198.51.100.20:443", "198.51.100.21:443"]); break;
        }
    }

    private static async Task AssertRejected(Pipeline pipeline, DefaultHttpContext context)
    {
        Assert.Equal(503, context.Response.StatusCode);
        Assert.False(pipeline.EndpointReached);
        Assert.Equal(0, context.Response.Headers.Location.Count);
        Assert.Equal(AuthenticationOriginStatus.Unavailable, pipeline.Origin(context).Status);
        Assert.False(context.Request.Headers.ContainsKey(OriginHeader));
        Assert.Equal("no-store", context.Response.Headers.CacheControl.ToString());
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("temporarily_unavailable", body);
        Assert.False(body.Contains(pipeline.Secret, StringComparison.Ordinal));
        Assert.False(pipeline.Logs.ToString().Contains(pipeline.Secret, StringComparison.Ordinal));
    }

    private static string NewSecret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)); // Solo fixture efímero; nunca un secreto desplegado.

    private static Dictionary<string, string?> Settings(string secret, bool enabled, bool knownProxy)
    {
        var values = new Dictionary<string, string?>
        {
            [RootKey + "CloudFront:Enabled"] = enabled.ToString(),
            [RootKey + "CloudFront:OriginSecret"] = secret
        };
        if (knownProxy) values[RootKey + "KnownProxies:0"] = Peer;
        return values;
    }

    private static ServiceProvider Services(Dictionary<string, string?> values, string environment, StringWriter logs)
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var hostEnvironment = new LocalEnvironment { EnvironmentName = environment };
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<IHostEnvironment>(hostEnvironment);
        services.AddLogging(builder => builder.ClearProviders().AddProvider(new SafeConsoleLoggerProvider(logs)));
        services.AddAuthenticationFoundation(config, hostEnvironment);
        var diagnostic = new DiagnosticListener("CloudFrontContract");
        services.AddSingleton(diagnostic); services.AddSingleton<DiagnosticSource>(diagnostic);
        services.AddRouting(); services.AddHealthChecks();
        return services.BuildServiceProvider();
    }

    private sealed class Pipeline : IDisposable
    {
        public string Secret { get; } = NewSecret();
        public StringWriter Logs { get; } = new();
        public ServiceProvider Provider { get; }
        public AuthenticationTransportOptions Options => Provider.GetRequiredService<IOptions<AuthenticationTransportOptions>>().Value;
        public bool EndpointReached { get; private set; }
        private readonly RequestDelegate application;

        public Pipeline(bool enabled = true, string environment = "Production", bool knownProxy = false)
        {
            Provider = Services(Settings(Secret, enabled, knownProxy), environment, Logs);
            var app = new ApplicationBuilder(Provider);
            app.UseAuthenticationTransport(); app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapHealthChecks("/health").AllowAnonymous());
            app.Run(context => { EndpointReached = true; context.Response.StatusCode = 204; return Task.CompletedTask; });
            application = app.Build();
        }

        public AuthenticationOriginResult Origin(HttpContext context) => Provider.GetRequiredService<IAuthenticationOriginProvider>().Resolve(context);

        public async Task<DefaultHttpContext> Request(string method = "POST", string path = "/api/v1/auth/register", Action<DefaultHttpContext>? change = null)
        {
            EndpointReached = false;
            var context = new DefaultHttpContext { RequestServices = Provider };
            context.Request.Method = method; context.Request.Path = path; context.Request.Scheme = "http";
            context.Request.Host = new HostString(OriginHost);
            context.Connection.RemoteIpAddress = IPAddress.Parse(Peer); context.Connection.RemotePort = 12345;
            context.Request.Headers[OriginHeader] = Secret;
            context.Request.Headers[ProtoHeader] = "https";
            context.Request.Headers[AddressHeader] = "198.51.100.20:49152";
            context.Request.Headers["X-Forwarded-For"] = "198.51.100.20, 192.0.2.30";
            context.Request.Headers["X-Forwarded-Proto"] = "http";
            context.Response.Body = new MemoryStream();
            change?.Invoke(context);
            await application(context);
            return context;
        }

        public void Dispose() { Provider.Dispose(); Logs.Dispose(); }
    }

    private sealed class LocalEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Production";
        public string ApplicationName { get; set; } = "CloudFrontContract";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
