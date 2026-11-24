using System.Collections.Concurrent;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.Testing;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class DistributedQuotaTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    [Theory]
    [InlineData(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Email, 3)]
    [InlineData(AuthenticationOperation.Register, AuthenticationQuotaScopeType.Origin, 20)]
    [InlineData(AuthenticationOperation.Login, AuthenticationQuotaScopeType.OriginEmail, 5)]
    [InlineData(AuthenticationOperation.Login, AuthenticationQuotaScopeType.Email, 10)]
    [InlineData(AuthenticationOperation.Login, AuthenticationQuotaScopeType.Origin, 60)]
    [InlineData(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Account, 30)]
    [InlineData(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Origin, 300)]
    public Task Seven_global_limits_alternate_hosts_and_race_with_known_excess(AuthenticationOperation operation, AuthenticationQuotaScopeType target, int limit) =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            Assert.Same(AuthenticationQuotaPolicyDescriptor.AcademicInitial, s.Config.Snapshot.Policy);
            AuthenticationQuotaConsumeRequest Request(int i) => s.Request(i % 2, operation,
                email: target == AuthenticationQuotaScopeType.Origin ? i + 1 : 1,
                origin: operation == AuthenticationOperation.Login && target == AuthenticationQuotaScopeType.Email ? i + 1 : 1,
                account: target == AuthenticationQuotaScopeType.Origin ? i + 1 : 1);
            var results = new ConcurrentBag<(AuthenticationQuotaConsumeRequest Request, AuthenticationQuotaConsumeResult Result)>();
            for (var i = 0; i < 2; i++)
            {
                var request = Request(i); var result = await s.SendAsync(i % 2, request);
                Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, result.Status); results.Add((request, result));
            }
            await Parallel.ForEachAsync(Enumerable.Range(2, limit + 8), new ParallelOptions { MaxDegreeOfParallelism = 8 }, async (i, _) =>
            { var request = Request(i); results.Add((request, await s.SendAsync(i % 2, request))); });
            Assert.Equal(limit, results.Count(r => r.Result.Status == AuthenticationQuotaStoreStatus.Admitted));
            Assert.Equal(10, results.Count(r => r.Result.Status == AuthenticationQuotaStoreStatus.Rejected));
            Assert.All(results, r => Assert.Contains(r.Result.Status, new[] { AuthenticationQuotaStoreStatus.Admitted, AuthenticationQuotaStoreStatus.Rejected }));
            foreach (var row in results) await s.AssertAttemptAsync(row.Request, row.Result.Status == AuthenticationQuotaStoreStatus.Admitted);
            var rejected = results.First(r => r.Result.Status == AuthenticationQuotaStoreStatus.Rejected).Request;
            var expiry = await Task.WhenAll(rejected.Scopes.Select(scope => s.Db.KeyExpireTimeAsync(s.Data.Key(rejected, scope))));
            Assert.Contains(expiry, value => value is not null);
            for (var n = 0; n < 4; n++)
                Assert.Equal(AuthenticationQuotaStoreStatus.Rejected, (await s.SendAsync(n % 2, Request(limit + 100 + n))).Status);
            Assert.Equal(expiry, await Task.WhenAll(rejected.Scopes.Select(scope => s.Db.KeyExpireTimeAsync(s.Data.Key(rejected, scope)))));
            await s.MeasureAsync("matrix-final");
        });

    [Fact]
    public Task Maximum_blocker_retry_rounds_up_and_synthetic_boundary_releases_without_penalty_extension() =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            var request = s.Request(0);
            s.Diagnostics?.Phase("blocker-rejection", 1);
            s.Timing?.Phase("blocker-rejection");
            s.Diagnostics?.Request(request);
            var now = await s.Data.NowAsync();
            long maximumHorizon = 0;
            foreach (var scope in request.Scopes)
            {
                // Préparation de frontière synthétique, pas 15 minutes de trafic réel.
                var remaining = scope.ScopeType == AuthenticationQuotaScopeType.Email ? 600_100_000L : 800_700_000L;
                var timestamp = now - 900_000_000L + remaining;
                s.Diagnostics?.Seed(scope.ScopeType, now, timestamp, timestamp + 900_000_000L);
                await s.Data.SeedAsync(request, scope, Enumerable.Repeat(timestamp, scope.Definition.PermitLimit));
                maximumHorizon = Math.Max(maximumHorizon, timestamp + 900_000_000L);
            }
            var invocation = Enver.API.Infrastructure.Redis.RedisAuthenticationQuotaScriptProtocol.Encode(s.Config, "evaluate", request);
            var previousLast = (long)await s.Db.HashGetAsync(s.Config.Keys.Metadata, "last");
            var reply = Enver.API.Infrastructure.Redis.RedisAuthenticationQuotaScriptProtocol.Parse(await s.Db.ScriptEvaluateAsync(
                Enver.API.Infrastructure.Redis.RedisAuthenticationQuotaScriptProtocol.ScriptText, invocation.Keys, invocation.Arguments));
            Assert.Equal("rejected", reply.Status);
            var logicalRetry = (maximumHorizon - reply.NowMicroseconds + 999999) / 1000000;
            s.Diagnostics?.Retry(maximumHorizon, previousLast, reply.NowMicroseconds, reply.RetryAfterSeconds);
            if (reply.NowMicroseconds > previousLast) Assert.Equal(logicalRetry, reply.RetryAfterSeconds);
            else Assert.True(reply.RetryAfterSeconds >= logicalRetry);
            await s.AssertAttemptAsync(request, false);
            // El mismo intento, ya rechazado, no se insertó; el horizonte bloqueante se prepara vencido
            // de forma sintética para comprobar que el rechazo no impone una penalización perpetua.
            s.Diagnostics?.Phase("boundary-release", 1);
            s.Timing?.Phase("boundary-release");
            foreach (var scope in request.Scopes)
            {
                await s.Db.KeyDeleteAsync(s.Data.Key(request, scope));
                var boundaryNow = await s.Data.NowAsync();
                var timestamp = boundaryNow - 900_000_001L;
                s.Diagnostics?.Seed(scope.ScopeType, boundaryNow, timestamp, timestamp + 900_000_000L);
                await s.Data.SeedAsync(request, scope, [timestamp]);
            }
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(1, request)).Status);
            await s.AssertAttemptAsync(request, true);
        }, diagnostics: true, timeObservability: true);

    [Fact]
    public Task Recreating_API_host_preserves_shared_buckets_revision_and_HMAC_identity() =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            var a = s.Request(0); var b = s.Request(1);
            Assert.Equal(a.Scopes.Select(r => r.OpaqueId), b.Scopes.Select(r => r.OpaqueId));
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(0, a)).Status);
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(1, b)).Status);
            var old = s.A.Services;
            await s.RestartHostAAsync(); Assert.NotSame(old, s.A.Services);
            var restarted = s.Request(0);
            Assert.Equal(a.Scopes.Select(r => r.OpaqueId), restarted.Scopes.Select(r => r.OpaqueId));
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(0, restarted)).Status);
            Assert.Equal(AuthenticationQuotaStoreStatus.Rejected, (await s.SendAsync(1, s.Request(1))).Status);
            Assert.Equal(2, await s.Data.CountAsync());
            foreach (var scope in a.Scopes)
            {
                var key = s.Data.Key(a, scope);
                Assert.DoesNotContain(s.Config.Snapshot.Policy.Revision, key);
                Assert.Equal(3, await s.Db.SortedSetLengthAsync(key));
            }
        });

    [Fact]
    public Task Failed_functional_continuations_keep_consumption_and_stop_after_global_limit() =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            var continued = 0;
            for (var i = 0; i < 8; i++)
            {
                var request = s.Request(i % 2, AuthenticationOperation.Login);
                var result = await s.SendAsync(i % 2, request, "failed-functional");
                if (result.Status == AuthenticationQuotaStoreStatus.Admitted)
                {
                    continued++;
                    // Continuation synthétique: aucun hasher, user lookup, HTTP ou remboursement.
                    var failure = new[] { "incorrect-password", "nonexistent-account", "functional-failure" }[i % 3];
                    Assert.NotEmpty(failure);
                }
                else Assert.Equal(AuthenticationQuotaStoreStatus.Rejected, result.Status);
                await s.AssertAttemptAsync(request, i < 5);
            }
            Assert.Equal(5, continued);
            Assert.Equal(8, s.A.Dispatches + s.B.Dispatches);
            var sample = s.Request(0, AuthenticationOperation.Login);
            foreach (var scope in sample.Scopes) Assert.Equal(5, await s.Db.SortedSetLengthAsync(s.Data.Key(sample, scope)));
        });
}
