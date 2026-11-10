using System.Globalization;
using System.Security.Cryptography;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

// Composición EXCLUSIVAMENTE de test: dos hosts lógicos, Redis real compartido y estado local independiente.
// No representa hosts físicos, load balancer, AWS, TLS ni ACL productivos.
internal sealed class RedisOperationalScope : IAsyncDisposable
{
    private const long QuotaBudgetBytesCited = 67_108_864;
    private readonly AuthenticationDependenciesFixture fixture;
    private readonly string environment;
    private readonly HashSet<RedisKey> owned = [];
    private readonly object sync = new();
    private ConnectionMultiplexer? admin;
    private IDatabase? database;
    private Task? disposal;

    internal OperationalHost A { get; }
    internal OperationalHost B { get; }
    internal string EnvironmentName => environment;
    internal string RevocationNamespace => $"enver:{environment}:auth:revoked";
    internal string RateLimitingNamespace => $"enver:{environment}:auth:ratelimit";

    private RedisOperationalScope(AuthenticationDependenciesFixture fixture)
    {
        this.fixture = fixture;
        environment = fixture.EnvironmentName.ToLowerInvariant() + "_op022_" + Guid.NewGuid().ToString("N")[..8];
        A = new OperationalHost(fixture, environment, fixture.RedisConnectionString);
        B = new OperationalHost(fixture, environment, fixture.RedisConnectionString);
    }

    internal static async Task<RedisOperationalScope> CreateAsync(AuthenticationDependenciesFixture fixture)
    {
        var scope = new RedisOperationalScope(fixture);
        try
        {
            await scope.InitializeAsync();
            return scope;
        }
        catch (Exception initializationFailure)
        {
            try
            {
                await scope.DisposeAsync();
            }
            catch (Exception cleanupFailure)
            {
                throw new AggregateException(
                    "Fallaron la inicialización y el cleanup del alcance operativo Redis.",
                    initializationFailure,
                    cleanupFailure);
            }

            throw;
        }
    }

    private async Task InitializeAsync()
    {
        var configuration = ConfigurationOptions.Parse(fixture.RedisConnectionString);
        configuration.AllowAdmin = true;
        configuration.AbortOnConnectFail = false;
        admin = await ConnectionMultiplexer.ConnectAsync(configuration);
        database = admin.GetDatabase();
        Assert.True((await database.PingAsync()) >= TimeSpan.Zero, "Redis de test no respondió al PING inicial.");

        _ = A.Services;
        _ = B.Services;
        Assert.NotSame(A.Services, B.Services);
        Assert.NotSame(A.Options, B.Options);
        Assert.NotSame(A.Connections, B.Connections);
        Assert.NotSame(A.Guard, B.Guard);
        Assert.NotSame(A.Store, B.Store);
        Assert.NotSame(await A.Connections.GetMultiplexerAsync(), await B.Connections.GetMultiplexerAsync());
        Assert.Equal(environment, A.Services.GetRequiredService<IHostEnvironment>().EnvironmentName);
        Assert.Equal(environment, B.Services.GetRequiredService<IHostEnvironment>().EnvironmentName);
    }

    internal async Task<TokenRevocationWriteResult> RevokeAsync(bool hostA, string jti, DateTimeOffset expiresAtUtc)
    {
        Track(jti);
        using var budget = Budget();
        var host = hostA ? A : B;
        return await host.Store.RevokeAsync(
            new TokenRevocationWriteRequest("enver-api", jti, expiresAtUtc, TimeSpan.Zero, TimeSpan.Zero),
            budget);
    }

    internal async Task<TokenRevocationReadResult> CheckAsync(bool hostA, string jti)
    {
        using var budget = Budget();
        var host = hostA ? A : B;
        return await host.Store.CheckAsync(new TokenRevocationReadRequest("enver-api", jti), budget);
    }

    internal async Task<AuthenticationSecurityDecision> EvaluateGuardAsync(bool hostA)
    {
        using var budget = Budget();
        return await (hostA ? A.Guard : B.Guard).EvaluateAsync(budget);
    }

    internal void CloseBothGuardsByInjectedSignal()
    {
        A.Guard.ObserveConnectivitySignal(RedisConnectionSignal.ConnectionFailed);
        B.Guard.ObserveConnectivitySignal(RedisConnectionSignal.ConnectionFailed);
    }

    internal async Task<bool> PingAsync()
    {
        EnsureDatabase();
        return (await database!.PingAsync()) >= TimeSpan.Zero;
    }

    internal async Task<bool> KeyExistsAsync(string jti)
    {
        EnsureDatabase();
        return await database!.KeyExistsAsync(RevocationKey(jti), CommandFlags.DemandMaster);
    }

    internal async Task<DateTime?> ExpireTimeAsync(string jti)
    {
        EnsureDatabase();
        return await database!.KeyExpireTimeAsync(RevocationKey(jti), CommandFlags.DemandMaster);
    }

    internal async Task<TimeSpan?> TimeToLiveAsync(string jti)
    {
        EnsureDatabase();
        return await database!.KeyTimeToLiveAsync(RevocationKey(jti), CommandFlags.DemandMaster);
    }

    internal async Task<string?> StringValueAsync(string jti)
    {
        EnsureDatabase();
        var value = await database!.StringGetAsync(RevocationKey(jti), CommandFlags.DemandMaster);
        return value.IsNull ? null : value.ToString();
    }

    internal async Task<RedisOperationalProfile> ReadProfileAsync()
    {
        EnsureDatabase();
        var policy = (RedisResult[])(await database!.ExecuteAsync("CONFIG", "GET", "maxmemory-policy"))!;
        var memory = (RedisResult[])(await database.ExecuteAsync("CONFIG", "GET", "maxmemory"))!;
        var info = ParseInfo((await database.ExecuteAsync("INFO", "MEMORY")).ToString());
        var maxMemory = long.Parse(memory[1].ToString(), CultureInfo.InvariantCulture);
        var used = long.Parse(info["used_memory"], CultureInfo.InvariantCulture);
        var peak = long.Parse(info["used_memory_peak"], CultureInfo.InvariantCulture);
        return new RedisOperationalProfile(
            policy[1].ToString(),
            info["maxmemory_policy"],
            maxMemory,
            used,
            peak,
            maxMemory - used);
    }

    internal string RevocationKey(string jti) => RedisKeyBuilder.BuildRevocationKey(environment, "enver-api", jti);

    internal string BuildDiagnosticReport(
        RedisOperationalProfile profile,
        bool revocationConfirmed,
        bool observedBySecondHost,
        bool ttlPreserved,
        bool closedStateDeniesBoth,
        bool redisPingWhileClosed) =>
        string.Join(
            " ",
            "OPERATIONAL_SCOPE",
            "HOSTS=2",
            "HOST_KIND=LOGICAL_TEST",
            "ENVIRONMENT_ISOLATED=true",
            "REVOCATION_NAMESPACE_SHARED=true",
            "PURPOSE_SEPARATED=true",
            $"NOEVICTION={Bool(profile.Policy == "noeviction")}",
            $"INFO_NOEVICTION={Bool(profile.InfoPolicy == "noeviction")}",
            $"MAXMEMORY_TOTAL_BYTES={profile.MaxMemoryBytes}",
            $"USED_MEMORY_BYTES={profile.UsedMemoryBytes}",
            $"USED_MEMORY_PEAK_BYTES={profile.UsedMemoryPeakBytes}",
            $"HEADROOM_BYTES={profile.HeadroomBytes}",
            $"QUOTA_BUDGET_BYTES_CITED={QuotaBudgetBytesCited}",
            "QUOTA_BUDGET_SCOPE=CUOTAS_ONLY",
            "MAXMEMORY_SCOPE=FIXTURE_TOTAL",
            $"REVOCATION_CONFIRMED={Bool(revocationConfirmed)}",
            $"REVOCATION_OBSERVED_BY_SECOND_HOST={Bool(observedBySecondHost)}",
            $"TTL_PRESERVED={Bool(ttlPreserved)}",
            $"CLOSED_STATE_DENIES_BOTH={Bool(closedStateDeniesBoth)}",
            $"REDIS_PING_WHILE_CLOSED={Bool(redisPingWhileClosed)}",
            "LOCAL_FALLBACK=false",
            "NEGATIVE_CACHE_PERMISSIVE=false",
            "CLOSURE_SIGNAL=INJECTED",
            "REACTION_ONLY=true",
            "DETECTOR_EVIDENCE=T-AUTH-019/T-AUTH-055",
            "PHYSICAL_HOSTS=false",
            "AWS=false",
            "LOAD_BALANCER=false",
            "TLS_PRODUCTIVE_CLAIM=false",
            "ACL_PRODUCTIVE_CLAIM=false");

    internal static string NewJti() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    public ValueTask DisposeAsync()
    {
        lock (sync)
        {
            return new ValueTask(disposal ??= DisposeCoreAsync());
        }
    }

    private async Task DisposeCoreAsync()
    {
        List<Exception> failures = [];
        try
        {
            RedisKey[] keys;
            lock (sync)
            {
                keys = owned.ToArray();
            }

            if (database is not null)
            {
                foreach (var batch in keys.Chunk(128))
                {
                    await database.KeyDeleteAsync(batch, CommandFlags.DemandMaster);
                }
            }
        }
        catch (Exception cleanupFailure)
        {
            failures.Add(cleanupFailure);
        }
        finally
        {
            try
            {
                await B.DisposeAsync();
            }
            catch (Exception disposalFailure)
            {
                failures.Add(disposalFailure);
            }

            try
            {
                await A.DisposeAsync();
            }
            catch (Exception disposalFailure)
            {
                failures.Add(disposalFailure);
            }

            try
            {
                if (admin is not null)
                {
                    await admin.DisposeAsync();
                }
            }
            catch (Exception disposalFailure)
            {
                failures.Add(disposalFailure);
            }

            lock (sync)
            {
                owned.Clear();
            }
        }

        if (failures.Count > 0)
        {
            throw new AggregateException("Falló el cleanup del alcance operativo Redis.", failures);
        }
    }

    private void Track(string jti)
    {
        lock (sync)
        {
            owned.Add(RevocationKey(jti));
        }
    }

    private void EnsureDatabase()
    {
        if (database is null)
        {
            throw new InvalidOperationException("Redis del alcance operativo no inicializado.");
        }
    }

    private static RequestBudget Budget() => new(5000, CancellationToken.None);

    private static string Bool(bool value) => value ? "true" : "false";

    private static Dictionary<string, string> ParseInfo(string text)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var raw in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            var separator = line.IndexOf(':');
            if (separator <= 0)
            {
                continue;
            }

            values[line[..separator]] = line[(separator + 1)..];
        }

        return values;
    }

    internal sealed record RedisOperationalProfile(
        string Policy,
        string InfoPolicy,
        long MaxMemoryBytes,
        long UsedMemoryBytes,
        long UsedMemoryPeakBytes,
        long HeadroomBytes);

    internal sealed class OperationalHost : WebApplicationFactory<AuthenticationOptions>
    {
        private readonly AuthenticationDependenciesFixture fixture;
        private readonly string environment;

        internal RedisOptions Options { get; }
        internal RedisConnections Connections => Services.GetRequiredService<RedisConnections>();
        internal RedisSecurityStateGuard Guard => Services.GetRequiredService<RedisSecurityStateGuard>();
        internal RedisTokenRevocationStore Store => Services.GetRequiredService<RedisTokenRevocationStore>();

        internal OperationalHost(AuthenticationDependenciesFixture fixture, string environment, string connectionString)
        {
            this.fixture = fixture;
            this.environment = environment;
            Options = new RedisOptions
            {
                ConnectionString = connectionString,
                NamespacePrefix = $"enver:{environment}",
                RevocationPurpose = "auth:revocation",
                RateLimitingPurpose = "auth:ratelimit",
                OperationTimeoutMilliseconds = 2000,
                ConnectionTimeoutMilliseconds = 2000,
                RevocationOperationalMarginSeconds = 30,
                FailClosed = true,
                RequiredRevocationEvictionPolicy = "noeviction"
            };
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                fixture.CreateConfiguration(new Dictionary<string, string?>
                {
                    ["Redis:ConnectionString"] = Options.ConnectionString,
                    ["Redis:NamespacePrefix"] = Options.NamespacePrefix,
                    ["MongoDb:DatabaseName"] = "enver_" + environment
                })));
            builder.ConfigureServices((context, services) =>
            {
                services.AddValidatedAuthenticationOptions(context.Configuration, context.HostingEnvironment);
                services.AddSingleton(_ => new RedisConnections(Options));
                services.AddSingleton(p => new RedisSecurityStateGuard(p.GetRequiredService<RedisConnections>(), Options));
                services.AddSingleton(p => new RedisTokenRevocationStore(
                    p.GetRequiredService<RedisConnections>(),
                    p.GetRequiredService<RedisSecurityStateGuard>(),
                    Options));
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment(environment);
            return base.CreateHost(builder);
        }
    }
}
