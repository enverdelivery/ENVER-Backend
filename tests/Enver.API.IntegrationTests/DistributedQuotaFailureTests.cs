using System.Globalization;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Aws;
using Enver.API.Infrastructure.Redis;
using Enver.API.Infrastructure.Security;
using Enver.Testing;
using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

// Fallos de infraestructura y de frontera sobre el MISMO mecanismo de cuotas de T-AUTH-021.
// Composición de test: no demuestra máquinas físicas, balanceador, AWS ni endpoint productivo.
[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class DistributedQuotaFailureTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    private static readonly TimeSpan SyntheticWindow = TimeSpan.FromSeconds(30);

    [Fact]
    public Task Post_dispatch_timeout_is_uncertain_without_redispatch_and_second_consumer_stays_operational() =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            var calls = 0;
            var options = new RedisOptions
            {
                ConnectionString = s.A.RedisOptions.ConnectionString,
                NamespacePrefix = s.A.RedisOptions.NamespacePrefix,
                OperationTimeoutMilliseconds = 50,
                ConnectionTimeoutMilliseconds = 2000
            };
            // Ambos consumidores comparten el mismo Redis; el global es el guard independiente del host B.
            var store = new RedisAuthenticationQuotaStore(s.A.Connections, options, s.Config, s.A.Guard, s.B.Guard,
                async (dependency, invocation) =>
                {
                    Interlocked.Increment(ref calls);
                    // Pausa finita del contenedor exclusivo de pruebas: el EVAL ya fue despachado.
                    await s.Db.ExecuteAsync("CLIENT", "PAUSE", "300", "ALL").WaitAsync(dependency.Token);
                    return await s.Db.ScriptEvaluateAsync(RedisAuthenticationQuotaScriptProtocol.ScriptText,
                        invocation.Keys, invocation.Arguments, CommandFlags.DemandMaster).WaitAsync(dependency.Token);
                });
            var request = s.Request(0);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
            {
                var result = await store.EvaluateAndConsumeAsync(request, budget);
                Assert.Equal(AuthenticationQuotaStoreStatus.Uncertain, result.Status);
                Assert.True(result.ResultUncertain);
                Assert.Null(result.RetryAfter);
                Assert.NotEqual(AuthenticationQuotaStoreStatus.Rejected, result.Status);
            }
            Assert.Equal(1, calls);
            // Solo diagnóstico posterior: no read-back del adapter para transformar el resultado incierto.
            await s.Data.NowAsync();
            var scores = await Task.WhenAll(request.Scopes.Select(scope =>
                s.Db.SortedSetScoreAsync(s.Data.Key(request, scope), request.InternalAttemptId)));
            Assert.All(scores, value => Assert.NotNull(value));
            Assert.All(scores, value => Assert.Equal(scores[0], value));
            output.WriteLine($"UNCERTAIN_SCOPES={scores.Length} SAME_ATTEMPT_AT_MOST_ONCE=true DISPATCHES={calls}");
            // El segundo consumidor independiente no reprodujo el intento incierto y sigue admitiendo.
            var before = s.B.Dispatches;
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted,
                (await s.SendAsync(1, s.Request(1), "after-timeout")).Status);
            Assert.Equal(before + 1, s.B.Dispatches);
            Assert.Equal(1, calls);
        }, budget: TimeSpan.FromSeconds(180));

    [Fact]
    public Task Restart_with_two_consumers_keeps_guard_closed_and_generation_recovery_is_explicit() =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            var request = s.Request(0);
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(0, request, "pre-restart")).Status);
            var before = await ObserveAsync(s, request, "BEFORE_RESTART");
            foreach (var bucket in before.Buckets) Assert.Single(bucket);
            var accounted = await s.Data.CountAsync();
            Assert.Equal(request.Scopes.Count, accounted);
            var dispatchesBefore = s.A.Dispatches + s.B.Dispatches;
            var aObserved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var bObserved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            s.A.Detector.SignalObserved += (_, signal) =>
            {
                if (signal.Signal == RedisConnectionSignal.ConnectionFailed) aObserved.TrySetResult();
            };
            s.B.Detector.SignalObserved += (_, signal) =>
            {
                if (signal.Signal == RedisConnectionSignal.ConnectionFailed) bObserved.TrySetResult();
            };
            var endpointBefore = RedisAuthenticationQuotaTestScope.Endpoint(fixture.RedisConnectionString);
            var containerBefore = fixture.RedisContainerId;
            await fixture.StopRedisAsync();
            try { await Task.WhenAll(aObserved.Task, bObserved.Task).WaitAsync(TimeSpan.FromSeconds(15)); }
            finally { await fixture.StartRedisAsync(); }
            Assert.Equal(containerBefore, fixture.RedisContainerId);
            var endpointAfter = RedisAuthenticationQuotaTestScope.Endpoint(fixture.RedisConnectionString);
            output.WriteLine($"RESTART_ENDPOINT_CHANGED={endpointBefore != endpointAfter} TWO_CONSUMERS_OBSERVED_FAILURE=true");
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
                Assert.False((await s.A.Guard.EvaluateAsync(budget)).CanContinue);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
                Assert.False((await s.B.Guard.EvaluateAsync(budget)).CanContinue);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
            {
                Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
                    (await s.A.Store.EvaluateAndConsumeAsync(s.Request(1), budget)).Status);
            }
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
            {
                Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
                    (await s.B.Store.EvaluateAndConsumeAsync(s.Request(1), budget)).Status);
            }
            Assert.Equal(dispatchesBefore, s.A.Dispatches + s.B.Dispatches);
            // Consumidores nuevos y explícitos, prestados por el scope de datos para su cleanup.
            var aConnection = await s.Data.CreateConnectionAfterControlledRestartAsync(fixture.RedisConnectionString, output.WriteLine);
            var afterRestart = await ObserveAsync(s, request, "AFTER_RESTART_BEFORE_CONSUME");
            for (var i = 0; i < request.Scopes.Count; i++)
            {
                var window = RedisAuthenticationQuotaConfiguration.WindowMicroseconds(request.Scopes[i].Definition.Window);
                var live = Live(before.Buckets[i], afterRestart.NowUs, window);
                Assert.True(live.Length > 0, "El restart superó la ventana sintética; evidencia no-reset inconclusa.");
                foreach (var entry in live)
                    Assert.Contains(afterRestart.Buckets[i], current => current.Element == entry.Element && current.Score == entry.Score);
            }
            Assert.Equal(accounted, await s.Data.CountAsync());
            Assert.Equal(s.Config.Generation, (await s.Db.HashGetAsync(s.Config.Keys.Metadata, "generation")).ToString());
            // Fence explícito de cuotas, sin reabrir el guard de seguridad ni reinicializar el ambiente.
            var generation = Guid.NewGuid().ToString("N");
            var oldControl = new RedisAuthenticationQuotaLifecycle(aConnection, s.Data.Options, s.Config,
                RedisAuthenticationQuotaTestScope.Open, RedisAuthenticationQuotaTestScope.Open);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
                Assert.Equal("fenced", (await oldControl.FenceAsync(generation, budget)).Status);
            var config = new RedisAuthenticationQuotaConfiguration(s.Config.Snapshot, generation);
            var bConnection = await s.Data.CreateConnectionAfterControlledRestartAsync(fixture.RedisConnectionString, output.WriteLine);
            var control = new RedisAuthenticationQuotaLifecycle(bConnection, s.Data.Options, config,
                RedisAuthenticationQuotaTestScope.Open, RedisAuthenticationQuotaTestScope.Open);
            var oldStore = s.Data.NewStore(aConnection, s.Config, RedisAuthenticationQuotaTestScope.Open, RedisAuthenticationQuotaTestScope.Open);
            var newStore = s.Data.NewStore(bConnection, config, RedisAuthenticationQuotaTestScope.Open, RedisAuthenticationQuotaTestScope.Open);
            Assert.Equal("closed", (await s.Db.HashGetAsync(config.Keys.Metadata, "state")).ToString());
            Assert.Equal(generation, (await s.Db.HashGetAsync(config.Keys.Metadata, "generation")).ToString());
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
                (await s.SendAsync(0, s.Request(0), "old-generation", oldStore)).Status);
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
                (await s.SendAsync(1, s.Request(0), "new-before-drain", newStore)).Status);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
                Assert.Equal("unavailable", (await control.ActivateAfterDrainAsync(budget)).Status);
            var drain = (long)await s.Db.HashGetAsync(config.Keys.Metadata, "drain");
            await s.Data.WaitUntilAsync(drain, TimeSpan.FromSeconds(45));
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
                Assert.Equal("activated", (await control.ActivateAfterDrainAsync(budget)).Status);
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
                (await s.SendAsync(0, s.Request(1), "old-generation-after", oldStore)).Status);
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted,
                (await s.SendAsync(1, s.Request(1), "new-generation", newStore)).Status);
            Assert.Equal("ready", (await s.Db.HashGetAsync(config.Keys.Metadata, "state")).ToString());
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
                Assert.False((await s.A.Guard.EvaluateAsync(budget)).CanContinue);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
                Assert.False((await s.B.Guard.EvaluateAsync(budget)).CanContinue);
            output.WriteLine("FENCE_AND_RECOVERY_OK GUARDS_REMAIN_CLOSED=true");
        }, SyntheticWindow, TimeSpan.FromSeconds(180));

    [Fact]
    public Task Invalid_revision_and_divergent_snapshot_fail_closed_without_mutation() =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            var calls = 0;
            RedisAuthenticationQuotaStore Counter(RedisAuthenticationQuotaConfiguration config) => new(
                s.A.Connections, s.A.RedisOptions, config, s.A.Guard, s.B.Guard,
                async (dependency, invocation) =>
                {
                    Interlocked.Increment(ref calls);
                    var database = (await s.A.Connections.GetMultiplexerAsync(dependency.Token)).GetDatabase();
                    return await database.ScriptEvaluateAsync(RedisAuthenticationQuotaScriptProtocol.ScriptText,
                        invocation.Keys, invocation.Arguments, CommandFlags.DemandMaster)
                        .WaitAsync(dependency.Token);
                });
            var forged = s.Request(0, revision: "forged-revision");
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
                (await s.SendAsync(0, forged, "invalid-revision", Counter(s.Config))).Status);
            Assert.Equal(0, calls);
            var divergent = new RedisAuthenticationQuotaConfiguration(new(
                s.Config.Snapshot.Environment, "config-divergent", "identity-053",
                AuthenticationQuotaPolicyDescriptor.AcademicInitial,
                AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(5000)), s.Config.Generation);
            var request = s.Request(0);
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
                (await s.SendAsync(0, request, "divergent-snapshot", Counter(divergent))).Status);
            Assert.Equal(1, calls);
            foreach (var scope in request.Scopes)
            {
                Assert.False(await s.Db.KeyExistsAsync(s.Data.Key(request, scope)));
                Assert.Null(await s.Db.SortedSetScoreAsync(s.Config.Keys.Registry, s.Data.Member(request, scope)));
            }
            Assert.Equal(0, await s.Data.CountAsync());
            Assert.Equal("0", (await s.Db.HashGetAsync(s.Config.Keys.Metadata, "dirty")).ToString());
            Assert.Equal("ready", (await s.Db.HashGetAsync(s.Config.Keys.Metadata, "state")).ToString());
            output.WriteLine($"REVISION_DISPATCHES=0 DIVERGENT_DISPATCHES={calls} NO_PARTIAL_PARTITIONS=true");
        }, budget: TimeSpan.FromSeconds(180));

    [Fact]
    public Task Closed_global_authority_blocks_both_consumers_and_quota_fence_never_reopens_it() =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            using var firstClient = fixture.CreateDynamoDbClient();
            using var secondClient = fixture.CreateDynamoDbClient();
            await fixture.DeleteAuthenticationSecurityAuthorityAsync(firstClient);
            await fixture.BootstrapAuthenticationSecurityAuthorityAsync(firstClient);
            var first = Coordinator(Authority(firstClient));
            var second = Coordinator(Authority(secondClient));
            using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.True((await first.EvaluateAsync(budget)).CanContinue);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.True((await second.EvaluateAsync(budget)).CanContinue);
            // Consumidor adicional de test que observa la autoridad compartida 020 y mantiene su guard local abierto.
            var blockedHost = s.Own(s.Host(global: second));
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
                Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, (await first.CloseAsync(budget)).Status);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.False((await second.EvaluateAsync(budget)).CanContinue);
            var database = (await s.A.Connections.GetMultiplexerAsync()).GetDatabase();
            Assert.True((await database.PingAsync()).TotalMilliseconds >= 0);
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
                (await s.SendAsync(0, s.Request(0), "global-closed", blockedHost.Store)).Status);
            Assert.Equal(0, blockedHost.Dispatches);
            // El fence de cuotas es CONTROL de Redis y no toca la autoridad global 020.
            var generation = Guid.NewGuid().ToString("N");
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
                Assert.Equal("fenced", (await s.Data.Lifecycle.FenceAsync(generation, budget)).Status);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.False((await second.EvaluateAsync(budget)).CanContinue);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.False((await first.EvaluateAsync(budget)).CanContinue);
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
                (await s.SendAsync(0, s.Request(1), "global-still-closed", blockedHost.Store)).Status);
            output.WriteLine("GLOBAL_AUTHORITY_CLOSED_BLOCKS_QUOTA=true QUOTA_FENCE_DOES_NOT_REOPEN_020=true");
            // Higiene del fixture aislado; no forma parte de la evidencia ni reabre confianza productiva.
            await fixture.DeleteAuthenticationSecurityAuthorityAsync(firstClient);
            await fixture.BootstrapAuthenticationSecurityAuthorityAsync(firstClient);
        }, budget: TimeSpan.FromSeconds(180));

    [Fact]
    public async Task Quota_operation_hits_real_OOM_without_admission_or_retry()
    {
        await using var redis = new RedisBuilder(AuthenticationDependenciesFixture.RedisImage)
            .WithCommand("redis-server", "--maxmemory-policy", "noeviction", "--maxmemory", "16mb")
            .Build();
        await redis.StartAsync();
        var environment = "oom_055_" + Guid.NewGuid().ToString("N")[..8];
        var configuration = new RedisAuthenticationQuotaConfiguration(new(
            environment, "config-055-oom", "identity-053",
            AuthenticationQuotaPolicyDescriptor.AcademicInitial,
            AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(5000)), Guid.NewGuid().ToString("N"));
        var options = new RedisOptions
        {
            ConnectionString = redis.GetConnectionString() + ",allowAdmin=true",
            NamespacePrefix = "enver:" + environment,
            OperationTimeoutMilliseconds = 2000,
            ConnectionTimeoutMilliseconds = 2000
        };
        await using var connections = new RedisConnections(options);
        var database = (await connections.GetMultiplexerAsync()).GetDatabase();
        var lifecycle = new RedisAuthenticationQuotaLifecycle(connections, options, configuration,
            RedisAuthenticationQuotaTestScope.Open, RedisAuthenticationQuotaTestScope.Open);
        using (var budget = RedisAuthenticationQuotaTestScope.Budget())
            Assert.Equal("initialized", (await lifecycle.InitializeVerifiedNewEnvironmentAsync(budget)).Status);
        // Relleno único por script: el servidor queda holgadamente sobre maxmemory antes de la operación de cuotas.
        const int fillers = 400;
        const string fill =
            "local v=string.rep('x',65536) local n=0 for i=1,tonumber(ARGV[1]) do " +
            "redis.call('set',KEYS[1]..i,v) n=n+1 end return n";
        var inserted = (long)await database.ScriptEvaluateAsync(fill,
            new RedisKey[] { $"enver:{environment}:oom-filler:" }, new RedisValue[] { fillers }, CommandFlags.DemandMaster);
        Assert.Equal(fillers, inserted);
        var usedMemory = await UsedMemoryAsync(database);
        var maxMemory = await ConfigAsync(database, "maxmemory");
        var policy = await ConfigTextAsync(database, "maxmemory-policy");
        output.WriteLine($"OOM_DEDICATED_REDIS FILLERS={inserted} USED_MEMORY={usedMemory} MAXMEMORY={maxMemory} POLICY={policy}");
        Assert.Equal("noeviction", policy);
        Assert.True(usedMemory > maxMemory, "La memoria usada no superó maxmemory.");
        // Escritura directa rechazada: el servidor está realmente en OOM antes de invocar la cuota.
        var directOom = await Assert.ThrowsAsync<RedisServerException>(() =>
            database.StringSetAsync($"enver:{environment}:oom-probe", "v", flags: CommandFlags.DemandMaster));
        Assert.Contains("OOM", directOom.Message, StringComparison.Ordinal);
        output.WriteLine($"OOM_DIRECT_WRITE_REJECTED={directOom.Message}");
        var calls = 0;
        var store = new RedisAuthenticationQuotaStore(connections, options, configuration,
            RedisAuthenticationQuotaTestScope.Open, RedisAuthenticationQuotaTestScope.Open,
            async (dependency, invocation) =>
            {
                Interlocked.Increment(ref calls);
                var target = (await connections.GetMultiplexerAsync(dependency.Token)).GetDatabase();
                return await target.ScriptEvaluateAsync(RedisAuthenticationQuotaScriptProtocol.ScriptText,
                    invocation.Keys, invocation.Arguments, CommandFlags.DemandMaster).WaitAsync(dependency.Token);
            });
        var request = Request(configuration, 1);
        using (var budget = RedisAuthenticationQuotaTestScope.Budget())
        {
            var result = await store.EvaluateAndConsumeAsync(request, budget);
            output.WriteLine($"OOM_QUOTA_RESULT STATUS={result.Status} DISPATCHES={calls}");
            Assert.Contains(result.Status, new[] { AuthenticationQuotaStoreStatus.Unavailable, AuthenticationQuotaStoreStatus.Uncertain });
            Assert.NotEqual(AuthenticationQuotaStoreStatus.Admitted, result.Status);
            Assert.Null(result.RetryAfter);
        }
        Assert.Equal(1, calls);
        // Redis sigue respondiendo a lecturas: el fallo es OOM de escritura, no indisponibilidad general.
        Assert.True((await database.PingAsync()).TotalMilliseconds >= 0);
        foreach (var scope in request.Scopes)
        {
            Assert.False(await database.KeyExistsAsync(configuration.Keys.Bucket(request.Operation, scope.ScopeType, scope.OpaqueId)));
            Assert.Null(await database.SortedSetScoreAsync(configuration.Keys.Registry,
                RedisAuthenticationQuotaKeyBuilder.Member(request.Operation, scope.ScopeType, scope.OpaqueId)));
        }
        Assert.Equal(0, await database.SortedSetLengthAsync(configuration.Keys.Registry));
        // La primera escritura del script es HSET metadata dirty 1; al rechazarse, no hay mutación.
        var dirty = (await database.HashGetAsync(configuration.Keys.Metadata, "dirty")).ToString();
        Assert.Equal("0", dirty);
        Assert.Equal("ready", (await database.HashGetAsync(configuration.Keys.Metadata, "state")).ToString());
        output.WriteLine($"OOM_FIRST_SCRIPT_WRITE_REJECTED=true DIRTY={dirty} NO_ADMISSION=true");
        // Liberar relleno acredita que la MISMA operación de cuotas funciona sin presión de memoria.
        var fillerKeys = Enumerable.Range(1, fillers).Select(i => (RedisKey)$"enver:{environment}:oom-filler:{i}").ToArray();
        foreach (var batch in fillerKeys.Chunk(128)) await database.KeyDeleteAsync(batch, CommandFlags.DemandMaster);
        Assert.True(await UsedMemoryAsync(database) < maxMemory);
        using (var budget = RedisAuthenticationQuotaTestScope.Budget())
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await store.EvaluateAndConsumeAsync(request, budget)).Status);
        Assert.Equal(2, calls);
        var keys = request.Scopes
            .Select(scope => (RedisKey)configuration.Keys.Bucket(request.Operation, scope.ScopeType, scope.OpaqueId))
            .Append(configuration.Keys.Registry).Append(configuration.Keys.Metadata).ToArray();
        await database.KeyDeleteAsync(keys, CommandFlags.DemandMaster);
        await redis.StopAsync();
        output.WriteLine("OOM_OPERATION_ISOLATED=true NO_ADMISSION_NO_RETRY=true");
    }

    private IAuthenticationSecurityAuthority Authority(Amazon.DynamoDBv2.IAmazonDynamoDB client) =>
        new DynamoDbAuthenticationSecurityAuthority(client, new AuthenticationSecurityAuthorityOptions
        {
            TableName = fixture.DynamoDbTableName,
            OperationTimeoutMilliseconds = 1000
        });

    private static AuthenticationSecurityCoordinator Coordinator(IAuthenticationSecurityAuthority authority) =>
        new(authority, new JwtOptions { LifetimeSeconds = 900, ClockSkewSeconds = 30 },
            new RedisOptions { RevocationOperationalMarginSeconds = 60 }, TimeProvider.System);

    private static AuthenticationQuotaConsumeRequest Request(RedisAuthenticationQuotaConfiguration configuration, int seed) =>
        new(AuthenticationOperation.Register, Guid.NewGuid().ToString("N"), configuration.Snapshot.Policy.Revision,
            AuthenticationQuotaPolicyDescriptor.RequiredScopes(AuthenticationOperation.Register).Select(type =>
                new AuthenticationQuotaScope(type,
                    RedisAuthenticationQuotaTestScope.Opaque(seed * 10 + (int)type),
                    configuration.Snapshot.Policy.GetDefinition(AuthenticationOperation.Register, type))));

    private static async Task<long> UsedMemoryAsync(IDatabase database)
    {
        var text = (await database.ExecuteAsync("INFO", "MEMORY")).ToString();
        var line = text.Split('\n').Select(value => value.Trim())
            .Single(value => value.StartsWith("used_memory:", StringComparison.Ordinal));
        return long.Parse(line["used_memory:".Length..], CultureInfo.InvariantCulture);
    }

    private static async Task<long> ConfigAsync(IDatabase database, string name)
    {
        var values = (RedisResult[])(await database.ExecuteAsync("CONFIG", "GET", name))!;
        Assert.Equal(2, values.Length);
        return long.Parse(values[1].ToString(), CultureInfo.InvariantCulture);
    }

    private static async Task<string> ConfigTextAsync(IDatabase database, string name)
    {
        var values = (RedisResult[])(await database.ExecuteAsync("CONFIG", "GET", name))!;
        Assert.Equal(2, values.Length);
        return values[1].ToString();
    }

    private static SortedSetEntry[] Live(SortedSetEntry[] events, long nowUs, long windowUs) =>
        events.Where(entry => entry.Score > nowUs - windowUs && entry.Score <= nowUs).ToArray();

    private async Task<(long NowUs, SortedSetEntry[][] Buckets)> ObserveAsync(DistributedQuotaTestScope scope,
        AuthenticationQuotaConsumeRequest request, string phase)
    {
        // Observación read-only del reloj lógico y scores; no repara ni poda.
        const string observation = "local t = redis.call('TIME'); local now = tonumber(t[1]) * 1000000 + tonumber(t[2]); " +
            "local last = tonumber(redis.call('HGET', KEYS[1], 'last')); if not last then return redis.error_reply('Invalid test metadata') end; now = math.max(now, last); " +
            "local result = {string.format('%.0f', now)}; for i = 2, #KEYS do " +
            "result[#result + 1] = redis.call('ZRANGE', KEYS[i], 0, -1, 'WITHSCORES'); end; return result";
        var keys = new[] { (RedisKey)scope.Config.Keys.Metadata }.Concat(request.Scopes.Select(scope2 => (RedisKey)scope.Data.Key(request, scope2))).ToArray();
        var result = (RedisResult[])(await scope.Db.ScriptEvaluateAsync(observation, keys, [],
            CommandFlags.DemandMaster))!;
        Assert.Equal(keys.Length, result.Length);
        var nowUs = (long)result[0];
        var buckets = new SortedSetEntry[keys.Length - 1][];
        for (var i = 0; i < buckets.Length; i++)
        {
            var row = (RedisResult[])result[i + 1]!;
            Assert.Equal(0, row.Length % 2);
            buckets[i] = Enumerable.Range(0, row.Length / 2)
                .Select(j => new SortedSetEntry((string)row[2 * j]!, (double)row[2 * j + 1])).ToArray();
            foreach (var entry in buckets[i]) Assert.True(entry.Score <= nowUs);
            output.WriteLine($"{phase} SCOPE={request.Scopes[i].ScopeType} NOW_US={nowUs} RAW={buckets[i].Length}");
        }
        return (nowUs, buckets);
    }
}
