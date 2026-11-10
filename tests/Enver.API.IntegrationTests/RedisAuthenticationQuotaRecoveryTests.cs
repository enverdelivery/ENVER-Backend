using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using System.Globalization;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisAuthenticationQuotaRecoveryTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Residual_registry_and_inactive_bucket_are_released_lazily(bool residualBucket)
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var first = s.Request();
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(first)).Status);
        var scope = first.Scopes.Single(x => x.ScopeType == AuthenticationQuotaScopeType.Email);
        var key = s.Key(first, scope);
        var horizon = await s.NowAsync() - 1000000;
        await s.Database.KeyDeleteAsync(key);
        if (residualBucket)
        {
            await s.Database.SortedSetAddAsync(key, Guid.NewGuid().ToString("N"), horizon - 900000000);
            await s.Database.ExecuteAsync("PEXPIREAT", key, (await s.NowAsync() + 60000000) / 1000);
        }
        await s.Database.SortedSetAddAsync(s.Configuration.Keys.Registry, s.Member(first, scope), horizon);
        await s.SynchronizeMetadataAsync();
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(s.Request(2))).Status);
        Assert.False(await s.Database.KeyExistsAsync(key));
        Assert.Null(await s.Database.SortedSetScoreAsync(s.Configuration.Keys.Registry, s.Member(first, scope)));
        Assert.Equal(3, await s.CountAsync());
    }

    [Fact]
    public async Task Renewed_candidate_is_revalidated_and_never_released_from_stale_hints()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var first = s.Request();
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(first)).Status);
        var member = s.Member(first, first.Scopes[0]);
        var next = s.Request(2);
        var invocation = RedisAuthenticationQuotaScriptProtocol.Encode(s.Configuration, "evaluate", next, [member]);
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(s.Request(), s.SecondStore)).Status);
        var response = RedisAuthenticationQuotaScriptProtocol.Parse(await s.Database.ScriptEvaluateAsync(RedisAuthenticationQuotaScriptProtocol.ScriptText,
            invocation.Keys, invocation.Arguments, CommandFlags.DemandMaster));
        Assert.Equal("admitted", response.Status);
        Assert.Equal(2, await s.Database.SortedSetLengthAsync(s.Key(first, first.Scopes[0])));
        Assert.Equal(4, await s.CountAsync());
    }

    [Theory]
    [InlineData("bucket")]
    [InlineData("registry")]
    [InlineData("dirty")]
    [InlineData("metadata")]
    public async Task Detected_loss_or_incomplete_marker_never_bootstraps_and_controlled_drain_recovers(string loss)
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture, TimeSpan.FromSeconds(1));
        var first = s.Request();
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(first)).Status);
        if (loss == "bucket") await s.Database.KeyDeleteAsync(s.Key(first, first.Scopes[0]));
        if (loss == "registry") await s.Database.KeyDeleteAsync(s.Configuration.Keys.Registry);
        if (loss == "metadata") await s.Database.KeyDeleteAsync(s.Configuration.Keys.Metadata);
        if (loss == "dirty") await s.Database.HashSetAsync(s.Configuration.Keys.Metadata, "dirty", "1");
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(s.Request(), s.SecondStore)).Status);
        var generation = Guid.NewGuid().ToString("N");
        using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.Equal("fenced", (await s.Lifecycle.FenceAsync(generation, budget)).Status);
        var config = new RedisAuthenticationQuotaConfiguration(s.Configuration.Snapshot, generation);
        var lifecycle = s.NewLifecycle(config);
        using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.Equal("unavailable", (await lifecycle.ActivateAfterDrainAsync(budget)).Status);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(s.Request())).Status);
        var drain = (long)await s.Database.HashGetAsync(config.Keys.Metadata, "drain");
        await s.WaitUntilAsync(drain);
        using (var budget = RedisAuthenticationQuotaTestScope.Budget())
            Assert.Equal("maintained", (await lifecycle.ReconcileBatchAsync(first.Scopes.Select(scope => s.Member(first, scope)), budget)).Status);
        using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.Equal("activated", (await lifecycle.ActivateAfterDrainAsync(budget)).Status);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(s.Request())).Status);
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(s.Request(), s.NewStore(s.SecondConnections, config))).Status);
    }

    [Fact]
    public async Task Closed_recovery_inventories_unregistered_permanent_residue_before_activation()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture, TimeSpan.FromSeconds(1));
        var request = s.Request();
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(request)).Status);
        // Estado de fallo parcial preparado explícitamente, no pretendido como fallo real de infraestructura.
        var orphan = s.Key(request, request.Scopes[0]);
        await s.Database.KeyPersistAsync(orphan);
        await s.Database.KeyDeleteAsync(s.Configuration.Keys.Registry);
        await s.Database.HashSetAsync(s.Configuration.Keys.Metadata, "dirty", "1");
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(s.Request())).Status);
        var generation = Guid.NewGuid().ToString("N");
        using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.Equal("fenced", (await s.Lifecycle.FenceAsync(generation, budget)).Status);
        await s.WaitUntilAsync((long)await s.Database.HashGetAsync(s.Configuration.Keys.Metadata, "drain"));
        var config = new RedisAuthenticationQuotaConfiguration(s.Configuration.Snapshot, generation);
        using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.Equal("activated", (await s.NewLifecycle(config).ActivateAfterDrainAsync(budget)).Status);
        Assert.False(await s.Database.KeyExistsAsync(orphan));
        Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(s.Request(), s.NewStore(s.SecondConnections, config))).Status);
    }

    [Fact]
    public async Task Active_bucket_without_registry_and_future_event_fail_closed()
    {
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var request = s.Request();
        var scope = request.Scopes[0];
        await s.Database.SortedSetAddAsync(s.Key(request, scope), Guid.NewGuid().ToString("N"), await s.NowAsync());
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(request)).Status);
        await s.Database.KeyDeleteAsync(s.Key(request, scope));
        await s.SeedAsync(request, scope, [await s.NowAsync() + 60000000]);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(request)).Status);
        Assert.Equal(1, await s.CountAsync());
    }

    [Fact]
    public async Task Controlled_restart_does_not_reopen_guard_or_reset_quota_and_explicit_fence_recovers()
    {
        // NO RESET requiere eventos todavía vigentes; expiración natural se prueba por separado en
        // Window_both_sides_and_expiry_release_capacity_with_real_TIME. Ninguna cuota productiva cambia.
        var testWindow = TimeSpan.FromSeconds(30);
        var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture, testWindow);
        Exception? testFailure = null;
        try
        {
            var request = s.Request();
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(request)).Status);
            var beforeRestart = await ObserveAsync(s, request, "BEFORE_RESTART");
            foreach (var bucket in beforeRestart.Buckets) Assert.Single(bucket);
            var accountedBeforeRestart = await s.CountAsync();
            Assert.Equal(request.Scopes.Count, accountedBeforeRestart);
            var guard = new RedisSecurityStateGuard(s.Connections, s.Options);
            using var detector = new RedisSecurityStateDetector(s.Connections, guard);
            var observed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            detector.SignalObserved += (_, signal) => { if (signal.Signal == RedisConnectionSignal.ConnectionFailed) observed.TrySetResult(); };
            var endpointBefore = RedisAuthenticationQuotaTestScope.Endpoint(fixture.RedisConnectionString);
            var capturedEndpoint = RedisAuthenticationQuotaTestScope.Endpoint(s.Options.ConnectionString);
            var containerBefore = fixture.RedisContainerId;
            output.WriteLine($"BEFORE_HOST={endpointBefore.Host} BEFORE_PORT={endpointBefore.Port}");
            Assert.Equal(endpointBefore, capturedEndpoint);
            await fixture.StopRedisAsync();
            try { await observed.Task.WaitAsync(TimeSpan.FromSeconds(10)); }
            finally { await fixture.StartRedisAsync(); }
            var endpointAfter = RedisAuthenticationQuotaTestScope.Endpoint(fixture.RedisConnectionString);
            output.WriteLine($"AFTER_HOST={endpointAfter.Host} AFTER_PORT={endpointAfter.Port} ENDPOINT_CHANGED={endpointBefore != endpointAfter}");
            output.WriteLine($"SAME_CONTAINER_ID={containerBefore == fixture.RedisContainerId} CAPTURED_OPTIONS_STALE={capturedEndpoint != endpointAfter}");
            Assert.Equal(containerBefore, fixture.RedisContainerId);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.False((await guard.EvaluateAsync(budget)).CanContinue);
            // Consumidor nuevo explícito, prestado por s: su owner limpia Redis antes de disponerlo.
            var restarted = await s.CreateConnectionAfterControlledRestartAsync(fixture.RedisConnectionString, output.WriteLine);
            // Observación read-only ANTES del siguiente consumo: no podar ni reparar para hacer pasar el test.
            var afterRestart = await ObserveAsync(s, request, "AFTER_RESTART_BEFORE_CONSUME");
            for (var i = 0; i < request.Scopes.Count; i++)
            {
                var windowUs = RedisAuthenticationQuotaConfiguration.WindowMicroseconds(request.Scopes[i].Definition.Window);
                var expectedLive = Live(beforeRestart.Buckets[i], afterRestart.NowUs, windowUs);
                Assert.True(expectedLive.Length > 0, "El restart superó la ventana sintética; evidencia no-reset inconclusa, no inferir reset.");
                foreach (var entry in expectedLive)
                    Assert.True(afterRestart.Buckets[i].Any(current => current.Element == entry.Element && current.Score == entry.Score),
                        "Desapareció un evento que sigue vigente según Redis TIME.");
                Assert.Equal(expectedLive.Length, Live(afterRestart.Buckets[i], afterRestart.NowUs, windowUs).Length);
            }
            Assert.Equal(accountedBeforeRestart, await s.CountAsync());
            Assert.Equal(accountedBeforeRestart, await s.Database.SortedSetLengthAsync(s.Configuration.Keys.Registry));
            Assert.Equal(s.Configuration.Generation, (await s.Database.HashGetAsync(s.Configuration.Keys.Metadata, "generation")).ToString());
            var elapsedUs = afterRestart.NowUs - (long)beforeRestart.Buckets[0][0].Score;
            output.WriteLine($"RESTART_ELAPSED_REDIS_US={elapsedUs} WOULD_EXPIRE_WITH_OLD_1S_WINDOW={elapsedUs >= 1000000}");
            output.WriteLine("LIVE_EVENTS_AND_ACCOUNTED_CAPACITY_PRESERVED_BEFORE_CONSUME=true");
            var previousGenerationStore = s.NewStore(restarted, s.Configuration);
            var nextRequest = s.Request();
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(nextRequest, previousGenerationStore)).Status);
            var afterConsume = await ObserveAsync(s, nextRequest, "AFTER_CONSUME");
            for (var i = 0; i < request.Scopes.Count; i++)
            {
                var windowUs = RedisAuthenticationQuotaConfiguration.WindowMicroseconds(request.Scopes[i].Definition.Window);
                var stillLive = Live(afterRestart.Buckets[i], afterConsume.NowUs, windowUs);
                var currentLive = Live(afterConsume.Buckets[i], afterConsume.NowUs, windowUs);
                foreach (var entry in stillLive)
                    Assert.True(currentLive.Any(current => current.Element == entry.Element && current.Score == entry.Score),
                        "El consumo posterior eliminó un evento previo todavía vigente.");
                Assert.Single(currentLive, entry => entry.Element.ToString() == nextRequest.InternalAttemptId);
                Assert.Equal(stillLive.Length + 1, currentLive.Length);
            }
            Assert.Equal(accountedBeforeRestart, await s.CountAsync());
            output.WriteLine("NO_RESET_ASSERTIONS=OK");
            var generation = Guid.NewGuid().ToString("N");
            var oldControl = new RedisAuthenticationQuotaLifecycle(restarted, s.Options, s.Configuration, RedisAuthenticationQuotaTestScope.Open, RedisAuthenticationQuotaTestScope.Open);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
            {
                var fence = await oldControl.FenceAsync(generation, budget);
                output.WriteLine($"FENCE_STATUS={fence.Status} SEMANTIC={fence.IsSemantic} CATEGORY={fence.Category}");
                Assert.Equal("fenced", fence.Status);
            }
            var config = new RedisAuthenticationQuotaConfiguration(s.Configuration.Snapshot, generation);
            var control = new RedisAuthenticationQuotaLifecycle(restarted, s.Options, config, RedisAuthenticationQuotaTestScope.Open, RedisAuthenticationQuotaTestScope.Open);
            var newGenerationStore = s.NewStore(restarted, config);
            Assert.Equal("closed", (await s.Database.HashGetAsync(config.Keys.Metadata, "state")).ToString());
            Assert.Equal(generation, (await s.Database.HashGetAsync(config.Keys.Metadata, "generation")).ToString());
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(s.Request(), previousGenerationStore)).Status);
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(s.Request(), newGenerationStore)).Status);
            using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.Equal("unavailable", (await control.ActivateAfterDrainAsync(budget)).Status);
            await s.WaitUntilAsync((long)await s.Database.HashGetAsync(config.Keys.Metadata, "drain"), TimeSpan.FromSeconds(45));
            using (var budget = RedisAuthenticationQuotaTestScope.Budget())
                Assert.Equal("maintained", (await control.ReconcileBatchAsync(request.Scopes.Select(scope => s.Member(request, scope)), budget)).Status);
            Assert.Equal(0, await s.CountAsync());
            Assert.Equal("closed", (await s.Database.HashGetAsync(config.Keys.Metadata, "state")).ToString());
            using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.Equal("activated", (await control.ActivateAfterDrainAsync(budget)).Status);
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(s.Request(), previousGenerationStore)).Status);
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(s.Request(), newGenerationStore)).Status);
            Assert.Equal("ready", (await s.Database.HashGetAsync(config.Keys.Metadata, "state")).ToString());
            Assert.Equal(request.Scopes.Count, await s.CountAsync());
            using (var budget = RedisAuthenticationQuotaTestScope.Budget()) Assert.False((await guard.EvaluateAsync(budget)).CanContinue);
            output.WriteLine("RECOVERY_AND_FENCING_ASSERTIONS=OK ORIGINAL_SECURITY_GUARD_REMAINS_CLOSED=true");
        }
        catch (Exception failure) { testFailure = failure; throw; }
        finally
        {
            try
            {
                await s.DisposeAsync();
                output.WriteLine("OWNED_KEYS_CLEANUP_AND_CONNECTION_DISPOSAL=OK");
            }
            catch (Exception cleanupFailure) when (testFailure is not null)
            {
                throw new AggregateException("Fallaron la prueba de recovery y su cleanup.", testFailure, cleanupFailure);
            }
        }
    }

    private static SortedSetEntry[] Live(SortedSetEntry[] events, long nowUs, long windowUs) =>
        events.Where(entry => entry.Score > nowUs - windowUs && entry.Score <= nowUs).ToArray();

    private async Task<(long NowUs, SortedSetEntry[][] Buckets)> ObserveAsync(RedisAuthenticationQuotaTestScope scope,
        AuthenticationQuotaConsumeRequest request, string phase)
    {
        // Script EXCLUSIVAMENTE DE TEST y read-only: TIME + scores observados en una misma ejecución.
        const string observation = "local t = redis.call('TIME'); local now = tonumber(t[1]) * 1000000 + tonumber(t[2]); " +
            "local result = {string.format('%.0f', now)}; for i = 1, #KEYS do " +
            "result[#result + 1] = redis.call('ZRANGE', KEYS[i], 0, -1, 'WITHSCORES'); end; return result";
        var keys = request.Scopes.Select(s => (RedisKey)scope.Key(request, s)).ToArray();
        var result = (RedisResult[])(await scope.Database.ScriptEvaluateAsync(observation, keys, [], CommandFlags.DemandMaster))!;
        Assert.Equal(keys.Length + 1, result.Length);
        var nowUs = (long)result[0];
        var buckets = new SortedSetEntry[keys.Length][];
        for (var i = 0; i < keys.Length; i++)
        {
            var row = (RedisResult[])result[i + 1]!;
            Assert.Equal(0, row.Length % 2);
            buckets[i] = Enumerable.Range(0, row.Length / 2).Select(j =>
                new SortedSetEntry((string)row[2 * j]!, (double)row[2 * j + 1])).ToArray();
            Assert.True(buckets[i].All(entry => entry.Score <= nowUs), "El bucket observado contiene un evento futuro.");
            var windowUs = RedisAuthenticationQuotaConfiguration.WindowMicroseconds(request.Scopes[i].Definition.Window);
            var live = Live(buckets[i], nowUs, windowUs).Length;
            var expired = buckets[i].Count(entry => entry.Score <= nowUs - windowUs);
            var scores = string.Join(",", buckets[i].Select(entry => entry.Score.ToString("F0", CultureInfo.InvariantCulture)));
            // Solo tiempo, scope cerrado y conteos: no keys, OpaqueIds ni IDs internos de intento.
            output.WriteLine($"{phase} SCOPE={request.Scopes[i].ScopeType} NOW_US={nowUs} WINDOW_US={windowUs} RAW={buckets[i].Length} LIVE={live} EXPIRED={expired} SCORES_US=[{scores}]");
        }
        return (nowUs, buckets);
    }
}
