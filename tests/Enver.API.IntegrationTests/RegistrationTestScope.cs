using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using Enver.API.Composition;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using Enver.API.Infrastructure.MongoDB;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using StackExchange.Redis;
using Xunit.Sdk;

namespace Enver.API.IntegrationTests;

/// <summary>Recursos propios, autoridad local explícita y socket sintético. No modifica fixture ni producto.</summary>
internal sealed class RegistrationTestScope : IAsyncDisposable
{
    private readonly AuthenticationDependenciesFixture fixture;
    private readonly OwnedCompositionAuthority authority;
    private readonly bool probeRegistrationStore;
    private readonly string databaseName;
    private readonly string environment = "reg_" + Guid.NewGuid().ToString("N");
    private readonly List<AuthenticationApiFactory> owners = [];
    private readonly List<WebApplicationFactory<AuthenticationOptions>> hosts = [];
    private readonly List<HttpClient> clients = [];
    internal Capture Logs { get; } = new();
    internal IServiceProvider Services => hosts[0].Services;
    internal IMongoDatabase Database => Services.GetRequiredService<IMongoClient>().GetDatabase(databaseName);
    internal IMongoCollection<BsonDocument> Documents => Database.GetCollection<BsonDocument>("users");
    internal HttpClient Client => clients[0];
    internal QuotaProbe Quotas => Services.GetRequiredService<QuotaProbe>();
    internal StoreProbe Store => Services.GetRequiredService<StoreProbe>();

    private RegistrationTestScope(AuthenticationDependenciesFixture fixture, OwnedCompositionAuthority authority, bool probeRegistrationStore)
    {
        this.fixture = fixture; this.authority = authority;
        this.probeRegistrationStore = probeRegistrationStore;
        databaseName = fixture.MongoDatabase + "_reg_" + Guid.NewGuid().ToString("N")[..8];
    }

    internal static async Task<RegistrationTestScope> CreateAsync(AuthenticationDependenciesFixture fixture, bool initializeQuota = true,
        bool openAuthority = true, IReadOnlyDictionary<string, string?>? overrides = null, bool probeRegistrationStore = true)
    {
        var authority = await OwnedCompositionAuthority.CreateAsync(fixture, openAuthority);
        var scope = new RegistrationTestScope(fixture, authority, probeRegistrationStore);
        try
        {
            scope.AddHost(overrides);
            // Abrir conexiones/setup explícitos fuera del presupuesto de los requests; no reintentar la operación.
            using var setup = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await scope.Database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: setup.Token);
            if (initializeQuota)
            {
                var p = scope.Services;
                // Preparación test-only: conectar y confirmar PING antes del presupuesto del lifecycle.
                // No consume cuota, ejecuta EVAL ni modifica buckets o la autoridad.
                var redis = await p.GetRequiredService<RedisConnections>().GetMultiplexerAsync(setup.Token);
                await redis.GetDatabase().PingAsync(CommandFlags.DemandMaster).WaitAsync(setup.Token);
                var lifecycle = new RedisAuthenticationQuotaLifecycle(p.GetRequiredService<RedisConnections>(), p.GetRequiredService<IOptions<RedisOptions>>().Value,
                    p.GetRequiredService<RedisAuthenticationQuotaConfiguration>(), p.GetRequiredService<RedisSecurityStateGuard>(), p.GetRequiredService<AuthenticationDependencyAdmission>());
                using var budget = new RequestBudget(8000, default);
                Assert.Equal("initialized", (await lifecycle.InitializeVerifiedNewEnvironmentAsync(budget)).Status);
            }
            return scope;
        }
        catch { await scope.DisposeAsync(); throw; }
    }

    internal HttpClient AddHost(IReadOnlyDictionary<string, string?>? overrides = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["MongoDb:DatabaseName"] = databaseName,
            ["AuthenticationSecurityAuthority:TableName"] = authority.TableName,
            ["Authentication:RegisterBudgetMilliseconds"] = "8000",
            ["Authentication:HashingConcurrencyLimit"] = "2"
        };
        if (overrides is not null) foreach (var pair in overrides) values[pair.Key] = pair.Value;
        var owner = new AuthenticationApiFactory(fixture, values); owners.Add(owner);
        var host = owner.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IStartupFilter, SyntheticPeer>();
            services.AddSingleton<ILoggerProvider>(Logs);
            // Mismos valores de política, generación compartida y clientes independientes, namespace exclusivo del caso.
            services.AddSingleton(p => p.GetRequiredService<IOptions<AuthenticationDependencyOptions>>().Value.Capture(environment, p.GetRequiredService<IOptions<RateLimitingOptions>>().Value));
            var quota = services.Last(d => d.ServiceType == typeof(IAuthenticationQuotaStore)); services.Remove(quota);
            services.AddSingleton(p => new QuotaProbe((IAuthenticationQuotaStore)quota.ImplementationFactory!(p)));
            services.AddSingleton<IAuthenticationQuotaStore>(p => p.GetRequiredService<QuotaProbe>());
            if (probeRegistrationStore)
            {
                var store = services.Last(d => d.ServiceType == typeof(IUserStore)); services.Remove(store);
                services.AddSingleton(p => new StoreProbe((IUserStore)store.ImplementationFactory!(p)));
                services.AddSingleton<IUserStore>(p => p.GetRequiredService<StoreProbe>());
            }
        }));
        hosts.Add(host);
        var client = host.CreateClient(); clients.Add(client);
        return client;
    }

    internal static StringContent Body(string email = "synthetic@uttt.edu.mx", string name = "  Ána  María  ", string password = "synthetic-secret") =>
        new(JsonSerializer.Serialize(new { fullName = name, email, password }), Encoding.UTF8, "application/json");
    internal Task<HttpResponseMessage> SendAsync(string email = "synthetic@uttt.edu.mx", string name = "  Ána  María  ", string password = "synthetic-secret", HttpClient? client = null) =>
        (client ?? Client).PostAsync("/api/v1/auth/register", Body(email, name, password));

    internal async Task<long> PartitionCountAsync()
    {
        var db = (await Services.GetRequiredService<RedisConnections>().GetMultiplexerAsync()).GetDatabase();
        return (long?)await db.HashGetAsync(Services.GetRequiredService<RedisAuthenticationQuotaConfiguration>().Keys.Metadata, "count") ?? 0;
    }
    internal async Task AssertProblemAsync(HttpResponseMessage response, int status, string code)
    {
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        Assert.Equal("about:blank", root.GetProperty("type").GetString());
        Assert.Equal(status, root.GetProperty("status").GetInt32());
        Assert.Equal(code, root.GetProperty("code").GetString());
        Assert.True(root.TryGetProperty("title", out _));
        Assert.True(root.TryGetProperty("detail", out _));
        Assert.Equal(response.Headers.GetValues("X-Correlation-ID").Single(), root.GetProperty("traceId").GetString());
        Assert.True(root.EnumerateObject().All(p => new[] { "type", "title", "status", "detail", "code", "traceId", "errors" }.Contains(p.Name)));
        Assert.False(fixture.ContainsSyntheticSecret(root.GetRawText()));
    }
    internal static async Task SafelyAsync(Func<Task> action)
    {
        try { await action(); }
        catch (Exception error)
        {
            throw new XunitException($"Register: evidencia no acreditada; tipo={error.GetType().Name}; detalles sensibles suprimidos.");
        }
    }
    public ValueTask DisposeAsync() => DisposeWithRedisCleanupAsync(null);

    // Opt-in solo para escenarios que reinician el contenedor y cambian su endpoint publicado.
    // El llamador posee esta conexión; no sustituye el cliente/estado de seguridad de la aplicación.
    internal async ValueTask DisposeWithRedisCleanupAsync(RedisConnections? cleanupConnections)
    {
        try
        {
            if (hosts.Count > 0)
            {
                var p = Services;
                var connection = await (cleanupConnections ?? p.GetRequiredService<RedisConnections>()).GetMultiplexerAsync();
                var config = p.GetRequiredService<RedisAuthenticationQuotaConfiguration>();
                var keys = new List<RedisKey>();
                await foreach (var key in connection.GetServer(connection.GetEndPoints()[0]).KeysAsync(pattern: config.Keys.Prefix + "*")) keys.Add(key);
                if (keys.Count > 0) await connection.GetDatabase().KeyDeleteAsync(keys.ToArray());
                await Database.Client.DropDatabaseAsync(databaseName);
            }
        }
        finally
        {
            foreach (var client in clients) client.Dispose();
            foreach (var host in hosts.AsEnumerable().Reverse()) await host.DisposeAsync();
            foreach (var owner in owners.AsEnumerable().Reverse()) await owner.DisposeAsync();
            await authority.DisposeAsync();
        }
    }
    private sealed class SyntheticPeer : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, proceed) => { context.Connection.RemoteIpAddress = IPAddress.Loopback; await proceed(context); }); next(app);
        };
    }
    internal sealed class Capture : ILoggerProvider
    {
        internal ConcurrentQueue<string> Messages { get; } = new();
        internal ConcurrentQueue<string[]> EventKeys { get; } = new();
        public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);
        public void Dispose() { }
        private sealed class Logger(Capture capture, string category) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel level) => true;
            public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> formatter)
            {
                capture.Messages.Enqueue(formatter(state, error) + error?.ToString());
                if (category == "Enver.API.Observability.AuthenticationEvents" && state is IEnumerable<KeyValuePair<string, object?>> fields)
                    capture.EventKeys.Enqueue(fields.Select(f => f.Key).Order().ToArray());
            }
        }
    }
    internal sealed class QuotaProbe(IAuthenticationQuotaStore inner) : IAuthenticationQuotaStore
    {
        public int Calls;
        internal AuthenticationQuotaStoreStatus? Outcome;
        internal AuthenticationQuotaConsumeRequest? Last;
        public async ValueTask<AuthenticationQuotaConsumeResult> EvaluateAndConsumeAsync(AuthenticationQuotaConsumeRequest request, RequestBudget budget)
        {
            Interlocked.Increment(ref Calls); Last = request;
            if (Outcome == AuthenticationQuotaStoreStatus.Unavailable) return new(AuthenticationQuotaStoreStatus.Unavailable);
            var result = await inner.EvaluateAndConsumeAsync(request, budget);
            return Outcome == AuthenticationQuotaStoreStatus.Uncertain && result.Status == AuthenticationQuotaStoreStatus.Admitted
                ? new(AuthenticationQuotaStoreStatus.Uncertain) : result;
        }
    }
    internal sealed class StoreProbe(IUserStore inner) : IUserStore
    {
        public int Calls;
        internal UserInsertStatus? Outcome;
        public async ValueTask<UserInsertResult> InsertAsync(User user, RequestBudget budget)
        {
            Interlocked.Increment(ref Calls);
            if (Outcome == UserInsertStatus.Unavailable) return new(UserInsertStatus.Unavailable);
            var result = await inner.InsertAsync(user, budget);
            return Outcome == UserInsertStatus.Uncertain && result.Status == UserInsertStatus.Confirmed ? new(UserInsertStatus.Uncertain) : result;
        }
        public ValueTask<UserReadResult> FindByNormalizedEmailAsync(string email, RequestBudget budget) => throw new InvalidOperationException("Read-before-write forbidden.");
        public ValueTask<UserReadResult> FindByIdAsync(Guid id, RequestBudget budget) => throw new InvalidOperationException("Read-before-write forbidden.");
        public ValueTask<UserCredentialUpdateResult> CompareExchangePasswordHashAsync(Guid id, string expected, UserStatus status, string hash, DateTime at, RequestBudget budget) => throw new InvalidOperationException("CAS forbidden.");
    }

}
