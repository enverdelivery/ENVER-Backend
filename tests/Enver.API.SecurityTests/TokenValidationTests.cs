using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Enver.API.SecurityTests;

public sealed partial class TokenValidationTests
{
    [Theory, Trait("Task", "039"), Trait("Task", "042")]
    [InlineData("malformed")][InlineData("oversized")][InlineData("signature")][InlineData("issuer")]
    [InlineData("audience")][InlineData("expired")][InlineData("boundary")][InlineData("future-iat")]
    [InlineData("future-nbf")][InlineData("missing-kid")][InlineData("unknown-kid")][InlineData("none")]
    [InlineData("wrong-alg")][InlineData("missing-claims")][InlineData("duplicate-sub")][InlineData("duplicate-jti")]
    [InlineData("escaped-duplicate")][InlineData("aud-array")][InlineData("ambiguous-iss")][InlineData("wrong-types")]
    [InlineData("invalid-uuid")][InlineData("invalid-jti")][InlineData("jku")][InlineData("x5u")]
    [InlineData("critical")][InlineData("duplicate-kid")][InlineData("extra-pii")][InlineData("utf8")]
    public async Task Invalid_Bearer_is_uniform_401_without_Redis_work_or_sensitive_diagnostics(string defect)
    {
        using var h = new Harness(); var p = h.Claims(); var header = "{\"alg\":\"HS256\",\"kid\":\"A\"}";
        string? raw = null;
        switch (defect)
        {
            case "malformed": raw = "not-a-token"; break;
            case "oversized": raw = new string('a', 4097); break;
            case "issuer": p["iss"] = "other"; break;
            case "audience": p["aud"] = "other"; break;
            case "expired": p["iat"] = Harness.Epoch - 931; p["exp"] = Harness.Epoch - 31; break;
            case "boundary": p["iat"] = Harness.Epoch - 930; p["exp"] = Harness.Epoch - 30; break;
            case "future-iat": p["iat"] = Harness.Epoch + 31; p["exp"] = Harness.Epoch + 931; break;
            case "future-nbf": p["nbf"] = Harness.Epoch + 31; break;
            case "missing-kid": header = "{\"alg\":\"HS256\"}"; break;
            case "unknown-kid": header = "{\"alg\":\"HS256\",\"kid\":\"unknown\"}"; break;
            case "none": header = "{\"alg\":\"none\",\"kid\":\"A\"}"; break;
            case "wrong-alg": header = "{\"alg\":\"RS256\",\"kid\":\"A\"}"; break;
            case "missing-claims": p.Remove("sub"); break;
            case "aud-array": p["aud"] = new[] { "enver-mobile", "other" }; break;
            case "wrong-types": p["iat"] = Harness.Epoch.ToString(); break;
            case "invalid-uuid": p["sub"] = "AAAAAAAA-AAAA-4AAA-8AAA-AAAAAAAAAAAA"; break;
            case "invalid-jti": p["jti"] = new string('A', 42) + "B"; break;
            case "jku": header = header[..^1] + ",\"jku\":\"https://invalid.example\"}"; break;
            case "x5u": header = header[..^1] + ",\"x5u\":\"https://invalid.example\"}"; break;
            case "critical": header = header[..^1] + ",\"crit\":[\"unsupported\"]}"; break;
            case "duplicate-kid": header = header[..^1] + ",\"kid\":\"A\"}"; break;
            case "extra-pii": p["email"] = Harness.SensitiveSentinel; break;
            case "utf8": raw = h.SignBytes([0xff]); break;
        }
        var payload = JsonSerializer.Serialize(p);
        var duplicate = defect switch { "duplicate-sub" => "sub", "duplicate-jti" => "jti", "escaped-duplicate" => "s\\u0075b", "ambiguous-iss" => "iss", _ => null };
        if (duplicate is not null) payload = payload[..^1] + ",\"" + duplicate + "\":\"ambiguous\"}";
        var token = raw ?? h.Sign(payload, header);
        if (defect == "signature") token = token[..(token.LastIndexOf('.') + 1)] + Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        using var response = await h.SendAsync(token);
        await h.AssertProblemAsync(response, 401, token);
        Assert.Equal(0, h.Store.Reads); Assert.Equal(0, h.State.Reads);
        h.AssertPrivate(token);
    }

    [Fact, Trait("Task", "038"), Trait("Task", "042")]
    public async Task Real_issuer_has_exact_minimal_payload_and_safe_result_representation()
    {
        using var h = new Harness(); var issued = h.Issuer.Issue(Guid.NewGuid());
        Assert.Equal(nameof(AccessTokenIssueResult), issued.ToString());
        using var json = JsonDocument.Parse(Base64UrlEncoder.DecodeBytes(issued.AccessToken!.Split('.')[1]));
        Assert.Equal(new[] { "aud", "exp", "iat", "iss", "jti", "sub" }, json.RootElement.EnumerateObject().Select(p => p.Name).Order());
        using var response = await h.SendAsync(issued.AccessToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        h.AssertPrivate(issued.AccessToken);
        var routes = h.Server.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>().ToArray();
        Assert.DoesNotContain(routes, r => r.RoutePattern.RawText is "/api/v1/auth/login" or "/api/v1/auth/logout");
    }

    [Theory, Trait("Task", "040"), Trait("Task", "042")]
    [InlineData(TokenRevocationReadStatus.Revoked, 401)]
    [InlineData(TokenRevocationReadStatus.Unavailable, 503)]
    [InlineData(TokenRevocationReadStatus.Uncertain, 503)]
    public async Task Authenticated_identity_does_not_hide_revocation_or_operational_failure(TokenRevocationReadStatus status, int expected)
    {
        using var h = new Harness(); h.Store.Status = status; var token = h.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        using var response = await h.SendAsync(token); await h.AssertProblemAsync(response, expected, token);
        Assert.Equal(1, h.Store.Reads); Assert.Equal(0, h.Store.Writes); h.AssertPrivate(token);
    }

    [Theory, Trait("Task", "040"), Trait("Task", "042")]
    [InlineData(AuthenticationSecurityAdmission.Unavailable)][InlineData(AuthenticationSecurityAdmission.Uncertain)]
    public async Task Unverifiable_authority_never_authorizes_or_reads_revocation(AuthenticationSecurityAdmission admission)
    {
        using var h = new Harness(); h.State.Admission = admission; var token = h.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        using var response = await h.SendAsync(token); await h.AssertProblemAsync(response, 503, token);
        Assert.Equal(0, h.Store.Reads); h.AssertPrivate(token);
    }

    [Fact, Trait("Task", "040"), Trait("Task", "042")]
    public async Task Logout_policy_on_a_different_test_route_cannot_admit_a_revoked_token()
    {
        using var h = new Harness(); h.Store.Status = TokenRevocationReadStatus.Revoked;
        var token = h.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        using var response = await h.SendAsync(token, "/_misused", HttpMethod.Post);
        await h.AssertProblemAsync(response, 401, token); Assert.Equal(0, h.Store.Writes); h.AssertPrivate(token);
    }

    [Fact, Trait("Task", "040"), Trait("Task", "042")]
    public async Task Fallback_default_and_missing_identity_are_deny_by_default()
    {
        using var h = new Harness();
        using var missing = await h.SendAsync(null); await h.AssertProblemAsync(missing, 401, "");
        h.Store.Status = TokenRevocationReadStatus.Revoked;
        var token = h.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        using var fallback = await h.SendAsync(token, "/_fallback"); await h.AssertProblemAsync(fallback, 401, token);
        h.AssertPrivate(token);
    }

    private sealed class Clock : TimeProvider { public override DateTimeOffset GetUtcNow() => DateTimeOffset.FromUnixTimeSeconds(Harness.Epoch); }
    private sealed class Harness : IDisposable
    {
        public const long Epoch = 1800000000;
        public const string SensitiveSentinel = "synthetic-sensitive-password-hash-not-for-logs";
        private readonly byte[] key = RandomNumberGenerator.GetBytes(32);
        private readonly string hmac = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        private readonly Capture logs = new();
        public TestServer Server { get; }
        private readonly HttpClient client;
        private readonly IHost application;
        public JwtTokenIssuer Issuer { get; }
        public Store Store { get; } = new();
        public State State { get; } = new();
        public Harness(Action<IServiceCollection>? configure = null)
        {
            var ring = new JwtKeyRing(new JwtOptions { Algorithm = "HS256", Issuer = "enver-api", Audience = "enver-mobile", LifetimeSeconds = 900,
                ClockSkewSeconds = 30, ActiveKid = "A", Keys = [new() { Kid = "A", KeyBase64 = Convert.ToBase64String(key) }] },
                new RateLimitingOptions { PseudonymizationKeyBase64 = hmac });
            var clock = new Clock(); Issuer = new(ring, clock); var validator = new JwtClaimsValidator(ring, clock);
            application = new HostBuilder().ConfigureWebHost(web => web.UseTestServer().ConfigureLogging(b => b.SetMinimumLevel(LogLevel.Trace).AddProvider(logs)).ConfigureServices(services =>
            {
                services.AddRouting(); ApiProblemDetails.AddSafeApiErrors(services);
                services.AddAuthentication(AuthenticationPolicies.Scheme).AddJwtBearer();
                services.AddSingleton<IConfigureOptions<JwtBearerOptions>>(new JwtBearerConfiguration(validator));
                services.AddAuthorization(AuthenticationPolicies.Configure);
                services.Configure<AuthenticationOptions>(o => o.LogoutBudgetMilliseconds = 1000);
                services.AddSingleton<IAuthenticationSecurityState>(State); services.AddSingleton<ITokenRevocationStore>(Store);
                services.AddSingleton<IAuthorizationHandler, RevocationAuthorizationHandler>();
                services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthenticationAuthorizationResultHandler>();
                configure?.Invoke(services);
            }).Configure(app =>
            {
                app.UseMiddleware<RequestContextMiddleware>(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
                app.UseEndpoints(e =>
                {
                    e.MapGet("/_jwt", () => "ok").RequireAuthorization();
                    e.MapGet("/_fallback", () => "ok");
                    e.MapPost("/_misused", () => "must-not-authorize").RequireAuthorization(AuthenticationPolicies.Logout);
                    if (configure is not null) e.MapControllers();
                });
            })).Start(); Server = application.GetTestServer(); client = Server.CreateClient();
        }
        public Dictionary<string, object> Claims() => new() { ["sub"] = Guid.NewGuid().ToString("D"), ["jti"] = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32)),
            ["iss"] = "enver-api", ["aud"] = "enver-mobile", ["iat"] = Epoch, ["exp"] = Epoch + 900 };
        public string Sign(string payload, string header = "{\"alg\":\"HS256\",\"kid\":\"A\"}") => SignBytes(Encoding.UTF8.GetBytes(payload), header);
        // Firma adversarial test-only con primitivo mantenido; no handler JWT alternativo en producto.
        public string SignBytes(byte[] payload, string header = "{\"alg\":\"HS256\",\"kid\":\"A\"}")
        {
            var compact = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(header)) + "." + Base64UrlEncoder.Encode(payload);
            return compact + "." + Base64UrlEncoder.Encode(HMACSHA256.HashData(key, Encoding.ASCII.GetBytes(compact)));
        }
        public Task<HttpResponseMessage> SendAsync(string? token, string path = "/_jwt", HttpMethod? method = null, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method ?? HttpMethod.Get, path) { Content = content };
            if (token is not null) request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
            return client.SendAsync(request);
        }
        public async Task AssertProblemAsync(HttpResponseMessage response, int expected, string token)
        {
            Assert.Equal(expected, (int)response.StatusCode); Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(expected == 401 ? "Bearer" : null, response.Headers.WwwAuthenticate.SingleOrDefault()?.ToString());
            var body = await response.Content.ReadAsStringAsync(); using var p = JsonDocument.Parse(body);
            Assert.Equal(expected == 401 ? "authentication_failed" : "temporarily_unavailable", p.RootElement.GetProperty("code").GetString());
            if (expected == 401) Assert.Equal("No fue posible autenticar la solicitud.", p.RootElement.GetProperty("detail").GetString());
            Assert.Equal(response.Headers.GetValues("X-Correlation-ID").Single(), p.RootElement.GetProperty("traceId").GetString());
            if (token.Length > 0) Assert.DoesNotContain(token, body);
            Assert.DoesNotContain(Convert.ToBase64String(key), body); Assert.DoesNotContain(SensitiveSentinel, body);
        }
        public void AssertPrivate(string token)
        {
            var messages = string.Join("\n", logs.Messages); Assert.NotEmpty(logs.Messages);
            if (token.Length > 0) Assert.DoesNotContain(token, messages);
            Assert.DoesNotContain(Convert.ToBase64String(key), messages); Assert.DoesNotContain(hmac, messages);
            Assert.DoesNotContain(SensitiveSentinel, messages);
        }
        public void Dispose() { client.Dispose(); application.Dispose(); }
    }
    private sealed class State : IAuthenticationSecurityState
    {
        public int Reads; public AuthenticationSecurityAdmission Admission = AuthenticationSecurityAdmission.Verifiable;
        public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget budget)
        { Reads++; return ValueTask.FromResult(new AuthenticationSecurityDecision(Admission)); }
    }
    private sealed class Store : ITokenRevocationStore
    {
        public int Reads; public int Writes; public TokenRevocationReadStatus Status = TokenRevocationReadStatus.NotRevoked;
        public ValueTask<TokenRevocationReadResult> CheckAsync(TokenRevocationReadRequest request, RequestBudget budget)
        { Reads++; return ValueTask.FromResult(new TokenRevocationReadResult(Status)); }
        public ValueTask<TokenRevocationWriteResult> RevokeAsync(TokenRevocationWriteRequest request, RequestBudget budget)
        { Writes++; throw new InvalidOperationException("La política no escribe."); }
    }
    private sealed class Capture : ILoggerProvider
    {
        public ConcurrentQueue<string> Messages { get; } = new();
        public ILogger CreateLogger(string categoryName) => new CaptureLogger(this);
        public void Dispose() { }
        private sealed class CaptureLogger(Capture owner) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            { owner.Messages.Enqueue(state.ToString() ?? ""); return null; }
            public bool IsEnabled(LogLevel level) => true;
            public void Log<TState>(LogLevel level, EventId id, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            { owner.Messages.Enqueue(formatter(state, exception) + exception?.ToString()); }
        }
    }
}
