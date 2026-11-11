using Enver.API.Configuration;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using Amazon.DynamoDBv2.Model;
using Enver.API.Composition;
using Enver.API.Features.Authentication.Application;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.API.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using AuthenticationOptions = Enver.API.Configuration.AuthenticationOptions;

namespace Enver.Testing;

public sealed class AuthenticationApiFactory : WebApplicationFactory<AuthenticationOptions>
{
    private readonly AuthenticationDependenciesFixture _dependencies;
    private readonly IReadOnlyDictionary<string, string?> _configuration;

    public AuthenticationApiFactory(AuthenticationDependenciesFixture dependencies, IReadOnlyDictionary<string, string?>? overrides = null)
    {
        _dependencies = dependencies;
        // Wiring sintético de 057; fixture/imágenes/topología intactas, sin recursos AWS reales.
        var values = new Dictionary<string, string?>
        {
            ["AuthenticationComposition:AuthorityRegion"] = "us-east-1",
            ["AuthenticationComposition:QuotaConfigurationVersion"] = "config-021",
            ["AuthenticationComposition:QuotaIdentityReference"] = "identity-053",
            ["AuthenticationComposition:QuotaGeneration"] = dependencies.RunId,
            ["RateLimiting:MaxActivePartitions"] = "5000",
            ["AuthenticationTransport:DirectConnectionApproved"] = "true",
            ["AuthenticationTransport:AllowLocalHttp"] = "true"
        };
        if (overrides is not null) foreach (var pair in overrides) values[pair.Key] = pair.Value;
        _configuration = dependencies.CreateConfiguration(values);
    }

    public AuthenticationDependenciesFixture Dependencies => _dependencies;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_dependencies.EnvironmentName);
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(_configuration));
        builder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IAmazonDynamoDB>(_ => _dependencies.CreateDynamoDbClient());
            services.AddSingleton<TestHostLocalState>();
            services.AddSingleton(new TestHostExternalDependencyProbe(_dependencies));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(_dependencies.EnvironmentName);
        return base.CreateHost(builder);
    }
}

public sealed class TestHostLocalState
{
    public Guid InstanceId { get; } = Guid.NewGuid();
    public Dictionary<string, string> Values { get; } = new(StringComparer.Ordinal);
}

public sealed class TestHostExternalDependencyProbe(AuthenticationDependenciesFixture dependencies)
{
    public Task WriteMongoAsync(string marker, CancellationToken cancellationToken = default) => dependencies.WriteMongoAsync(marker, cancellationToken);
    public Task<bool> ContainsMongoAsync(string marker, CancellationToken cancellationToken = default) => dependencies.ContainsMongoAsync(marker, cancellationToken);
    public Task WriteRedisAsync(string marker, CancellationToken cancellationToken = default) => dependencies.WriteRedisAsync(marker, cancellationToken);
    public Task<bool> ContainsRedisAsync(string marker, CancellationToken cancellationToken = default) => dependencies.ContainsRedisAsync(marker, cancellationToken);
}

/// <summary>Opt-in 043–046. Hosts propios; ningún cambio en el factory/Program habitual ni recursos AWS reales.</summary>
public sealed class LogoutTestScope : IAsyncDisposable
{
    private readonly AuthenticationDependenciesFixture fixture;
    private readonly IAmazonDynamoDB authorityClient;
    private readonly List<LogoutTestHost> hosts = [];
    private readonly HashSet<string> marks = [];
    private readonly string table = "enver_logout_" + Guid.NewGuid().ToString("N");
    private readonly string environment = "t015_logout_" + Guid.NewGuid().ToString("N");
    private bool mongoUnavailable;
    public LogoutTestHost First => hosts[0];
    public LogoutLogCapture Logs { get; } = new();
    private LogoutTestScope(AuthenticationDependenciesFixture fixture) { this.fixture = fixture; authorityClient = fixture.CreateDynamoDbClient(); }

    public static async Task<LogoutTestScope> CreateAsync(AuthenticationDependenciesFixture fixture, bool mongoUnavailable = false)
    {
        var scope = new LogoutTestScope(fixture);
        scope.mongoUnavailable = mongoUnavailable;
        var created = false;
        try
        {
            await scope.authorityClient.CreateTableAsync(new CreateTableRequest
            {
                TableName = scope.table, AttributeDefinitions = [new("Id", ScalarAttributeType.S)],
                KeySchema = [new("Id", KeyType.HASH)], ProvisionedThroughput = new(1, 1)
            });
            created = true;
            await scope.authorityClient.PutItemAsync(new PutItemRequest { TableName = scope.table, Item = new()
            {
                ["Id"] = new() { S = "authentication" }, ["State"] = new() { S = "Open" },
                ["Generation"] = new() { N = "0" }, ["Revision"] = new() { N = "0" }
            }, ConditionExpression = "attribute_not_exists(Id)" });
            var host = await scope.AddHostAsync(mongoUnavailable);
            var p = host.Services;
            using var setup = new RequestBudget(4000, default);
            var lifecycle = new RedisAuthenticationQuotaLifecycle(p.GetRequiredService<RedisConnections>(), p.GetRequiredService<IOptions<RedisOptions>>().Value,
                p.GetRequiredService<RedisAuthenticationQuotaConfiguration>(), p.GetRequiredService<RedisSecurityStateGuard>(), p.GetRequiredService<AuthenticationDependencyAdmission>());
            var initialized = await lifecycle.InitializeVerifiedNewEnvironmentAsync(setup);
            if (initialized.Status != "initialized") throw new InvalidOperationException("No se inicializó la cuota de prueba.");
            return scope;
        }
        catch
        {
            if (created) await scope.DisposeAsync(); else scope.authorityClient.Dispose();
            throw;
        }
    }

    public async Task<LogoutTestHost> AddHostAsync(bool mongoUnavailable = false)
    {
        var values = new Dictionary<string, string?>
        {
            ["AuthenticationComposition:AuthorityRegion"] = "us-east-1", ["AuthenticationComposition:QuotaConfigurationVersion"] = "config-021",
            ["AuthenticationComposition:QuotaIdentityReference"] = "identity-053", ["AuthenticationComposition:QuotaGeneration"] = fixture.RunId,
            ["RateLimiting:MaxActivePartitions"] = "5000",
            ["AuthenticationTransport:DirectConnectionApproved"] = "true", ["AuthenticationTransport:AllowLocalHttp"] = "true",
            ["AuthenticationSecurityAuthority:TableName"] = table, ["Redis:NamespacePrefix"] = "enver:" + environment,
            ["MongoDb:DatabaseName"] = "enver_" + environment,
            // Perfil aprobado Logout: no cambia Options/appsettings, límites Redis ni el factory base.
            ["Authentication:LogoutBudgetMilliseconds"] = "4000", ["Redis:RevocationOperationalMarginSeconds"] = "60"
        };
        if (mongoUnavailable) values["MongoDb:ConnectionString"] = "mongodb://127.0.0.1:1/?directConnection=true";
        var configuration = fixture.CreateConfiguration(values);
        var host = new LogoutTestHost(fixture, configuration, Logs);
        hosts.Add(host);
        await host.StartAsync();
        return host;
    }

    public void Track(LogoutInput input) => marks.Add(RedisKeyBuilder.BuildRevocationKey(environment, input.Issuer, input.Jti));
    public string MarkKey(LogoutInput input) { Track(input); return RedisKeyBuilder.BuildRevocationKey(environment, input.Issuer, input.Jti); }
    public async Task RemoveAuthorityAsync() => await authorityClient.DeleteItemAsync(new DeleteItemRequest
    { TableName = table, Key = new() { ["Id"] = new() { S = "authentication" } } });

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (hosts.Count > 0)
            {
                // Cleanup siempre al endpoint vigente, sin sustituir clientes/estado de seguridad del request.
                var original = First.Services.GetRequiredService<IOptions<RedisOptions>>().Value;
                await using var cleanup = new RedisConnections(new RedisOptions { ConnectionString = fixture.RedisConnectionString,
                    ConnectionTimeoutMilliseconds = original.ConnectionTimeoutMilliseconds, OperationTimeoutMilliseconds = original.OperationTimeoutMilliseconds });
                var mux = await cleanup.GetMultiplexerAsync();
                var keys = marks.Select(k => (RedisKey)k).ToList();
                var prefix = First.Services.GetRequiredService<RedisAuthenticationQuotaConfiguration>().Keys.Prefix;
                await foreach (var key in mux.GetServer(mux.GetEndPoints()[0]).KeysAsync(pattern: prefix + "*")) keys.Add(key);
                if (keys.Count > 0) await mux.GetDatabase().KeyDeleteAsync(keys.ToArray());
                if (!mongoUnavailable) await First.Services.GetRequiredService<MongoDB.Driver.IMongoClient>().DropDatabaseAsync("enver_" + environment);
            }
        }
        finally
        {
            try { foreach (var host in hosts.AsEnumerable().Reverse()) await host.DisposeAsync(); }
            finally
            {
                try { await authorityClient.DeleteTableAsync(new DeleteTableRequest { TableName = table }); }
                finally { authorityClient.Dispose(); }
            }
        }
    }
}

public sealed class LogoutTestHost : IAsyncDisposable
{
    private readonly IHost host;
    private bool started;
    public IServiceProvider Services => host.Services;
    public HttpClient Client { get; private set; } = null!;
    public LogoutRevocationProbe Revocations => Services.GetRequiredService<LogoutRevocationProbe>();
    public LogoutQuotaProbe Quotas => Services.GetRequiredService<LogoutQuotaProbe>();
    public LogoutAuthorityProbe Authority => Services.GetRequiredService<LogoutAuthorityProbe>();
    public JwtOptions Jwt => Services.GetRequiredService<IOptions<JwtOptions>>().Value;
    public JwtTokenIssuer Issuer { get; private set; } = null!;

    internal LogoutTestHost(AuthenticationDependenciesFixture fixture, IReadOnlyDictionary<string, string?> configuration, LogoutLogCapture logs)
    {
        host = new HostBuilder().ConfigureLogging(l => { l.ClearProviders(); l.AddProvider(logs); })
            .ConfigureWebHost(web => web.UseTestServer().UseEnvironment("T015_" + configuration["Redis:NamespacePrefix"]!["enver:t015_".Length..])
                .ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(configuration))
                .ConfigureServices((context, services) =>
                {
                    services.AddControllers().AddApplicationPart(typeof(Enver.API.Controllers.AuthenticationController).Assembly);
                    services.AddAuthenticationFoundation(context.Configuration, context.HostingEnvironment);
                    services.AddSingleton<IAmazonDynamoDB>(_ => fixture.CreateDynamoDbClient());
                    services.AddAuthenticationDependencies(context.Configuration);
                    var authority = services.Last(d => d.ServiceType == typeof(IAuthenticationSecurityAuthority)); services.Remove(authority);
                    services.AddSingleton(p => new LogoutAuthorityProbe((IAuthenticationSecurityAuthority)authority.ImplementationFactory!(p)));
                    services.AddSingleton<IAuthenticationSecurityAuthority>(p => p.GetRequiredService<LogoutAuthorityProbe>());
                    services.AddSingleton<LogoutRevocationProbe>(p => new(new RedisTokenRevocationStore(p.GetRequiredService<RedisConnections>(),
                        p.GetRequiredService<IAuthenticationSecurityState>(), p.GetRequiredService<IOptions<RedisOptions>>().Value)));
                    services.AddSingleton<ITokenRevocationStore>(p => p.GetRequiredService<LogoutRevocationProbe>());
                    var quota = services.Last(d => d.ServiceType == typeof(IAuthenticationQuotaStore)); services.Remove(quota);
                    services.AddSingleton(p => new LogoutQuotaProbe((IAuthenticationQuotaStore)quota.ImplementationFactory!(p)));
                    services.AddSingleton<IAuthenticationQuotaStore>(p => p.GetRequiredService<LogoutQuotaProbe>());
                    services.AddSingleton(p => new JwtKeyRing(p.GetRequiredService<IOptions<JwtOptions>>().Value, p.GetRequiredService<IOptions<RateLimitingOptions>>().Value));
                    services.AddSingleton(p => new JwtClaimsValidator(p.GetRequiredService<JwtKeyRing>(), TimeProvider.System));
                    services.AddAuthentication(AuthenticationPolicies.Scheme).AddJwtBearer();
                    services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerConfiguration>();
                    services.AddAuthorization(AuthenticationPolicies.Configure);
                    services.AddSingleton<IAuthorizationHandler, RevocationAuthorizationHandler>();
                    services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthenticationAuthorizationResultHandler>();
                }).Configure(app =>
                {
                    app.Use(async (http, next) => { http.Connection.RemoteIpAddress = IPAddress.Loopback; await next(); });
                    app.UseAuthenticationFoundation(); app.UseAuthentication(); app.UseAuthorization();
                    app.UseEndpoints(e =>
                    {
                        e.MapControllers(); e.MapGet("/_logout/protected", () => "ok").RequireAuthorization();
                        e.MapPost("/_logout/copied", () => "unsafe").RequireAuthorization(AuthenticationPolicies.Logout);
                        e.MapGet("/api/v1/auth/logout", () => "unsafe").RequireAuthorization(AuthenticationPolicies.Logout);
                    });
                })).Build();
    }
    internal async Task StartAsync()
    {
        await host.StartAsync(); started = true; Client = host.GetTestClient();
        Issuer = new(Services.GetRequiredService<JwtKeyRing>(), TimeProvider.System);
        // Conectar fuera del presupuesto funcional; nunca reintentar el request.
        await (await Services.GetRequiredService<RedisConnections>().GetMultiplexerAsync()).GetDatabase().PingAsync();
    }
    public async Task<LogoutInput> InputAsync(string token)
    {
        var validator = Services.GetRequiredService<JwtClaimsValidator>(); var result = await validator.ValidateAsync(token);
        if (!result.IsValid) throw new InvalidOperationException("Fixture JWT no válida.");
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(result.ClaimsIdentity) };
        var options = new JwtBearerOptions(); new JwtBearerConfiguration(validator).Configure(options);
        await options.Events.TokenValidated(new TokenValidatedContext(http, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), options)
        { Principal = http.User, SecurityToken = result.SecurityToken });
        if (!LogoutInput.TryFromAuthenticatedContext(http, out var input)) throw new InvalidOperationException("Input no autenticado.");
        return input!;
    }
    public Task<HttpResponseMessage> SendAsync(string? token, string path = "/api/v1/auth/logout", HttpMethod? method = null, HttpContent? body = null)
    {
        var request = new HttpRequestMessage(method ?? HttpMethod.Post, path) { Content = body };
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return Client.SendAsync(request);
    }
    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        try { if (started) await host.StopAsync(); }
        finally { host.Dispose(); }
    }
}

public sealed class LogoutRevocationProbe(ITokenRevocationStore inner) : ITokenRevocationStore
{
    public int Writes, Reads;
    public TokenRevocationWriteStatus? WriteOutcome;
    public TokenRevocationReadStatus? ReadOutcome;
    public Action? AfterWrite;
    public async ValueTask<TokenRevocationWriteResult> RevokeAsync(TokenRevocationWriteRequest request, RequestBudget budget)
    {
        Interlocked.Increment(ref Writes);
        if (WriteOutcome is TokenRevocationWriteStatus.Unavailable or TokenRevocationWriteStatus.NoOperation)
            return new(WriteOutcome.Value);
        var result = await inner.RevokeAsync(request, budget); AfterWrite?.Invoke();
        return WriteOutcome == TokenRevocationWriteStatus.Uncertain && result.Status == TokenRevocationWriteStatus.Confirmed
            ? new(TokenRevocationWriteStatus.Uncertain) : result;
    }
    public async ValueTask<TokenRevocationReadResult> CheckAsync(TokenRevocationReadRequest request, RequestBudget budget)
    {
        Interlocked.Increment(ref Reads); var result = await inner.CheckAsync(request, budget);
        return ReadOutcome is { } status ? new(status) : result;
    }
}

public sealed class LogoutQuotaProbe(IAuthenticationQuotaStore inner) : IAuthenticationQuotaStore
{
    public int Calls;
    public AuthenticationQuotaStoreStatus? Outcome;
    public Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaConsumeRequest? Last;
    public async ValueTask<Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaConsumeResult> EvaluateAndConsumeAsync(
        Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaConsumeRequest request, RequestBudget budget)
    {
        Interlocked.Increment(ref Calls); Last = request;
        if (Outcome == Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaStoreStatus.Unavailable) return new(Outcome.Value);
        var result = await inner.EvaluateAndConsumeAsync(request, budget);
        return Outcome == Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaStoreStatus.Uncertain && result.Status == Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaStoreStatus.Admitted
            ? new(Outcome.Value) : result;
    }
}

/// <summary>Solo opt-in Logout: conserva la autoridad real, puede ocultar su respuesta de lectura.</summary>
public sealed class LogoutAuthorityProbe(IAuthenticationSecurityAuthority inner) : IAuthenticationSecurityAuthority
{
    public AuthenticationSecurityAuthorityResultStatus? ReadOutcome;
    public async ValueTask<AuthenticationSecurityAuthorityReadResult> ReadAsync(RequestBudget budget)
    {
        var result = await inner.ReadAsync(budget);
        return ReadOutcome is { } status ? new(status) : result;
    }
    public ValueTask<AuthenticationSecurityAuthorityCloseResult> MergeClosedAsync(CloseAuthenticationSecurityRequest request, RequestBudget budget) => inner.MergeClosedAsync(request, budget);
    public ValueTask<AuthenticationSecurityAuthorityPotentialIssuanceResult> RegisterPotentialIssuanceAsync(RegisterPotentialIssuanceRequest request, RequestBudget budget) => inner.RegisterPotentialIssuanceAsync(request, budget);
    public ValueTask<AuthenticationSecurityAuthorityReopenResult> TryReopenAsync(TryReopenAuthenticationSecurityRequest request, RequestBudget budget) => inner.TryReopenAsync(request, budget);
}

/// <summary>Captura explícita de eventos de aplicación, no certifica todos los proveedores de logs productivos.</summary>
public sealed class LogoutLogCapture : ILoggerProvider
{
    public ConcurrentQueue<string> Messages { get; } = new();
    public ILogger CreateLogger(string categoryName) => new Capture(this, categoryName);
    public void Dispose() { }
    private sealed class Capture(LogoutLogCapture owner, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> formatter)
        { if (category == "Enver.API.Observability.AuthenticationEvents") owner.Messages.Enqueue(formatter(state, error)); }
    }
}
