using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.API.Security;
using Enver.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class JwtCheckpointTests(AuthenticationDependenciesFixture dependencies)
{
    [Fact, Trait("Task", "042")]
    public async Task Two_logical_hosts_cross_validate_real_emissions_and_Register_remains_operational()
    {
        await using var scope = await RegistrationTestScope.CreateAsync(dependencies);
        var options = Options();
        await using var a = await Host.CreateAsync(scope, options);
        await using var b = await Host.CreateAsync(scope, options);
        var subject = Guid.NewGuid(); var first = a.Issuer.Issue(subject); var second = b.Issuer.Issue(subject);
        using var p = Payload(first.AccessToken!); using var q = Payload(second.AccessToken!);
        Assert.Equal(p.RootElement.GetProperty("sub").GetString(), q.RootElement.GetProperty("sub").GetString());
        Assert.NotEqual(p.RootElement.GetProperty("jti").GetString(), q.RootElement.GetProperty("jti").GetString());
        Assert.Equal(HttpStatusCode.OK, (await a.SendAsync(second.AccessToken!)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await b.SendAsync(first.AccessToken!)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await scope.SendAsync()).StatusCode);
        var endpoints = scope.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>().ToArray();
        var login = Assert.Single(endpoints, e => e.RoutePattern.RawText == "api/v1/auth/login");
        Assert.NotNull(login.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.Equal(AuthenticationOperation.Login, login.Metadata.GetMetadata<AuthenticationRequestMetadata>()!.Operation);
        // 058 activa el mismo pipeline en el Program real usado por RegistrationTestScope.
        var logout = Assert.Single(endpoints, e => e.RoutePattern.RawText == "api/v1/auth/logout");
        Assert.Equal(AuthenticationPolicies.Logout, Assert.Single(logout.Metadata.GetOrderedMetadata<IAuthorizeData>()).Policy);
        Assert.Equal(AuthenticationOperation.Logout, logout.Metadata.GetMetadata<AuthenticationRequestMetadata>()!.Operation);
        var schemes = scope.Services.GetRequiredService<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
        Assert.Equal(AuthenticationPolicies.Scheme, Assert.Single(await schemes.GetAllSchemesAsync()).Name);
        Assert.All(a.Server.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>(),
            e => Assert.DoesNotContain("/api/v1/auth/", e.RoutePattern.RawText!));
    }

    [Fact, Trait("Task", "040"), Trait("Task", "042")]
    public async Task Real_Redis_mark_is_rejected_by_both_hosts_without_a_Logout_endpoint()
    {
        await using var scope = await RegistrationTestScope.CreateAsync(dependencies);
        var options = Options(); await using var a = await Host.CreateAsync(scope, options); await using var b = await Host.CreateAsync(scope, options);
        var issued = a.Issuer.Issue(Guid.NewGuid()); using var p = Payload(issued.AccessToken!);
        var jti = p.RootElement.GetProperty("jti").GetString()!;
        var redis = scope.Services.GetRequiredService<IOptions<RedisOptions>>().Value;
        var environment = redis.NamespacePrefix["enver:".Length..];
        var key = RedisKeyBuilder.BuildRevocationKey(environment, "enver-api", jti);
        try
        {
            Assert.Equal(HttpStatusCode.OK, (await b.SendAsync(issued.AccessToken!)).StatusCode);
            using var budget = new RequestBudget(1000, default);
            // Setup sintético por el store 017 existente, no flujo/escritura Logout nuevo.
            var mark = await a.Store.RevokeAsync(new("enver-api", jti, issued.ExpiresAtUtc!.Value,
                TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(redis.RevocationOperationalMarginSeconds)), budget);
            Assert.Equal(TokenRevocationWriteStatus.Confirmed, mark.Status);
            await ProblemAsync(await a.SendAsync(issued.AccessToken!), 401);
            await ProblemAsync(await b.SendAsync(issued.AccessToken!), 401);
        }
        finally { await (await a.Connections.GetMultiplexerAsync()).GetDatabase().KeyDeleteAsync(key); }
    }

    [Fact, Trait("Task", "040"), Trait("Task", "042")]
    public async Task Uncertain_result_after_real_read_is_503_not_401()
    {
        await using var scope = await RegistrationTestScope.CreateAsync(dependencies);
        await using var host = await Host.CreateAsync(scope, Options(), uncertain: true);
        await ProblemAsync(await host.SendAsync(host.Issuer.Issue(Guid.NewGuid()).AccessToken!), 503);
        Assert.Equal(1, host.Probe.Reads);
    }

    [Fact, Trait("Task", "040"), Trait("Task", "042")]
    public async Task Stopped_real_Redis_is_fail_closed_for_a_cryptographically_valid_identity()
    {
        var scope = await RegistrationTestScope.CreateAsync(dependencies);
        try
        {
            await using var host = await Host.CreateAsync(scope, Options());
            var token = host.Issuer.Issue(Guid.NewGuid()).AccessToken!;
            Assert.Equal(HttpStatusCode.OK, (await host.SendAsync(token)).StatusCode);
            try
            {
                await dependencies.StopRedisAsync();
                await ProblemAsync(await host.SendAsync(token), 503);
            }
            finally { await dependencies.StartRedisAsync(); }
        }
        finally
        {
            var original = scope.Services.GetRequiredService<IOptions<RedisOptions>>().Value;
            // StartRedisAsync puede remapear el puerto publicado. Solo el cleanup usa el endpoint actual
            // del harness; los clientes de seguridad anteriores permanecen cerrados y no se reutilizan.
            await using var cleanup = new RedisConnections(new RedisOptions
            {
                ConnectionString = dependencies.RedisConnectionString,
                ConnectionTimeoutMilliseconds = original.ConnectionTimeoutMilliseconds,
                OperationTimeoutMilliseconds = original.OperationTimeoutMilliseconds
            });
            await scope.DisposeWithRedisCleanupAsync(cleanup);
        }
    }

    [Fact, Trait("Task", "041"), Trait("Task", "042")]
    public async Task Real_Bearer_rotation_coexists_and_retires_with_a_controlled_clock()
    {
        await using var scope = await RegistrationTestScope.CreateAsync(dependencies);
        var clock = new Clock(); var options = Options();
        await using var a = await Host.CreateAsync(scope, options, clock);
        var tokenA = a.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        options.Keys.Add(new() { Kid = "B", KeyBase64 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) });
        await using var distributed = await Host.CreateAsync(scope, options, clock);
        options.ActiveKid = "B"; await using var b = await Host.CreateAsync(scope, options, clock);
        var tokenB = b.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        Assert.Equal(HttpStatusCode.OK, (await distributed.SendAsync(tokenB)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await b.SendAsync(tokenA)).StatusCode);
        var ceased = clock.Now; clock.Now = ceased.AddSeconds(930).AddTicks(-1);
        Assert.False(JwtKeyRing.CanRetirePreviousKey(ceased, clock.Now));
        Assert.Equal(HttpStatusCode.OK, (await b.SendAsync(tokenA)).StatusCode);
        clock.Now = ceased.AddSeconds(930); Assert.True(JwtKeyRing.CanRetirePreviousKey(ceased, clock.Now));
        await ProblemAsync(await b.SendAsync(tokenA), 401);
        options.Keys.RemoveAt(0); await using var retired = await Host.CreateAsync(scope, options, clock);
        await ProblemAsync(await retired.SendAsync(tokenA), 401);
        var fresh = b.Issuer.Issue(Guid.NewGuid()).AccessToken!;
        Assert.Equal(HttpStatusCode.OK, (await retired.SendAsync(fresh)).StatusCode);
    }

    private static JwtOptions Options() => new() { Algorithm = "HS256", Issuer = "enver-api", Audience = "enver-mobile", LifetimeSeconds = 900,
        ClockSkewSeconds = 30, ActiveKid = "A", Keys = [new() { Kid = "A", KeyBase64 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) }] };
    private static JsonDocument Payload(string token) => JsonDocument.Parse(Base64UrlEncoder.DecodeBytes(token.Split('.')[1]));
    private static async Task ProblemAsync(HttpResponseMessage response, int expected)
    {
        using (response)
        {
            Assert.Equal(expected, (int)response.StatusCode); Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(expected == 401 ? "Bearer" : null, response.Headers.WwwAuthenticate.SingleOrDefault()?.ToString());
            using var p = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(expected == 401 ? "authentication_failed" : "temporarily_unavailable", p.RootElement.GetProperty("code").GetString());
        }
    }
    private sealed class Clock : TimeProvider
    { public DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); public override DateTimeOffset GetUtcNow() => Now; }

    private sealed class Host : IAsyncDisposable
    {
        public TestServer Server { get; }
        private readonly HttpClient client;
        private readonly IHost application;
        public JwtTokenIssuer Issuer { get; }
        public RedisConnections Connections { get; }
        public RedisTokenRevocationStore Store { get; }
        public ReadProbe Probe { get; }
        private Host(RegistrationTestScope scope, JwtOptions options, TimeProvider clock, bool uncertain)
        {
            var p = scope.Services; var redis = p.GetRequiredService<IOptions<RedisOptions>>().Value;
            Connections = new RedisConnections(redis);
            var state = p.GetRequiredService<IAuthenticationSecurityState>(); Store = new(Connections, state, redis);
            Probe = new(Store, uncertain);
            var ring = new JwtKeyRing(options, p.GetRequiredService<IOptions<RateLimitingOptions>>().Value);
            Issuer = new(ring, clock); var validator = new JwtClaimsValidator(ring, clock);
            application = new HostBuilder().ConfigureWebHost(web => web.UseTestServer().ConfigureServices(services =>
            {
                services.AddRouting(); ApiProblemDetails.AddSafeApiErrors(services);
                services.AddAuthentication(AuthenticationPolicies.Scheme).AddJwtBearer();
                services.AddSingleton<IConfigureOptions<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>>(new JwtBearerConfiguration(validator));
                services.AddAuthorization(AuthenticationPolicies.Configure);
                services.AddSingleton<IAuthenticationSecurityState>(state); services.AddSingleton<ITokenRevocationStore>(Probe);
                services.AddSingleton(p.GetRequiredService<IOptions<AuthenticationOptions>>());
                services.AddSingleton<IAuthorizationHandler, RevocationAuthorizationHandler>();
                services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthenticationAuthorizationResultHandler>();
            }).Configure(app =>
            {
                app.UseMiddleware<RequestContextMiddleware>(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
                app.UseEndpoints(e => e.MapGet("/_jwt", () => "ok").RequireAuthorization());
            })).Start(); Server = application.GetTestServer();
            client = Server.CreateClient();
        }
        public static async Task<Host> CreateAsync(RegistrationTestScope scope, JwtOptions options, TimeProvider? clock = null, bool uncertain = false)
        {
            var host = new Host(scope, options, clock ?? TimeProvider.System, uncertain);
            try { await (await host.Connections.GetMultiplexerAsync()).GetDatabase().PingAsync(); return host; }
            catch { await host.DisposeAsync(); throw; }
        }
        public Task<HttpResponseMessage> SendAsync(string token)
        { var request = new HttpRequestMessage(HttpMethod.Get, "/_jwt"); request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); return client.SendAsync(request); }
        public async ValueTask DisposeAsync() { client.Dispose(); application.Dispose(); await Connections.DisposeAsync(); }
    }
    private sealed class ReadProbe(ITokenRevocationStore inner, bool uncertain) : ITokenRevocationStore
    {
        public int Reads;
        public async ValueTask<TokenRevocationReadResult> CheckAsync(TokenRevocationReadRequest request, RequestBudget budget)
        {
            Reads++; var result = await inner.CheckAsync(request, budget);
            return uncertain && result.Status == TokenRevocationReadStatus.NotRevoked ? new(TokenRevocationReadStatus.Uncertain) : result;
        }
        public ValueTask<TokenRevocationWriteResult> RevokeAsync(TokenRevocationWriteRequest request, RequestBudget budget) =>
            throw new InvalidOperationException("La política no escribe.");
    }
}
