using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using Enver.API.Composition;
using Enver.API.Http;
using Enver.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class AuthenticationFoundationCompositionTests(AuthenticationDependenciesFixture fixture)
{
    [Theory]
    [InlineData("MongoDb:DatabaseName", "invalid")]
    [InlineData("Redis:FailClosed", "false")]
    [InlineData("Jwt:Algorithm", "none")]
    [InlineData("RateLimiting:PseudonymizationKeyBase64", "")]
    [InlineData("Authentication:HashingConcurrencyLimit", "0")]
    [InlineData("AuthenticationTransport:AllowLocalHttp", "true")]
    public void Startup_validation_is_not_bypassed_by_composition(string key, string value)
    {
        using var factory = new AuthenticationApiFactory(fixture, new Dictionary<string, string?>
        {
            ["AuthenticationTransport:DirectConnectionApproved"] = "false", [key] = value
        });
        var exception = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        Assert.False(fixture.ContainsSyntheticSecret(exception.ToString()));
    }

    [Fact]
    public async Task Errors_are_safe_correlated_no_store_and_not_logged_with_exception_plaintext()
    {
        var logs = new SafeCapture();
        using var server = Server(direct: true, localHttp: true, logs: logs);
        using var client = server.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "unsafe\"client");
        var response = await client.GetAsync("/__test/error");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        Assert.Equal("internal_error", json.RootElement.GetProperty("code").GetString());
        Assert.Equal(response.Headers.GetValues("X-Correlation-ID").Single(), json.RootElement.GetProperty("traceId").GetString());
        Assert.False(json.RootElement.TryGetProperty("instance", out _));
        Assert.False(body.Contains("synthetic-password-never-log", StringComparison.Ordinal));
        Assert.False(string.Join("\n", logs.Messages).Contains("synthetic-password-never-log", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(16385, "application/json", 413)]
    [InlineData(2, "text/plain", 415)]
    [InlineData(2, "application/json", 400)]
    public async Task Routed_metadata_activates_boundary_before_test_handler(int bytes, string contentType, int status)
    {
        using var server = Server(true, true);
        using var client = server.GetTestClient();
        var response = await client.PostAsync("/__test/boundary", new StringContent(new string('x', bytes), Encoding.UTF8, contentType));
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Request_budget_and_no_store_are_connected_without_business_endpoints()
    {
        using var server = Server(true, true);
        using var client = server.GetTestClient();
        var response = await client.PostAsync("/__test/boundary", new StringContent("{\"fullName\":\"Test\",\"email\":\"a@uttt.edu.mx\",\"password\":\"synthetic-password\"}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
    }

    [Theory]
    [InlineData(false, false, "http", "https", 307)]
    [InlineData(false, true, "https", "https", 200)]
    [InlineData(true, true, "https", "https", 200)]
    public async Task Untrusted_headers_do_not_control_scheme_and_authorized_proxy_avoids_redirect_loop(bool known, bool httpsRequest, string scheme, string forwardedScheme, int status)
    {
        using var server = Server(direct: !known, localHttp: false, knownProxy: known);
        using var client = server.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, (httpsRequest ? "https" : scheme) + "://test.invalid/__test/origin");
        request.Headers.Add("X-Forwarded-For", "198.51.100.20");
        request.Headers.Add("X-Forwarded-Proto", forwardedScheme);
        var response = await client.SendAsync(request);
        Assert.Equal(status, (int)response.StatusCode);
        if (status == 200)
        {
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("https", json.RootElement.GetProperty("scheme").GetString());
            Assert.Equal(known ? "198.51.100.20" : "127.0.0.1", json.RootElement.GetProperty("peer").GetString());
            Assert.True(json.RootElement.GetProperty("originAvailable").GetBoolean());
            Assert.True(response.Headers.Contains("Strict-Transport-Security"));
        }
    }

    [Fact]
    public async Task Authorized_proxy_with_https_forwarding_accepts_http_hop_but_missing_or_multiple_headers_close_origin()
    {
        using var server = Server(false, false, knownProxy: true);
        using var client = server.GetTestClient();
        foreach (var forwarded in new[] { "198.51.100.20", "198.51.100.20, 198.51.100.21", "" })
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://test.invalid/__test/origin");
            if (forwarded.Length > 0) request.Headers.Add("X-Forwarded-For", forwarded);
            request.Headers.Add("X-Forwarded-Proto", "https");
            var response = await client.SendAsync(request);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(forwarded == "198.51.100.20", json.RootElement.GetProperty("originAvailable").GetBoolean());
        }
        using var valid = new HttpRequestMessage(HttpMethod.Get, "http://test.invalid/__test/origin");
        valid.Headers.Add("X-Forwarded-For", "198.51.100.20"); valid.Headers.Add("X-Forwarded-Proto", "https");
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(valid)).StatusCode);
    }

    private IHost Server(bool direct, bool localHttp, bool knownProxy = false, SafeCapture? logs = null)
    {
        var overrides = new Dictionary<string, string?>
        {
            ["AuthenticationTransport:DirectConnectionApproved"] = direct.ToString(),
            ["AuthenticationTransport:AllowLocalHttp"] = localHttp.ToString()
        };
        if (knownProxy) overrides["AuthenticationTransport:KnownProxies:0"] = "127.0.0.1";
        var configuration = fixture.CreateConfiguration(overrides);
        return new HostBuilder().UseEnvironment(fixture.EnvironmentName).ConfigureWebHost(web => web.UseTestServer()
            .ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(configuration))
            .ConfigureLogging(l => { l.ClearProviders(); if (logs is not null) l.AddProvider(logs); })
            .ConfigureServices((context, services) =>
            {
                services.AddRouting(); services.AddAuthenticationFoundation(context.Configuration, context.HostingEnvironment);
            })
            .Configure(app =>
            {
                // Socket sintético controlado por servidor de test, nunca por un header del usuario.
                app.Use(async (context, next) => { context.Connection.RemoteIpAddress = IPAddress.Loopback; await next(context); });
                app.UseAuthenticationFoundation();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/__test/error", _ => throw new InvalidOperationException("synthetic-password-never-log"));
                    endpoints.MapGet("/__test/origin", context => context.Response.WriteAsJsonAsync(new
                    {
                        scheme = context.Request.Scheme, peer = context.Connection.RemoteIpAddress?.ToString(),
                        originAvailable = context.RequestServices.GetRequiredService<IAuthenticationOriginProvider>().Resolve(context).Status == AuthenticationOriginStatus.Available
                    }));
                    endpoints.MapPost("/__test/boundary", context =>
                    {
                        Assert.NotNull(context.Features.Get<RequestBudget>());
                        context.Response.StatusCode = 204; return Task.CompletedTask;
                    }).WithMetadata(new AuthenticationRequestMetadata(AuthenticationOperation.Register));
                });
            })).Start();
    }

    private sealed class SafeCapture : ILoggerProvider
    {
        public ConcurrentQueue<string> Messages { get; } = new();
        public ILogger CreateLogger(string categoryName) => new Capture(Messages);
        public void Dispose() { }
        private sealed class Capture(ConcurrentQueue<string> messages) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => messages.Enqueue(formatter(state, exception) + exception?.ToString());
        }
    }
}
