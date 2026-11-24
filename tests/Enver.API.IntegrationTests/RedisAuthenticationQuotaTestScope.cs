using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

// Preparación sintética explícita. No constituye coordinación productiva ni evidencia de forwarding/JWT.
internal sealed class RedisAuthenticationQuotaTestScope : IAsyncDisposable
{
    private readonly HashSet<RedisKey> owned = [];
    private readonly object sync = new();
    // Único owner de cada RedisConnections y, transitivamente, de su multiplexer.
    private readonly List<RedisConnections> ownedConnections = [];
    private Task? disposal;
    internal static readonly OpenState Open = new();
    internal RedisOptions Options { get; }
    internal RedisAuthenticationQuotaConfiguration Configuration { get; }
    internal RedisConnections Connections { get; }
    internal RedisConnections SecondConnections { get; }
    internal IDatabase Database { get; private set; } = null!;
    internal RedisAuthenticationQuotaStore Store { get; }
    internal RedisAuthenticationQuotaStore SecondStore { get; }
    internal RedisAuthenticationQuotaLifecycle Lifecycle { get; }
    internal Action<long>? RedisTimeObserved { get; set; }

    private RedisAuthenticationQuotaTestScope(AuthenticationDependenciesFixture fixture, TimeSpan? window)
    {
        var environment = fixture.EnvironmentName.ToLowerInvariant() + "_" + Guid.NewGuid().ToString("N")[..8];
        var policy = window is null ? AuthenticationQuotaPolicyDescriptor.AcademicInitial : new AuthenticationQuotaPolicyDescriptor("synthetic-window",
            AuthenticationQuotaPolicyDescriptor.AcademicInitial.Rules.Select(r => new AuthenticationQuotaPolicyRule(r.Operation, r.ScopeType, new(r.Definition.PermitLimit, window.Value))));
        Configuration = new(new(environment, "config-021", "identity-053", policy, AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(5000)), Guid.NewGuid().ToString("N"));
        // Permiso de cliente solo de IT para CONFIG GET y CLIENT PAUSE en este contenedor de pruebas.
        Options = new() { ConnectionString = fixture.RedisConnectionString + ",allowAdmin=true", NamespacePrefix = "enver:" + environment,
            OperationTimeoutMilliseconds = 2000, ConnectionTimeoutMilliseconds = 2000 };
        Connections = new(Options);
        SecondConnections = new(Options);
        ownedConnections.Add(Connections);
        ownedConnections.Add(SecondConnections);
        Store = NewStore(Connections, Configuration);
        SecondStore = NewStore(SecondConnections, Configuration);
        Lifecycle = NewLifecycle(Configuration);
        Track(Configuration.Keys.Metadata);
        Track(Configuration.Keys.Registry);
    }

    internal static async Task<RedisAuthenticationQuotaTestScope> CreateAsync(AuthenticationDependenciesFixture fixture, TimeSpan? window = null)
    {
        var scope = new RedisAuthenticationQuotaTestScope(fixture, window);
        try
        {
            scope.Database = (await scope.Connections.GetMultiplexerAsync()).GetDatabase();
            using var budget = Budget();
            // Ambiente aleatorio exclusivo creado por esta prueba, sin historia previa.
            Assert.Equal("initialized", (await scope.Lifecycle.InitializeVerifiedNewEnvironmentAsync(budget)).Status);
            return scope;
        }
        catch (Exception initializationFailure)
        {
            try { await scope.DisposeAsync(); }
            catch (Exception cleanupFailure)
            {
                throw new AggregateException("Fallaron la inicialización y el cleanup del scope Redis.", initializationFailure, cleanupFailure);
            }
            throw;
        }
    }

    internal static (string Host, int Port) Endpoint(string connectionString)
    {
        var endpoints = ConfigurationOptions.Parse(connectionString).EndPoints;
        if (endpoints.Count != 1) throw new InvalidOperationException("El harness requiere un único endpoint Redis.");
        return endpoints[0] switch
        {
            DnsEndPoint dns => (dns.Host, dns.Port),
            IPEndPoint ip => (ip.Address.ToString(), ip.Port),
            _ => throw new InvalidOperationException("Tipo de endpoint Redis de test no reconocido.")
        };
    }

    internal async Task<RedisConnections> CreateConnectionAfterControlledRestartAsync(string currentConnectionString, Action<string> diagnostic)
    {
        // No reutilizar el endpoint capturado antes de Stop/Start. Solo cambian opciones del harness.
        var currentOptions = new RedisOptions
        {
            ConnectionString = currentConnectionString + ",allowAdmin=true",
            NamespacePrefix = Options.NamespacePrefix,
            OperationTimeoutMilliseconds = Options.OperationTimeoutMilliseconds,
            ConnectionTimeoutMilliseconds = Options.ConnectionTimeoutMilliseconds
        };
        var expectedEndpoint = Endpoint(currentOptions.ConnectionString);
        RedisConnections connection;
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposal is not null, this);
            connection = new(currentOptions);
            // Registrar ownership ANTES de inicializar: también se libera si la conexión falla.
            ownedConnections.Add(connection);
        }
        using var budget = Budget();
        var multiplexer = await connection.GetMultiplexerAsync(budget.Token);
        var configured = multiplexer.GetEndPoints(configuredOnly: true);
        Assert.Single(configured);
        var actualEndpoint = configured[0] switch
        {
            DnsEndPoint dns => (dns.Host, dns.Port),
            IPEndPoint ip => (ip.Address.ToString(), ip.Port),
            _ => throw new InvalidOperationException("Endpoint del cliente Redis de test no reconocido.")
        };
        Assert.Equal(expectedEndpoint, actualEndpoint);
        diagnostic($"NEW_CLIENT_HOST={actualEndpoint.Item1} NEW_CLIENT_PORT={actualEndpoint.Item2} CURRENT_ENDPOINT_MATCH=true");
        var database = multiplexer.GetDatabase();
        // Sondeos inocuos y acotados de infraestructura; no reabren guard ni autoridad productiva.
        while (true)
        {
            budget.ThrowIfCancellationRequested();
            try
            {
                await database.PingAsync(CommandFlags.DemandMaster).WaitAsync(budget.Token);
                Assert.True(multiplexer.IsConnected, "PING completado sin cliente conectado.");
                diagnostic("NEW_MULTIPLEXER_CONNECTED=true TEST_PING=OK TEST_INFRASTRUCTURE_READY=true");
                break;
            }
            catch (Exception failure) when (failure is RedisConnectionException or RedisTimeoutException)
            {
                if (budget.Remaining <= TimeSpan.FromMilliseconds(50))
                    throw new InvalidOperationException("Readiness Redis del harness no acreditada dentro del plazo.", failure);
                await Task.Delay(TimeSpan.FromMilliseconds(50), budget.Token);
            }
        }
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposal is not null, this);
            Database = database;
        }
        // Referencia prestada; el llamador no debe disponerla.
        return connection;
    }

    internal RedisAuthenticationQuotaStore NewStore(RedisConnections connections, RedisAuthenticationQuotaConfiguration config,
        IAuthenticationSecurityState? local = null, IAuthenticationSecurityState? global = null) =>
        new(connections, Options, config, local ?? Open, global ?? Open);
    internal RedisAuthenticationQuotaLifecycle NewLifecycle(RedisAuthenticationQuotaConfiguration config) => new(Connections, Options, config, Open, Open);
    internal static RequestBudget Budget() => new(10000, CancellationToken.None);
    internal static string Opaque(int seed) => Convert.ToBase64String(SHA256.HashData(BitConverter.GetBytes(seed))).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    internal void Track(RedisKey key) { lock (sync) owned.Add(key); }

    internal AuthenticationQuotaConsumeRequest Request(int seed = 1, AuthenticationOperation operation = AuthenticationOperation.Register,
        string? attempt = null, string? revision = null, IReadOnlyDictionary<AuthenticationQuotaScopeType, int>? identities = null)
    {
        var request = new AuthenticationQuotaConsumeRequest(operation, attempt ?? Guid.NewGuid().ToString("N"), revision ?? Configuration.Snapshot.Policy.Revision,
            AuthenticationQuotaPolicyDescriptor.RequiredScopes(operation).Select(type => new AuthenticationQuotaScope(type,
                Opaque(identities is not null && identities.TryGetValue(type, out var value) ? value : seed * 10 + (int)type),
                Configuration.Snapshot.Policy.GetDefinition(operation, type))));
        foreach (var scope in request.Scopes) Track(Key(request, scope));
        return request;
    }
    internal string Key(AuthenticationQuotaConsumeRequest request, AuthenticationQuotaScope scope) => Configuration.Keys.Bucket(request.Operation, scope.ScopeType, scope.OpaqueId);
    internal string Member(AuthenticationQuotaConsumeRequest request, AuthenticationQuotaScope scope) => RedisAuthenticationQuotaKeyBuilder.Member(request.Operation, scope.ScopeType, scope.OpaqueId);
    internal async Task<AuthenticationQuotaConsumeResult> SendAsync(AuthenticationQuotaConsumeRequest request, RedisAuthenticationQuotaStore? store = null)
    {
        using var budget = Budget();
        return await (store ?? Store).EvaluateAndConsumeAsync(request, budget);
    }
    internal async Task<RedisAuthenticationQuotaReply> RawAsync(AuthenticationQuotaConsumeRequest request, Action<RedisAuthenticationQuotaInvocation>? change = null)
    {
        var invocation = RedisAuthenticationQuotaScriptProtocol.Encode(Configuration, "evaluate", request);
        change?.Invoke(invocation);
        return RedisAuthenticationQuotaScriptProtocol.Parse(await Database.ScriptEvaluateAsync(RedisAuthenticationQuotaScriptProtocol.ScriptText,
            invocation.Keys, invocation.Arguments, CommandFlags.DemandMaster));
    }
    internal async Task<long> NowAsync()
    {
        var result = (RedisResult[])(await Database.ExecuteAsync("TIME"))!;
        var now = checked((long)result[0] * 1000000 + (long)result[1]);
        RedisTimeObserved?.Invoke(now);
        return now;
    }
    internal async Task WaitUntilAsync(long target, TimeSpan? maximumWait = null)
    {
        // Stopwatch limita la espera del harness; únicamente Redis TIME decide si terminó la ventana.
        var timeout = maximumWait ?? TimeSpan.FromSeconds(10);
        var timer = Stopwatch.StartNew();
        while (await NowAsync() < target)
        {
            Assert.True(timer.Elapsed < timeout, "No se alcanzó el horizonte Redis dentro del límite de prueba.");
            await Task.Delay(5);
        }
    }
    internal async Task<long> CountAsync() => (long)await Database.HashGetAsync(Configuration.Keys.Metadata, "count");

    internal async Task SeedAsync(AuthenticationQuotaConsumeRequest request, AuthenticationQuotaScope scope, IEnumerable<long> times)
    {
        var entries = times.Select(t => new SortedSetEntry(Guid.NewGuid().ToString("N"), t)).ToArray();
        var horizon = checked((long)entries.Max(e => e.Score) + RedisAuthenticationQuotaConfiguration.WindowMicroseconds(scope.Definition.Window));
        await SeedBucketAsync(Key(request, scope), Member(request, scope), entries, horizon);
        await SynchronizeMetadataAsync();
    }
    internal async Task SeedBucketAsync(string key, string member, SortedSetEntry[] entries, long horizon)
    {
        Track(key);
        await Database.SortedSetAddAsync(key, entries, CommandFlags.DemandMaster);
        await Database.ExecuteAsync("PEXPIREAT", key, (horizon + 999) / 1000);
        await Database.SortedSetAddAsync(Configuration.Keys.Registry, member, horizon, CommandFlags.DemandMaster);
    }
    internal async Task SynchronizeMetadataAsync()
    {
        var count = await Database.SortedSetLengthAsync(Configuration.Keys.Registry);
        var top = await Database.SortedSetRangeByRankWithScoresAsync(Configuration.Keys.Registry, -1, -1);
        var horizon = top.Length == 0 ? 0 : (long)top[0].Score;
        await Database.HashSetAsync(Configuration.Keys.Metadata, [new HashEntry("count", count), new HashEntry("horizon", horizon)]);
        if (count > 0) await Database.ExecuteAsync("PEXPIREAT", Configuration.Keys.Registry, (horizon + 999) / 1000);
    }
    internal async Task FillRegisterAsync(int requests)
    {
        // Secuencial deliberadamente: cada éxito ejecuta el adapter/Lua productivo aislable, no un seed.
        for (var i = 1; i <= requests; i++) Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await SendAsync(Request(i))).Status);
    }
    public ValueTask DisposeAsync()
    {
        // Una sola limpieza/disposición, incluso si se solicita nuevamente tras un fallo.
        lock (sync) return new(disposal ??= DisposeCoreAsync());
    }

    private async Task DisposeCoreAsync()
    {
        List<Exception> failures = [];
        try
        {
            if (Database is not null)
            {
                RedisKey[] keys;
                lock (sync) keys = owned.ToArray();
                foreach (var batch in keys.Chunk(128)) await Database.KeyDeleteAsync(batch, CommandFlags.DemandMaster);
            }
        }
        catch (Exception cleanupFailure) { failures.Add(cleanupFailure); }
        finally
        {
            Database = null!;
            // El cleanup anterior termina mientras su conexión sigue viva. No disponer IDatabase por separado.
            foreach (var connection in ownedConnections.AsEnumerable().Reverse())
            {
                try { await connection.DisposeAsync(); }
                catch (Exception disposalFailure) { failures.Add(disposalFailure); }
            }
            ownedConnections.Clear();
            lock (sync) owned.Clear();
        }
        if (failures.Count > 0) throw new AggregateException("Falló el cleanup del scope Redis.", failures);
    }
    internal sealed class OpenState : IAuthenticationSecurityState
    {
        public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget requestBudget) =>
            ValueTask.FromResult(new AuthenticationSecurityDecision(AuthenticationSecurityAdmission.Verifiable));
    }
}
