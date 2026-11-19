using System.Security.Cryptography;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class DistributedQuotaPressureTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public Task P1_exact_academic_windows_3000_attempts_paced_over_90_seconds() =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            await s.MeasureAsync("P1-before");
            for (var cycle = 0; cycle < 3; cycle++)
            {
                var counts = new int[3];
                await s.PaceAsync(1000, TimeSpan.FromSeconds(30), async (i, host) =>
                {
                    var slot = i % 20;
                    var operation = slot < 6 ? AuthenticationOperation.Register : slot < 13 ? AuthenticationOperation.Login : AuthenticationOperation.Logout;
                    Interlocked.Increment(ref counts[(int)operation]);
                    var email = (i * 13 + cycle * 31) % 128 + 1;
                    var origin = operation == AuthenticationOperation.Login ? (email * 2 + i / 20 % 2) % 16 + 1 : (i + cycle) % 16 + 1;
                    var request = s.Request(host, operation, email, origin, (i + cycle * 17) % 64 + 1);
                    var result = await s.SendAsync(host, request, $"P1-{cycle + 1}");
                    Assert.Contains(result.Status, new[] { AuthenticationQuotaStoreStatus.Admitted, AuthenticationQuotaStoreStatus.Rejected });
                    await s.AssertAttemptAsync(request, result.Status == AuthenticationQuotaStoreStatus.Admitted);
                });
                Assert.Equal(new[] { 300, 350, 350 }, counts);
                var m = await s.MeasureAsync($"P1-cycle-{cycle + 1}");
                Assert.InRange(m.Accounted, 1, 624);
            }
            Assert.Equal(3000, s.A.Dispatches + s.B.Dispatches);
        }, budget: TimeSpan.FromSeconds(180));

    [Fact]
    public Task P2_mixed_5000_partitions_102000_events_memory_and_revocations_during_pressure() =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            await s.MeasureAsync("P2-before");
            var rows = new Dictionary<string, (string Member, AuthenticationQuotaScope Scope, List<string> Attempts)>();
            AuthenticationQuotaConsumeRequest? registerSpare = null, loginSpare = null, logoutSpare = null;
            void Add(AuthenticationQuotaConsumeRequest request)
            {
                foreach (var scope in request.Scopes)
                {
                    var key = s.Data.Key(request, scope);
                    if (!rows.TryGetValue(key, out var row)) rows.Add(key, row = (s.Data.Member(request, scope), scope, []));
                    row.Attempts.Add(request.InternalAttemptId);
                }
            }
            for (var email = 0; email < 1500; email++)
                for (var e = 0; e < 3; e++)
                { var r = s.Request(0, email: email + 1, origin: (email * 3 + e) % 225 + 1); Add(r); registerSpare = r; }
            for (var email = 0; email < 600; email++)
                for (var e = 0; e < 10; e++)
                { var r = s.Request(0, AuthenticationOperation.Login, email + 1, (email * 2 + e % 2) % 100 + 1); Add(r); loginSpare = r; }
            for (var account = 0; account < 1250; account++)
                for (var e = 0; e < 30; e++)
                { var r = s.Request(0, AuthenticationOperation.Logout, origin: (account * 30 + e) % 125 + 1, account: account + 1); Add(r); logoutSpare = r; }
            Assert.Equal(5000, rows.Count); Assert.Equal(102000, rows.Values.Sum(r => r.Attempts.Count));
            foreach (var spare in new[] { registerSpare!, loginSpare!, logoutSpare! })
                foreach (var scope in spare.Scopes) Assert.True(rows[s.Data.Key(spare, scope)].Attempts.Remove(spare.InternalAttemptId));
            var seedTime = await s.Data.NowAsync();
            foreach (var batch in rows.Chunk(64))
                await Task.WhenAll(batch.Select(row => s.Data.SeedBucketAsync(row.Key, row.Value.Member,
                    row.Value.Attempts.Select(id => new SortedSetEntry(id, seedTime)).ToArray(),
                    seedTime + RedisAuthenticationQuotaConfiguration.WindowMicroseconds(row.Value.Scope.Definition.Window))));
            await s.Data.SynchronizeMetadataAsync();
            var initial = await s.MeasureAsync("P2-dense-synthetic");
            Assert.Equal(5000, initial.Accounted); Assert.Equal(102000 - 7, initial.Events); Assert.Equal(5000, initial.Active);
            Assert.True(await s.Data.NowAsync() - seedTime < 55_000_000, "Preparación demasiado lenta: densidad de 60s inconclusa.");
            // Ámbitos existentes al máximo de capacidad siguen siendo evaluables; cada intento escribe en todos sus ámbitos.
            foreach (var spare in new[] { registerSpare!, loginSpare!, logoutSpare! })
                Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(1, spare, "P2-spare-at-capacity")).Status);
            var withSpares = await s.MeasureAsync("P2-dense-with-existing-admissions");
            Assert.Equal(102000, withSpares.Events); Assert.Equal(5000, withSpares.Accounted);
            output.WriteLine("P2_RESERVED_FOR_SPARES=7 SEEDED_EVENTS=101993 PEAK_EVENTS=102000");
            var witness = $"enver:{s.Config.Snapshot.Environment}:test-witness";
            s.TrackExtra(witness); await s.Db.StringSetAsync(witness, "preserve");
            var longs = new List<(string Jti, string Key, DateTime Expiry)>();
            var shorts = new List<string>();
            for (var cycle = 0; cycle < 3; cycle++)
            {
                var c = cycle;
                var writer = c % 2 == 0 ? s.A : s.B; var reader = c % 2 == 0 ? s.B : s.A;
                var writes = new RedisTokenRevocationStore(writer.Connections, writer.Guard, writer.RedisOptions);
                var reads = new RedisTokenRevocationStore(reader.Connections, reader.Guard, reader.RedisOptions);
                var traffic = s.PaceAsync(256, TimeSpan.FromSeconds(20), async (i, host) =>
                {
                    var op = (AuthenticationOperation)(i % 3);
                    var r = s.Request(host, op, email: i % (op == AuthenticationOperation.Register ? 1500 : 600) + 1,
                        origin: op == AuthenticationOperation.Login ? (i % 600 * 2 + i % 2) % 100 + 1 : i % (op == AuthenticationOperation.Register ? 225 : 125) + 1,
                        account: i % 1250 + 1);
                    var result = await s.SendAsync(host, r, $"P2-{c + 1}");
                    Assert.Contains(result.Status, new[] { AuthenticationQuotaStoreStatus.Admitted, AuthenticationQuotaStoreStatus.Rejected });
                });
                var revocations = Task.Run(async () =>
                {
                    for (var i = 0; i < 256; i++)
                    {
                        // JTI sintético conforme al contrato: 32 bytes aleatorios en Base64url sin padding (43 caracteres).
                        var jti = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
                        var key = RedisKeyBuilder.BuildRevocationKey(s.Config.Snapshot.Environment, "enver-api", jti);
                        s.TrackExtra(key);
                        var expiry = DateTimeOffset.UtcNow.AddSeconds(i < 128 ? 180 : 5);
                        using (var b = RedisAuthenticationQuotaTestScope.Budget())
                            Assert.Equal(TokenRevocationWriteStatus.Confirmed, (await writes.RevokeAsync(new("enver-api", jti, expiry, TimeSpan.Zero, TimeSpan.Zero), b)).Status);
                        using (var b = RedisAuthenticationQuotaTestScope.Budget())
                            Assert.Equal(TokenRevocationReadStatus.Revoked, (await reads.CheckAsync(new("enver-api", jti), b)).Status);
                        if (i < 128)
                        {
                            var retained = (await s.Db.KeyExpireTimeAsync(key))!.Value;
                            using var b = RedisAuthenticationQuotaTestScope.Budget();
                            Assert.Equal(TokenRevocationWriteStatus.Confirmed, (await writes.RevokeAsync(new("enver-api", jti, expiry.AddSeconds(-20), TimeSpan.Zero, TimeSpan.Zero), b)).Status);
                            Assert.Equal(retained, await s.Db.KeyExpireTimeAsync(key)); longs.Add((jti, key, retained));
                        }
                        else shorts.Add(key);
                    }
                });
                // Tous les workers sont joints même si l'une des activités échoue.
                await Task.WhenAll(traffic, revocations);
                Assert.Equal("preserve", (await s.Db.StringGetAsync(witness)).ToString());
                foreach (var key in shorts) Assert.False(await s.Db.KeyExistsAsync(key));
                foreach (var mark in longs) Assert.Equal(mark.Expiry, await s.Db.KeyExpireTimeAsync(mark.Key));
                await s.MeasureAsync($"P2-cycle-{cycle + 1}");
                output.WriteLine($"P2 REVOCATIONS_CREATED={(cycle + 1) * 256} LONG_LIVE={longs.Count} SHORT_EXPIRED={shorts.Count}");
            }
            await s.Data.WaitUntilAsync(seedTime + 60_000_001, TimeSpan.FromSeconds(10));
            var housekeeping = s.Request(0, AuthenticationOperation.Login, email: 1, origin: 1);
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(0, housekeeping, "P2-housekeeping")).Status);
            await s.MeasureAsync("P2-after-expiry-housekeeping");
            long revocationBytes = 0;
            foreach (var mark in longs)
            {
                Assert.Equal(mark.Expiry, await s.Db.KeyExpireTimeAsync(mark.Key));
                revocationBytes += (long)await s.Db.ExecuteAsync("MEMORY", "USAGE", mark.Key, "SAMPLES", "0");
            }
            output.WriteLine($"P2 REVOCATION_BYTES_SEPARATE={revocationBytes} LONG_COUNT={longs.Count} SHORT_EXPIRED={shorts.Count}");
            await s.CleanupQuotaAsync();
            var after = await s.MeasureAsync("P2-after-selective-cleanup"); Assert.Equal(0, after.Bytes);
            var finalReader = new RedisTokenRevocationStore(s.B.Connections, s.B.Guard, s.B.RedisOptions);
            foreach (var mark in longs)
            {
                using var b = RedisAuthenticationQuotaTestScope.Budget();
                Assert.Equal(TokenRevocationReadStatus.Revoked, (await finalReader.CheckAsync(new("enver-api", mark.Jti), b)).Status);
                Assert.Equal(mark.Expiry, await s.Db.KeyExpireTimeAsync(mark.Key));
            }
            Assert.Equal("preserve", (await s.Db.StringGetAsync(witness)).ToString());
            var policy = (RedisResult[])(await s.Db.ExecuteAsync("CONFIG", "GET", "maxmemory-policy"))!;
            Assert.Equal("noeviction", policy[1].ToString());
        }, budget: TimeSpan.FromSeconds(300));

    [Fact]
    public Task Capacity_boundary_race_release_and_three_reassignment_rounds_never_exceed_5000() =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            // Ventanas sintéticas de 10 s SOLO en este caso; no se mezclan con la matriz académica.
            // Tres rondas: crecer hasta 5000, presionar la frontera y reasignar tras el vencimiento real.
            for (var round = 0; round < 3; round++)
            {
                var start = round * 100000 + 1;
                var initialRequest = s.Request(0, email: start, origin: start);
                Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(0, initialRequest, $"capacity-start-{round + 1}")).Status);
                var now = await s.Data.NowAsync();
                // Faltante real hasta 4998: las particiones reasignadas de la ronda anterior siguen vivas.
                var accountedBefore = await s.Data.CountAsync();
                var missing = 4998 - accountedBefore;
                Assert.InRange(missing, 0, 4998);
                Assert.Equal(0, missing % 2);
                var requests = Enumerable.Range(start + 1, (int)(missing / 2)).Select(i => s.Request(i % 2, email: i, origin: i)).ToArray();
                foreach (var batch in requests.Chunk(32))
                    await Task.WhenAll(batch.SelectMany(r => r.Scopes.Select(scope => s.Data.SeedBucketAsync(s.Data.Key(r, scope), s.Data.Member(r, scope),
                        [new SortedSetEntry(r.InternalAttemptId, now)], now + 10_000_000))));
                await s.Data.SynchronizeMetadataAsync();
                var accountedSeeded = await s.Data.CountAsync();
                output.WriteLine($"CAPACITY_PREPARE_ROUND={round + 1} ACCOUNTED_BEFORE={accountedBefore} MISSING={missing} SEED_REQUESTS={requests.Length} ACCOUNTED_SEEDED={accountedSeeded}");
                Assert.Equal(4998, accountedSeeded);
                Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(1, s.Request(1, email: start + 3000, origin: start), $"capacity-one-new-{round + 1}")).Status);
                Assert.Equal(4999, await s.Data.CountAsync());
                var race = await Task.WhenAll(Enumerable.Range(0, 16).Select(async i =>
                {
                    var r = s.Request(i % 2, email: start + 4000 + i, origin: start);
                    var result = await s.SendAsync(i % 2, r, $"capacity-race-{round + 1}");
                    Assert.InRange(await s.Data.CountAsync(), 4999, 5000);
                    await s.AssertAttemptAsync(r, result.Status == AuthenticationQuotaStoreStatus.Admitted);
                    return result.Status;
                }));
                Assert.Single(race, r => r == AuthenticationQuotaStoreStatus.Admitted);
                Assert.Equal(15, race.Count(r => r == AuthenticationQuotaStoreStatus.Unavailable));
                var blocked = s.Request(0, email: start + 5000, origin: start + 5000);
                Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await s.SendAsync(0, blocked, $"capacity-two-new-{round + 1}")).Status);
                await s.AssertAttemptAsync(blocked, false);
                Assert.Equal(5000, await s.Data.CountAsync());
                // Ámbitos existentes siguen siendo evaluables con la ocupación al máximo.
                Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(1, s.Request(1, email: start, origin: start), $"capacity-existing-{round + 1}")).Status);
                Assert.Equal(5000, await s.Data.CountAsync());
                var boundaryAccounted = await s.Data.CountAsync();
                var raceAdmitted = race.Count(r => r == AuthenticationQuotaStoreStatus.Admitted);
                var raceUnavailable = race.Count(r => r == AuthenticationQuotaStoreStatus.Unavailable);
                output.WriteLine($"CAPACITY_ROUND={round + 1} MAX_ACCOUNTED={boundaryAccounted} RACE_ADMITTED={raceAdmitted} RACE_UNAVAILABLE={raceUnavailable} TWO_NEW=Unavailable EXISTING=Admitted");
                if (round == 0)
                {
                    // Medición de memoria en la frontera de capacidad; las rondas siguientes reasignan.
                    var peak = await s.MeasureAsync("capacity-at-5000");
                    Assert.Equal(5000, peak.Accounted);
                }
                // Vencimiento real del horizonte máximo: registro y buckets quedan fuera; la ocupación se reasigna.
                var horizon = (long)await s.Db.HashGetAsync(s.Config.Keys.Metadata, "horizon");
                await s.Data.WaitUntilAsync(horizon + 1, TimeSpan.FromSeconds(20));
                var reassigned = s.Request(1, email: start + 50000, origin: start + 50000);
                Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(1, reassigned, $"capacity-post-expiry-{round + 1}")).Status);
                var accounted = await s.Data.CountAsync();
                output.WriteLine($"CAPACITY_POST_EXPIRY_ROUND={round + 1} ACCOUNTED={accounted} REASSIGNED=Admitted");
                Assert.Equal(2, accounted);
                Assert.InRange(accounted, 1, 5000);
            }
        }, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(240));

    [Fact]
    public Task Lazy_cleanup_revalidates_renewed_hint_and_releases_only_three_residues_during_other_host_consumption() =>
        DistributedQuotaTestScope.RunAsync(fixture, output, async s =>
        {
            var first = s.Request(0); Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(0, first)).Status);
            var member = s.Data.Member(first, first.Scopes[0]);
            var custom = new RedisAuthenticationQuotaStore(s.A.Connections, s.A.RedisOptions, s.Config, s.A.Guard, RedisAuthenticationQuotaTestScope.Open,
                hintsOverride: async () =>
                {
                    Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(1, s.Request(1), "renew-candidate")).Status);
                    return [member];
                });
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(0, s.Request(0, email: 2, origin: 2), "stale-hint", custom)).Status);
            Assert.Equal(2, await s.Db.SortedSetLengthAsync(s.Data.Key(first, first.Scopes[0])));
            var residues = new List<string>();
            for (var i = 10; i < 15; i++)
            {
                var r = s.Request(0, email: i, origin: i); var scope = r.Scopes[0];
                var key = s.Data.Key(r, scope); var id = s.Data.Member(r, scope); residues.Add(id);
                var past = await s.Data.NowAsync() - 1_000_000;
                await s.Db.SortedSetAddAsync(key, Guid.NewGuid().ToString("N"), past - 900_000_000);
                await s.Db.KeyExpireAsync(key, TimeSpan.FromMinutes(1));
                await s.Db.SortedSetAddAsync(s.Config.Keys.Registry, id, past);
            }
            await s.Data.SynchronizeMetadataAsync(); Assert.Equal(9, await s.Data.CountAsync());
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(1, s.Request(1, email: 3, origin: 3), "cleanup-batch")).Status);
            Assert.Equal(8, await s.Data.CountAsync()); // -3 residues +2 scopes.
            Assert.Equal(2, (await Task.WhenAll(residues.Select(m => s.Db.SortedSetScoreAsync(s.Config.Keys.Registry, m)))).Count(v => v is not null));
            Assert.Equal(AuthenticationQuotaStoreStatus.Admitted, (await s.SendAsync(0, s.Request(0, email: 4, origin: 4), "cleanup-rest")).Status);
            Assert.Equal(8, await s.Data.CountAsync());
            foreach (var id in residues) Assert.Null(await s.Db.SortedSetScoreAsync(s.Config.Keys.Registry, id));
            Assert.Equal(2, await s.Db.SortedSetLengthAsync(s.Data.Key(first, first.Scopes[0])));
            output.WriteLine("LAZY_RELEASES=5 MAX_CANDIDATES_PER_EVAL=3 RENEWED_CANDIDATE_PRESERVED=true");
        }, budget: TimeSpan.FromSeconds(90));
}
