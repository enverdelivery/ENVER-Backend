using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.API.Infrastructure.Security;
using Enver.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

// Composición EXCLUSIVAMENTE de test: no endpoints, hosts físicos, forwarding o JWT productivos.
internal sealed class DistributedQuotaTestScope
{
    internal RedisAuthenticationQuotaTestScope Data { get; }
    internal RedisAuthenticationQuotaConfiguration Config => Data.Configuration;
    internal IDatabase Db => Data.Database;
    internal QuotaHost A { get; private set; } = null!;
    internal QuotaHost B { get; private set; } = null!;
    internal DistributedQuotaDiagnostics? Diagnostics { get; private set; }
    private readonly AuthenticationDependenciesFixture fixture;
    private readonly ITestOutputHelper output;
    private readonly string hmac = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    private readonly ConcurrentDictionary<string, AuthenticationQuotaScope> buckets = new();
    private readonly ConcurrentDictionary<(string Phase, int Host, AuthenticationOperation Operation, AuthenticationQuotaStoreStatus Status), int> outcomes = new();
    private readonly HashSet<string> extraKeys = [];
    private readonly List<IAsyncDisposable> extraOwners = [];
    internal long PeakBytes { get; private set; }

    private DistributedQuotaTestScope(AuthenticationDependenciesFixture fixture, ITestOutputHelper output, RedisAuthenticationQuotaTestScope data)
    { this.fixture = fixture; this.output = output; Data = data; }

    // Presupuesto de harness: acota el cuerpo de la prueba; no es SLA, RequestBudget ni timeout funcional de Redis.
    internal static async Task RunAsync(AuthenticationDependenciesFixture fixture, ITestOutputHelper output,
        Func<DistributedQuotaTestScope, Task> test, TimeSpan? syntheticWindow = null, TimeSpan? budget = null, bool diagnostics = false)
    {
        var data = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture, syntheticWindow);
        var scope = new DistributedQuotaTestScope(fixture, output, data);
        if (diagnostics) scope.Diagnostics = new(scope.Config.Keys.Metadata);
        Exception? primary = null;
        try
        {
            scope.A = scope.Host(); scope.B = scope.Host();
            Assert.NotSame(scope.A.Services, scope.B.Services);
            Assert.NotSame(scope.A.Server, scope.B.Server);
            Assert.NotSame(scope.A.Connections, scope.B.Connections);
            Assert.NotSame(await scope.A.Connections.GetMultiplexerAsync(), await scope.B.Connections.GetMultiplexerAsync());
            Assert.NotSame(scope.A.Guard, scope.B.Guard);
            Assert.NotSame(scope.A.Detector, scope.B.Detector);
            Assert.NotSame(scope.A.Pseudonymizer, scope.B.Pseudonymizer);
            if (scope.Diagnostics is { } trace)
            {
                scope.A.Detector.SignalObserved += (_, signal) => trace.Connectivity("A", signal.Signal);
                scope.B.Detector.SignalObserved += (_, signal) => trace.Connectivity("B", signal.Signal);
            }
            // Un TimeoutException del presupuesto debe fallar la prueba; nunca se captura para simular éxito.
            using var observer = scope.Diagnostics?.Observe();
            await test(scope).WaitAsync(budget ?? TimeSpan.FromMinutes(5));
            scope.Diagnostics?.Report(output);
        }
        catch (Exception failure)
        {
            primary = failure;
            if (scope.Diagnostics is null) throw;
            try { primary = await scope.Diagnostics.FailureAsync(scope, failure); }
            catch (Exception diagnosticFailure)
            {
                primary = new AggregateException("Falló el diagnóstico; se conserva el fallo original.", failure, diagnosticFailure);
            }
            throw primary;
        }
        finally
        {
            List<Exception> cleanup = [];
            try { await scope.CleanupQuotaAsync(); foreach (var key in scope.extraKeys) await scope.Db.KeyDeleteAsync(key); }
            catch (Exception failure) { cleanup.Add(failure); }
            foreach (var owner in scope.extraOwners.AsEnumerable().Reverse())
            {
                try { await owner.DisposeAsync(); } catch (Exception failure) { cleanup.Add(failure); }
            }
            try { await data.DisposeAsync(); } catch (Exception failure) { cleanup.Add(failure); }
            if (scope.B is not null) try { await scope.B.DisposeAsync(); } catch (Exception failure) { cleanup.Add(failure); }
            if (scope.A is not null) try { await scope.A.DisposeAsync(); } catch (Exception failure) { cleanup.Add(failure); }
            scope.Report();
            if (cleanup.Count > 0) throw new AggregateException("Fallaron cleanup/owners de 055.", primary is null ? cleanup : new[] { primary }.Concat(cleanup));
        }
    }

    internal QuotaHost Host(string? connection = null, RedisAuthenticationQuotaConfiguration? config = null,
        IAuthenticationSecurityState? global = null) => new(fixture, config ?? Config, hmac, connection ?? fixture.RedisConnectionString, global, Diagnostics);

    /// <summary>Host/owner adicional del caso, liberado antes de A/B y del scope de datos.</summary>
    internal T Own<T>(T owner) where T : IAsyncDisposable { extraOwners.Add(owner); return owner; }

    internal RedisOptions RestartOptions() => new()
    {
        ConnectionString = fixture.RedisConnectionString + ",allowAdmin=true",
        NamespacePrefix = "enver:" + Config.Snapshot.Environment,
        OperationTimeoutMilliseconds = 2000, ConnectionTimeoutMilliseconds = 2000
    };

    internal async Task RestartHostAAsync()
    {
        await A.DisposeAsync(); A = Host();
        _ = A.Services; await (await A.Connections.GetMultiplexerAsync()).GetDatabase().PingAsync();
    }

    internal AuthenticationQuotaConsumeRequest Request(int host, AuthenticationOperation operation = AuthenticationOperation.Register,
        int email = 1, int origin = 1, int account = 1, string? revision = null, string? ip = null)
    {
        var owner = host == 0 ? A : B;
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip ?? $"198.18.{origin / 250 % 250}.{origin % 250 + 1}");
        context.Features.Set(AuthenticationOriginTrustContext.ApproveDirectConnection());
        var canonicalOrigin = new AuthenticationOriginProvider().Resolve(context).Origin!;
        var canonicalEmail = AuthenticationCanonicalEmail.FromValidatedEmail($"synthetic{email}@uttt.edu.mx");
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", new Guid(account, 0, 0, new byte[8]).ToString("D"))], "test-accredited"));
        var identities = AuthenticationQuotaPolicyDescriptor.RequiredScopes(operation).Select(type => type switch
        {
            AuthenticationQuotaScopeType.Email => AuthenticationScopeIdentity.Email(operation, canonicalEmail),
            AuthenticationQuotaScopeType.Origin => AuthenticationScopeIdentity.Origin(operation, canonicalOrigin),
            AuthenticationQuotaScopeType.OriginEmail => AuthenticationScopeIdentity.OriginEmail(canonicalOrigin, canonicalEmail),
            _ => AuthenticationScopeIdentity.Account(AuthenticationAuthenticatedAccount.FromAuthenticatedPrincipal(principal))
        });
        var request = new AuthenticationQuotaConsumeRequest(operation, Guid.NewGuid().ToString("N"), revision ?? Config.Snapshot.Policy.Revision,
            identities.Select(id => new AuthenticationQuotaScope(id.ScopeType, owner.Pseudonymizer.Pseudonymize(id), Config.Snapshot.Policy.GetDefinition(operation, id.ScopeType))));
        Track(request); return request;
    }

    internal void Track(AuthenticationQuotaConsumeRequest request)
    {
        foreach (var scope in request.Scopes)
        { var key = Data.Key(request, scope); buckets.TryAdd(key, scope); Data.Track(key); }
    }
    internal void TrackExtra(string key) { extraKeys.Add(key); Data.Track(key); }

    internal async Task<AuthenticationQuotaConsumeResult> SendAsync(int host, AuthenticationQuotaConsumeRequest request, string phase = "matrix",
        RedisAuthenticationQuotaStore? store = null)
    {
        Diagnostics?.Request(request);
        using var budget = RedisAuthenticationQuotaTestScope.Budget();
        var result = await (store ?? (host == 0 ? A : B).Store).EvaluateAndConsumeAsync(request, budget);
        Diagnostics?.Decision(result);
        outcomes.AddOrUpdate((phase, host, request.Operation, result.Status), 1, (_, value) => value + 1);
        return result;
    }

    internal async Task AssertAttemptAsync(AuthenticationQuotaConsumeRequest request, bool admitted)
    {
        double? score = null;
        foreach (var scope in request.Scopes)
        {
            var value = await Db.SortedSetScoreAsync(Data.Key(request, scope), request.InternalAttemptId);
            if (!admitted) Assert.Null(value);
            else { Assert.NotNull(value); score ??= value; Assert.Equal(score, value); }
        }
    }

    internal async Task<Measurement> MeasureAsync(string phase)
    {
        var now = await Data.NowAsync(); long bytes = 0, events = 0, active = 0;
        // Barrido de diagnóstico no atómico bajo mutación; no es un máximo continuo.
        foreach (var batch in buckets.ToArray().Chunk(64))
        {
            var rows = await Task.WhenAll(batch.Select(async pair =>
            {
                var size = await Db.ExecuteAsync("MEMORY", "USAGE", pair.Key, "SAMPLES", "0");
                var count = await Db.SortedSetLengthAsync(pair.Key);
                var live = await Db.SortedSetLengthAsync(pair.Key, now - RedisAuthenticationQuotaConfiguration.WindowMicroseconds(pair.Value.Definition.Window) + 1, now);
                return (Bytes: size.IsNull ? 0 : (long)size, Events: count, Active: live > 0 ? 1 : 0);
            }));
            bytes += rows.Sum(r => r.Bytes); events += rows.Sum(r => r.Events); active += rows.Sum(r => r.Active);
        }
        var registry = await Db.ExecuteAsync("MEMORY", "USAGE", Config.Keys.Registry, "SAMPLES", "0");
        var metadata = await Db.ExecuteAsync("MEMORY", "USAGE", Config.Keys.Metadata, "SAMPLES", "0");
        var registryBytes = registry.IsNull ? 0 : (long)registry;
        var metadataBytes = metadata.IsNull ? 0 : (long)metadata;
        var total = bytes + registryBytes + metadataBytes;
        PeakBytes = Math.Max(PeakBytes, total);
        var countValue = await Db.HashGetAsync(Config.Keys.Metadata, "count");
        var accounted = countValue.IsNull ? 0 : (long)countValue;
        var info = (await Db.ExecuteAsync("INFO", "MEMORY")).ToString();
        output.WriteLine($"{phase} BUCKET_BYTES={bytes} REGISTRY_BYTES={registryBytes} METADATA_BYTES={metadataBytes} ATTRIBUTED={total} OBSERVED_MAX={PeakBytes} BUDGET=67108864 EVENTS={events} ACTIVE={active} ACCOUNTED={accounted} RESIDUES={Math.Max(0, accounted - active)}");
        output.WriteLine(string.Join(" ", info.Split('\n').Select(s => s.Trim()).Where(s => s.StartsWith("used_memory:") || s.StartsWith("used_memory_peak:") || s.StartsWith("used_memory_overhead:") || s.StartsWith("used_memory_dataset:"))));
        Assert.InRange(total, 0, 67108864); Assert.InRange(accounted, 0, 5000);
        return new(total, events, active, accounted);
    }

    internal async Task CleanupQuotaAsync()
    {
        foreach (var batch in buckets.Keys.Select(k => (RedisKey)k).Append(Config.Keys.Registry).Append(Config.Keys.Metadata).Chunk(128))
            await Db.KeyDeleteAsync(batch);
    }

    internal async Task PaceAsync(int count, TimeSpan duration, Func<int, int, Task> action)
    {
        var timer = Stopwatch.StartNew();
        await Task.WhenAll(Enumerable.Range(0, 8).Select(async worker =>
        {
            for (var i = worker; i < count; i += 8)
            {
                var due = TimeSpan.FromTicks(duration.Ticks * i / count);
                if (due > timer.Elapsed) await Task.Delay(due - timer.Elapsed);
                await action(i, worker % 2);
            }
        }));
        if (timer.Elapsed < duration) await Task.Delay(duration - timer.Elapsed);
        output.WriteLine($"PACED ATTEMPTS={count} OFFERED_MS={duration.TotalMilliseconds} ELAPSED_MS={timer.Elapsed.TotalMilliseconds:F0} WORKERS=8 HOSTS=2");
    }

    private void Report()
    {
        foreach (var group in outcomes.GroupBy(r => (r.Key.Phase, r.Key.Host, r.Key.Operation)).OrderBy(g => g.Key))
            output.WriteLine($"PHASE={group.Key.Phase} HOST={(group.Key.Host == 0 ? "A" : "B")} OP={group.Key.Operation} ATTEMPTED={group.Sum(r => r.Value)} " +
                string.Join(" ", Enum.GetValues<AuthenticationQuotaStoreStatus>().Select(status => $"{status}={group.Where(r => r.Key.Status == status).Sum(r => r.Value)}")));
        output.WriteLine($"DISPATCHES_A={A?.Dispatches} DISPATCHES_B={B?.Dispatches} OBSERVED_MAX_QUOTA_BYTES={PeakBytes}");
    }

    internal sealed record Measurement(long Bytes, long Events, long Active, long Accounted);

    internal sealed class QuotaHost : WebApplicationFactory<AuthenticationOptions>
    {
        private readonly AuthenticationDependenciesFixture fixture;
        private readonly RedisAuthenticationQuotaConfiguration config;
        private readonly string hmac;
        private readonly IAuthenticationSecurityState global;
        private readonly DistributedQuotaDiagnostics? diagnostics;
        internal RedisOptions RedisOptions { get; }
        private long dispatches;
        internal long Dispatches => Interlocked.Read(ref dispatches);
        internal RedisConnections Connections => Services.GetRequiredService<RedisConnections>();
        internal RedisSecurityStateGuard Guard => Services.GetRequiredService<RedisSecurityStateGuard>();
        internal RedisSecurityStateDetector Detector => Services.GetRequiredService<RedisSecurityStateDetector>();
        internal HmacAuthenticationScopePseudonymizer Pseudonymizer => Services.GetRequiredService<HmacAuthenticationScopePseudonymizer>();
        internal RedisAuthenticationQuotaStore Store => Services.GetRequiredService<RedisAuthenticationQuotaStore>();

        internal QuotaHost(AuthenticationDependenciesFixture fixture, RedisAuthenticationQuotaConfiguration config, string hmac, string connection, IAuthenticationSecurityState? global, DistributedQuotaDiagnostics? diagnostics)
        {
            this.fixture = fixture; this.config = config; this.hmac = hmac; this.global = global ?? RedisAuthenticationQuotaTestScope.Open;
            this.diagnostics = diagnostics;
            RedisOptions = new() { ConnectionString = connection + ",allowAdmin=true", NamespacePrefix = "enver:" + config.Snapshot.Environment,
                OperationTimeoutMilliseconds = 2000, ConnectionTimeoutMilliseconds = 2000 };
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(config.Snapshot.Environment);
            builder.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(fixture.CreateConfiguration(new Dictionary<string, string?>
            {
                ["Redis:ConnectionString"] = RedisOptions.ConnectionString, ["Redis:NamespacePrefix"] = RedisOptions.NamespacePrefix,
                ["MongoDb:DatabaseName"] = "enver_" + config.Snapshot.Environment,
                ["RateLimiting:PseudonymizationKeyBase64"] = hmac, ["RateLimiting:MaxActivePartitions"] = "5000",
                ["RateLimiting:PolicyRevision"] = config.Snapshot.Policy.Revision,
                ["AuthenticationComposition:AuthorityRegion"] = "us-east-1",
                ["AuthenticationComposition:QuotaConfigurationVersion"] = config.Snapshot.ConfigurationVersion,
                ["AuthenticationComposition:QuotaIdentityReference"] = config.Snapshot.IdentityConfigurationReference,
                ["AuthenticationComposition:QuotaGeneration"] = config.Generation
            })));
            builder.ConfigureServices((context, services) =>
            {
                // No se prueba el cliente de autoridad desde esta factory. El store bajo prueba
                // conserva exactamente su colaborador global explícito y sus aserciones históricas.
                services.AddSingleton<IAuthenticationSecurityAuthority>(new AuthenticationSecurityAuthorityFake());
                services.AddSingleton(_ => new RedisConnections(RedisOptions));
                services.AddSingleton(p => new RedisSecurityStateGuard(p.GetRequiredService<RedisConnections>(), RedisOptions));
                services.AddSingleton(p => new RedisSecurityStateDetector(p.GetRequiredService<RedisConnections>(), p.GetRequiredService<RedisSecurityStateGuard>()));
                services.AddSingleton<HmacAuthenticationScopePseudonymizer>();
                services.AddSingleton(p =>
                {
                    _ = p.GetRequiredService<RedisSecurityStateDetector>();
                    return new RedisAuthenticationQuotaStore(p.GetRequiredService<RedisConnections>(), RedisOptions, config,
                        p.GetRequiredService<RedisSecurityStateGuard>(), global, async (budget, invocation) =>
                        {
                            Interlocked.Increment(ref dispatches);
                            Assert.InRange(invocation.Keys.Length, 4, 8); // 2 control + scopes + <=3 hints.
                            diagnostics?.Reached();
                            var database = (await Connections.GetMultiplexerAsync(budget.Token)).GetDatabase();
                            // Solo el camino diagnosticado usa la copia instrumentada de test;
                            // el resto de la suite conserva el script productivo sin envoltura.
                            var script = diagnostics is null
                                ? RedisAuthenticationQuotaScriptProtocol.ScriptText
                                : DistributedQuotaLuaGuardDiagnostics.ScriptText;
                            var pending = database.ScriptEvaluateAsync(
                                script, invocation.Keys, invocation.Arguments, CommandFlags.DemandMaster);
                            diagnostics?.Submitted();
                            var response = await pending;
                            if (diagnostics is not null)
                            {
                                response = DistributedQuotaLuaGuardDiagnostics.Unwrap(response, out var guard);
                                diagnostics.Lua(guard);
                            }
                            diagnostics?.Reply(response);
                            return response;
                        });
                });
            });
        }
        protected override IHost CreateHost(IHostBuilder builder)
        { builder.UseEnvironment(config.Snapshot.Environment); return base.CreateHost(builder); }
    }
}
