using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Enver.API.Composition;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.API.Infrastructure.Redis;
using Enver.API.Security;
using Enver.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using MongoDB.Driver;
using StackExchange.Redis;
using AuthenticationOptions = Enver.API.Configuration.AuthenticationOptions;

namespace Enver.API.IntegrationTests;

/// <summary>058: WebApplicationFactory ejecuta Program; no instala otro pipeline o esquema en el test.</summary>
[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class AuthenticationPipelineTests(AuthenticationDependenciesFixture fixture)
{
    [Theory, Trait("Task", "058")]
    [InlineData("absent")]
    [InlineData("malformed")]
    [InlineData("valid")]
    public async Task Anonymous_Register_Login_skip_optional_Bearer_and_charge_once(string bearer)
    {
        await using var s = await PipelineScope.CreateAsync(fixture);
        var optional = bearer switch { "absent" => null, "malformed" => "not-a-jwt", _ => s.Issue() };
        using var register = await s.SendAsync("/api/v1/auth/register", optional, JsonContent.Create(new
        { fullName = "Pipeline synthetic", email = PipelineScope.Email, password = PipelineScope.Secret }));
        Assert.Equal(HttpStatusCode.Created, register.StatusCode); NoStore(register);
        using var login = await s.SendAsync("/api/v1/auth/login", optional, JsonContent.Create(new
        { email = PipelineScope.Email, password = PipelineScope.Secret }));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode); NoStore(login);
        using var payload = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        Assert.Equal(new[] { "accessToken", "expiresIn", "tokenType" }, payload.RootElement.EnumerateObject().Select(p => p.Name).Order());
        Assert.Equal("Bearer", payload.RootElement.GetProperty("tokenType").GetString());
        Assert.Equal(900, payload.RootElement.GetProperty("expiresIn").GetInt32());
        var token = payload.RootElement.GetProperty("accessToken").GetString()!; s.Track(token);
        Assert.Equal(2, s.Events.PublicNoResult); Assert.Equal(0, s.Events.Validated); Assert.Equal(0, s.Events.Failed);
        Assert.Equal(0, s.Revocations.Reads); Assert.Equal(2, s.Quotas.Calls);
        using var authorized = await s.SendAsync("/_058/protected", token, method: HttpMethod.Get);
        Assert.Equal(HttpStatusCode.OK, authorized.StatusCode); NoStore(authorized);
        Assert.Equal(1, s.Events.Validated); Assert.Equal(1, s.Revocations.Reads);
        var user = Assert.Single(await s.Services.GetRequiredService<IMongoDatabase>().GetCollection<UserDocument>("users")
            .Find(FilterDefinition<UserDocument>.Empty).ToListAsync());
        s.AssertSafeLogs(token, optional, user.PasswordHash);
    }

    [Fact, Trait("Task", "058")]
    public async Task Logout_requires_Bearer_revokes_and_only_exact_Logout_can_reconfirm()
    {
        await using var s = await PipelineScope.CreateAsync(fixture);
        await ProblemAsync(await s.SendAsync("/api/v1/auth/logout"), 401);
        var token = s.Issue(); var other = s.Issue();
        foreach (var path in new[] { "/_058/protected", "/_058/protected/default", "/_058/protected/normal" })
        {
            using var before = await s.SendAsync(path, token, method: HttpMethod.Get);
            Assert.Equal(HttpStatusCode.OK, before.StatusCode); NoStore(before);
        }
        using var logout = await s.SendAsync("/api/v1/auth/logout", token);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode); NoStore(logout);
        Assert.Empty(await logout.Content.ReadAsByteArrayAsync());
        foreach (var path in new[] { "/_058/protected", "/_058/protected/default", "/_058/protected/normal" })
            await ProblemAsync(await s.SendAsync(path, token, method: HttpMethod.Get), 401);
        using var repeated = await s.SendAsync("/api/v1/auth/logout", token);
        Assert.Equal(HttpStatusCode.NoContent, repeated.StatusCode); NoStore(repeated);
        Assert.Equal(2, s.Revocations.Writes); Assert.Equal(2, s.Quotas.Calls);
        await ProblemAsync(await s.SendAsync("/_058/protected/copied-logout", token), 401);
        Assert.Equal(2, s.Revocations.Writes);
        using var unaffected = await s.SendAsync("/_058/protected", other, method: HttpMethod.Get);
        Assert.Equal(HttpStatusCode.OK, unaffected.StatusCode); NoStore(unaffected);
        s.AssertSafeLogs(token, other);
    }

    [Theory, Trait("Task", "058")]
    [InlineData(null)]
    [InlineData("not-a-jwt")]
    [InlineData("aaa.bbb.ccc")]
    public async Task Fallback_rejects_missing_or_invalid_identity_with_exact_safe_challenge(string? token)
    {
        await using var s = await PipelineScope.CreateAsync(fixture);
        await ProblemAsync(await s.SendAsync("/_058/protected", token, method: HttpMethod.Get), 401);
        Assert.Equal(0, s.Revocations.Reads); Assert.Equal(0, s.Quotas.Calls);
        s.AssertSafeLogs(token);
    }

    [Theory, Trait("Task", "058")]
    [InlineData(TokenRevocationReadStatus.Unavailable)]
    [InlineData(TokenRevocationReadStatus.Uncertain)]
    public async Task Nonverifiable_revocation_is_503_not_a_challenge(TokenRevocationReadStatus status)
    {
        await using var s = await PipelineScope.CreateAsync(fixture);
        s.Revocations.ReadOutcome = status; var token = s.Issue();
        await ProblemAsync(await s.SendAsync("/_058/protected", token, method: HttpMethod.Get), 503);
        Assert.Equal(1, s.Events.Validated); Assert.Equal(1, s.Revocations.Reads);
        Assert.Equal(0, s.Revocations.Writes); s.AssertSafeLogs(token);
    }

    [Theory, Trait("Task", "058")]
    [InlineData(AuthenticationSecurityAuthorityResultStatus.Unavailable)]
    [InlineData(AuthenticationSecurityAuthorityResultStatus.Uncertain)]
    public async Task Nonverifiable_authority_is_503_before_revocation_and_public_validation(AuthenticationSecurityAuthorityResultStatus status)
    {
        await using var s = await PipelineScope.CreateAsync(fixture);
        s.Authority.ReadOutcome = status; var token = s.Issue();
        await ProblemAsync(await s.SendAsync("/_058/protected", token, method: HttpMethod.Get), 503);
        foreach (var path in new[] { "/api/v1/auth/register", "/api/v1/auth/login" })
            await ProblemAsync(await s.SendAsync(path, token, new StringContent("{", Encoding.UTF8, "application/json")), 503);
        Assert.Equal(0, s.Revocations.Reads); Assert.Equal(0, s.Quotas.Calls);
        Assert.Equal(1, s.Events.Validated); Assert.Equal(2, s.Events.PublicNoResult); s.AssertSafeLogs(token);
    }

    [Fact, Trait("Task", "058")]
    public async Task Real_host_initializes_existing_components_without_bootstrapping_Redis_and_has_one_scheme()
    {
        await using var s = await PipelineScope.CreateAsync(fixture, initializeQuota: false);
        var p = s.Services;
        Assert.True(p.GetRequiredService<DummyPasswordHash>().IsInitialized);
        Assert.Equal(MongoUsersInitializationStatus.Ready, p.GetRequiredService<AuthenticationDependencyAdmission>().IndexStatus);
        Assert.Same(p.GetRequiredService<JwtKeyRing>(), p.GetRequiredService<JwtKeyRing>());
        Assert.IsType<JwtTokenIssuer>(Assert.Single(p.GetServices<IAccessTokenIssuer>()));
        Assert.IsType<PasswordService>(Assert.Single(p.GetServices<IPasswordService>()));
        Assert.Single(p.GetServices<IAuthenticationQuotaStore>()); Assert.Single(p.GetServices<IAuthenticationSecurityAuthority>());
        var schemes = await p.GetRequiredService<IAuthenticationSchemeProvider>().GetAllSchemesAsync();
        Assert.Equal(AuthenticationPolicies.Scheme, Assert.Single(schemes).Name);
        var policies = p.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        Assert.Same(policies.DefaultPolicy, policies.FallbackPolicy);
        Assert.Contains(policies.DefaultPolicy.Requirements, r => r is Microsoft.AspNetCore.Authorization.Infrastructure.DenyAnonymousAuthorizationRequirement);
        Assert.IsType<AuthenticationAuthorizationResultHandler>(p.GetRequiredService<IAuthorizationMiddlewareResultHandler>());
        Assert.Single(p.GetServices<IAuthorizationHandler>().OfType<RevocationAuthorizationHandler>());
        var metadata = p.GetRequiredService<RedisAuthenticationQuotaConfiguration>().Keys.Metadata;
        Assert.False(await (await p.GetRequiredService<RedisConnections>().GetMultiplexerAsync()).GetDatabase().KeyExistsAsync(metadata));
        await ProblemAsync(await s.SendAsync("/api/v1/auth/login", body: JsonContent.Create(new { email = PipelineScope.Email, password = PipelineScope.Secret })), 503);
        using var health = await s.Client.GetAsync("/health"); Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        s.AssertSafeLogs();
    }

    [Theory, Trait("Task", "058")]
    [InlineData("/api/v1/auth/register", 413)]
    [InlineData("/api/v1/auth/login", 413)]
    [InlineData("/api/v1/auth/register", 415)]
    [InlineData("/api/v1/auth/login", 415)]
    public async Task Early_boundary_rejects_without_JWT_or_quota(string path, int status)
    {
        await using var s = await PipelineScope.CreateAsync(fixture);
        var token = s.Issue();
        using var body = new StringContent(status == 413 ? new string('x', 16385) : "{}", Encoding.UTF8,
            status == 413 ? "application/json" : "text/plain");
        await ProblemAsync(await s.SendAsync(path, token, body), status);
        Assert.Equal(0, s.Events.Received); Assert.Equal(0, s.Events.Validated);
        Assert.Equal(0, s.Revocations.Reads); Assert.Equal(0, s.Quotas.Calls); s.AssertSafeLogs(token);
    }

    private static void NoStore(HttpResponseMessage response)
    { Assert.Equal("no-store", response.Headers.CacheControl?.ToString()); Assert.False(response.Headers.Contains("Set-Cookie")); }

    private static async Task ProblemAsync(HttpResponseMessage response, int status)
    {
        using (response)
        {
            Assert.Equal(status, (int)response.StatusCode); NoStore(response);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(status == 401 ? "Bearer" : null, response.Headers.WwwAuthenticate.SingleOrDefault()?.ToString());
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(status, json.RootElement.GetProperty("status").GetInt32());
            Assert.Equal(response.Headers.GetValues("X-Correlation-ID").Single(), json.RootElement.GetProperty("traceId").GetString());
            Assert.Equal(status switch { 401 => "authentication_failed", 413 => "payload_too_large", 415 => "unsupported_media_type", _ => "temporarily_unavailable" },
                json.RootElement.GetProperty("code").GetString());
            Assert.False(json.RootElement.TryGetProperty("exception", out _));
        }
    }

    private sealed class PipelineScope : IAsyncDisposable
    {
        internal const string Email = "pipeline@uttt.edu.mx", Secret = "pipeline-synthetic-password";
        private readonly OwnedCompositionAuthority authority;
        private readonly AuthenticationApiFactory owner;
        private readonly WebApplicationFactory<AuthenticationOptions> factory;
        private readonly AuthenticationDependenciesFixture fixture;
        private readonly string database;
        private readonly List<string> marks = [];
        private readonly List<string> rawIdentities = [];
        private readonly RegistrationTestScope.Capture logs = new();
        internal IServiceProvider Services => factory.Services;
        internal HttpClient Client { get; }
        internal BearerObservations Events { get; } = new();
        internal LogoutRevocationProbe Revocations => Services.GetRequiredService<LogoutRevocationProbe>();
        internal LogoutAuthorityProbe Authority => Services.GetRequiredService<LogoutAuthorityProbe>();
        internal RegistrationTestScope.QuotaProbe Quotas => Services.GetRequiredService<RegistrationTestScope.QuotaProbe>();

        private PipelineScope(AuthenticationDependenciesFixture fixture, OwnedCompositionAuthority authority)
        {
            this.fixture = fixture; this.authority = authority;
            var id = Guid.NewGuid().ToString("N"); database = fixture.MongoDatabase + "_058_" + id[..8];
            owner = new AuthenticationApiFactory(fixture, new Dictionary<string, string?>
            { ["MongoDb:DatabaseName"] = database, ["AuthenticationSecurityAuthority:TableName"] = authority.TableName });
            factory = owner.WithWebHostBuilder(web => web.ConfigureTestServices(services =>
            {
                services.AddSingleton<IStartupFilter, SyntheticPeer>(); services.AddSingleton<ILoggerProvider>(logs);
                services.AddControllers().ConfigureApplicationPartManager(m => m.FeatureProviders.Add(new ProbeFeature()));
                services.AddSingleton(p => p.GetRequiredService<IOptions<AuthenticationDependencyOptions>>().Value.Capture("pipeline_058_" + id,
                    p.GetRequiredService<IOptions<RateLimitingOptions>>().Value));
                var revocation = services.Single(d => d.ServiceType == typeof(ITokenRevocationStore)); services.Remove(revocation);
                services.AddSingleton(p => new LogoutRevocationProbe((ITokenRevocationStore)revocation.ImplementationFactory!(p)));
                services.AddSingleton<ITokenRevocationStore>(p => p.GetRequiredService<LogoutRevocationProbe>());
                var state = services.Single(d => d.ServiceType == typeof(IAuthenticationSecurityAuthority)); services.Remove(state);
                services.AddSingleton(p => new LogoutAuthorityProbe((IAuthenticationSecurityAuthority)state.ImplementationFactory!(p)));
                services.AddSingleton<IAuthenticationSecurityAuthority>(p => p.GetRequiredService<LogoutAuthorityProbe>());
                var quota = services.Single(d => d.ServiceType == typeof(IAuthenticationQuotaStore)); services.Remove(quota);
                services.AddSingleton(p => new RegistrationTestScope.QuotaProbe((IAuthenticationQuotaStore)quota.ImplementationFactory!(p)));
                services.AddSingleton<IAuthenticationQuotaStore>(p => p.GetRequiredService<RegistrationTestScope.QuotaProbe>());
                services.PostConfigure<JwtBearerOptions>(AuthenticationPolicies.Scheme, options =>
                {
                    var received = options.Events.OnMessageReceived; var validated = options.Events.OnTokenValidated; var failed = options.Events.OnAuthenticationFailed;
                    options.Events.OnMessageReceived = async context =>
                    {
                        Events.Received++; await received(context);
                        if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() is not null && context.Result?.None == true)
                            Events.PublicNoResult++;
                    };
                    options.Events.OnTokenValidated = async context => { Events.Validated++; await validated(context); };
                    options.Events.OnAuthenticationFailed = async context => { Events.Failed++; await failed(context); };
                });
            }));
            Client = factory.CreateClient();
        }

        internal static async Task<PipelineScope> CreateAsync(AuthenticationDependenciesFixture fixture, bool initializeQuota = true)
        {
            var authority = await OwnedCompositionAuthority.CreateAsync(fixture, open: true);
            PipelineScope? scope = null;
            try
            {
                scope = new(fixture, authority); var p = scope.Services;
                // PING previo solo de setup, sin retry funcional ni cambio de timeouts/budgets.
                await (await p.GetRequiredService<RedisConnections>().GetMultiplexerAsync()).GetDatabase().PingAsync();
                if (initializeQuota)
                {
                    using var setup = new RequestBudget(8000, default);
                    var lifecycle = new RedisAuthenticationQuotaLifecycle(p.GetRequiredService<RedisConnections>(), p.GetRequiredService<IOptions<RedisOptions>>().Value,
                        p.GetRequiredService<RedisAuthenticationQuotaConfiguration>(), p.GetRequiredService<RedisSecurityStateGuard>(), p.GetRequiredService<AuthenticationDependencyAdmission>());
                    Assert.Equal("initialized", (await lifecycle.InitializeVerifiedNewEnvironmentAsync(setup)).Status);
                }
                return scope;
            }
            catch { if (scope is not null) await scope.DisposeAsync(); else await authority.DisposeAsync(); throw; }
        }

        internal string Issue()
        {
            var result = Services.GetRequiredService<IAccessTokenIssuer>().Issue(Guid.NewGuid());
            Assert.Equal(AccessTokenIssueStatus.Issued, result.Status); Track(result.AccessToken!); return result.AccessToken!;
        }
        internal void Track(string token)
        {
            var jwt = new JsonWebToken(token); var environment = fixture.RedisNamespace["enver:".Length..];
            rawIdentities.Add(jwt.GetClaim("sub").Value); rawIdentities.Add(jwt.GetClaim("jti").Value);
            marks.Add(RedisKeyBuilder.BuildRevocationKey(environment, "enver-api", jwt.GetClaim("jti").Value));
        }
        internal Task<HttpResponseMessage> SendAsync(string path, string? token = null, HttpContent? body = null, HttpMethod? method = null)
        {
            var request = new HttpRequestMessage(method ?? HttpMethod.Post, path) { Content = body };
            if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return Client.SendAsync(request);
        }
        internal void AssertSafeLogs(params string?[] sensitive)
        {
            var text = string.Join('\n', logs.Messages);
            var jwt = Services.GetRequiredService<IOptions<JwtOptions>>().Value;
            var quota = Services.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
            foreach (var value in sensitive.Concat(rawIdentities).Concat(new[] { Email, Secret, fixture.MongoConnectionString, fixture.RedisConnectionString,
                jwt.Keys[0]!.KeyBase64, quota.PseudonymizationKeyBase64 }).Where(v => !string.IsNullOrEmpty(v)))
                Assert.True(!text.Contains(value!, StringComparison.Ordinal), "Sensitive value in captured logs.");
            Assert.False(fixture.ContainsSyntheticSecret(text));
        }
        public async ValueTask DisposeAsync()
        {
            try
            {
                var mux = await Services.GetRequiredService<RedisConnections>().GetMultiplexerAsync();
                var keys = marks.Select(k => (RedisKey)k).ToList();
                var prefix = Services.GetRequiredService<RedisAuthenticationQuotaConfiguration>().Keys.Prefix;
                await foreach (var key in mux.GetServer(mux.GetEndPoints()[0]).KeysAsync(pattern: prefix + "*")) keys.Add(key);
                if (keys.Count > 0) await mux.GetDatabase().KeyDeleteAsync(keys.ToArray());
                await Services.GetRequiredService<IMongoClient>().DropDatabaseAsync(database);
            }
            finally { Client.Dispose(); await factory.DisposeAsync(); await owner.DisposeAsync(); await authority.DisposeAsync(); }
        }
    }

    private sealed class BearerObservations { internal int Received, PublicNoResult, Validated, Failed; }
    private sealed class SyntheticPeer : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        { app.Use(async (context, proceed) => { context.Connection.RemoteIpAddress = IPAddress.Loopback; await proceed(context); }); next(app); };
    }
    private sealed class ProbeFeature : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature) =>
            feature.Controllers.Add(typeof(Pipeline058ProbeController).GetTypeInfo());
    }
}

/// <summary>Solo se descubre por ProbeFeature en el factory 058; nunca en el assembly productivo.</summary>
[ApiController, Route("_058/protected")]
public sealed class Pipeline058ProbeController : ControllerBase
{
    [HttpGet] public IActionResult Fallback() => Ok("authorized");
    [HttpGet("default"), Authorize] public IActionResult Default() => Ok("authorized");
    [HttpGet("normal"), Authorize(Policy = AuthenticationPolicies.Normal)] public IActionResult Normal() => Ok("authorized");
    [HttpPost("copied-logout"), Authorize(Policy = AuthenticationPolicies.Logout)] public IActionResult CopiedLogout() => Ok("must-not-authorize");
}
