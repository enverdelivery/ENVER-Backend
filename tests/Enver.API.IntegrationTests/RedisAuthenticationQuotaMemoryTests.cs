using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisAuthenticationQuotaMemoryTests(AuthenticationDependenciesFixture fixture, ITestOutputHelper output)
{
    [Fact]
    public async Task Dense_5000_partition_diagnostic_measures_budget_and_preserves_real_revocations()
    {
        // Diagnóstico sintético de huella, NO benchmark: mismas siete cotas, ventana ampliada solo en test
        // para que la preparación local no expire. 4545 cuentas x 30 = 136350 admisiones posibles;
        // 455 orígenes reciben <=300 cada uno. Cada ID aparece en exactamente cuenta+origen.
        await using var s = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture, TimeSpan.FromMinutes(15));
        var before = (await s.Database.ExecuteAsync("INFO", "MEMORY")).ToString();
        Assert.Contains("used_memory:", before);
        var jti = RedisAuthenticationQuotaTestScope.Opaque(-1);
        var revocationKey = RedisKeyBuilder.BuildRevocationKey(s.Configuration.Snapshot.Environment, "enver-api", jti);
        s.Track(revocationKey);
        var revocations = new RedisTokenRevocationStore(s.Connections, RedisAuthenticationQuotaTestScope.Open, s.Options);
        using (var budget = RedisAuthenticationQuotaTestScope.Budget())
            Assert.Equal(TokenRevocationWriteStatus.Confirmed, (await revocations.RevokeAsync(new("enver-api", jti,
                DateTimeOffset.UtcNow.AddMinutes(15), TimeSpan.Zero, TimeSpan.Zero), budget)).Status);
        var revocationExpiry = await s.Database.KeyExpireTimeAsync(revocationKey);
        var secondRevocations = new RedisTokenRevocationStore(s.SecondConnections, RedisAuthenticationQuotaTestScope.Open, s.Options);
        var now = await s.NowAsync();
        var origins = Enumerable.Range(0, 455).Select(_ => new List<SortedSetEntry>(300)).ToArray();
        var buckets = new List<(string Key, string Member, SortedSetEntry[] Events)>(5000);
        for (var account = 0; account < 4545; account++)
        {
            var events = new SortedSetEntry[30];
            for (var e = 0; e < 30; e++)
            {
                var ordinal = account * 30 + e;
                var item = new SortedSetEntry(Guid.NewGuid().ToString("N"), now);
                events[e] = item;
                origins[ordinal % 455].Add(item);
            }
            var id = RedisAuthenticationQuotaTestScope.Opaque(account);
            buckets.Add((s.Configuration.Keys.Bucket(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Account, id),
                RedisAuthenticationQuotaKeyBuilder.Member(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Account, id), events));
        }
        for (var origin = 0; origin < 455; origin++)
        {
            var id = RedisAuthenticationQuotaTestScope.Opaque(origin + 100000);
            Assert.InRange(origins[origin].Count, 1, 300);
            buckets.Add((s.Configuration.Keys.Bucket(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Origin, id),
                RedisAuthenticationQuotaKeyBuilder.Member(AuthenticationOperation.Logout, AuthenticationQuotaScopeType.Origin, id), origins[origin].ToArray()));
        }
        foreach (var batch in buckets.Chunk(32))
            await Task.WhenAll(batch.Select(b => s.SeedBucketAsync(b.Key, b.Member, b.Events, now + 900000000)));
        await s.SynchronizeMetadataAsync();
        Assert.Equal(5000, await s.CountAsync());
        long bucketBytes = 0;
        long eventCount = 0;
        foreach (var batch in buckets.Chunk(64))
        {
            var sizes = await Task.WhenAll(batch.Select(async b => (long)await s.Database.ExecuteAsync("MEMORY", "USAGE", b.Key, "SAMPLES", "0")));
            bucketBytes += sizes.Sum();
            eventCount += batch.Sum(b => (long)b.Events.Length);
        }
        var registryBytes = (long)await s.Database.ExecuteAsync("MEMORY", "USAGE", s.Configuration.Keys.Registry, "SAMPLES", "0");
        var metadataBytes = (long)await s.Database.ExecuteAsync("MEMORY", "USAGE", s.Configuration.Keys.Metadata, "SAMPLES", "0");
        var during = (await s.Database.ExecuteAsync("INFO", "MEMORY")).ToString();
        var accountEncoding = (await s.Database.ExecuteAsync("OBJECT", "ENCODING", buckets[0].Key)).ToString();
        var originEncoding = (await s.Database.ExecuteAsync("OBJECT", "ENCODING", buckets[^1].Key)).ToString();
        var attributed = bucketBytes + registryBytes + metadataBytes;
        output.WriteLine($"PARTITIONS=5000 EVENTS={eventCount} BUCKET_BYTES={bucketBytes} REGISTRY_BYTES={registryBytes} METADATA_BYTES={metadataBytes} ATTRIBUTED_BYTES={attributed} BUDGET_BYTES=67108864");
        output.WriteLine($"ACCOUNT_ENCODING={accountEncoding} ORIGIN_ENCODING={originEncoding}");
        output.WriteLine("INFO_BEFORE " + Summary(before));
        output.WriteLine("INFO_DURING " + Summary(during));
        Assert.Equal(272700, eventCount);
        Assert.InRange(attributed, 1, AuthenticationQuotaCapacityDescriptor.AuthenticationQuotaMemoryBudgetBytes);
        Assert.True(bucketBytes > 0 && registryBytes > 0 && metadataBytes > 0);
        using (var budget = RedisAuthenticationQuotaTestScope.Budget())
            Assert.Equal(TokenRevocationReadStatus.Revoked, (await secondRevocations.CheckAsync(new("enver-api", jti), budget)).Status);
        Assert.Equal(revocationExpiry, await s.Database.KeyExpireTimeAsync(revocationKey));
        Assert.StartsWith("enver:{", buckets[0].Key);
        Assert.DoesNotContain("auth:revoked", s.Configuration.Keys.Prefix);
        // Limpieza selectiva del diagnóstico, no reset productivo; revocación queda intacta.
        foreach (var batch in buckets.Select(b => (RedisKey)b.Key).Append(s.Configuration.Keys.Registry).Chunk(128)) await s.Database.KeyDeleteAsync(batch);
        var after = (await s.Database.ExecuteAsync("INFO", "MEMORY")).ToString();
        output.WriteLine("INFO_AFTER " + Summary(after));
        Assert.True(await s.Database.KeyExistsAsync(revocationKey));
        var config = (RedisResult[])(await s.Database.ExecuteAsync("CONFIG", "GET", "maxmemory-policy"))!;
        Assert.Equal("noeviction", config[1].ToString());
    }

    private static string Summary(string info) => string.Join(" ", info.Split('\n').Select(s => s.Trim()).Where(s =>
        s.StartsWith("used_memory:", StringComparison.Ordinal) || s.StartsWith("used_memory_peak:", StringComparison.Ordinal) ||
        s.StartsWith("used_memory_overhead:", StringComparison.Ordinal) || s.StartsWith("used_memory_dataset:", StringComparison.Ordinal)));
}
