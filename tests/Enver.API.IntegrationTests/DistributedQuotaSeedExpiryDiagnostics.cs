using System.Text.Json;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

// Opt-in del boundary-release. Ningún cambio de valores, orden de mutaciones ni TTL.
// TIME delimita la ejecución: NO se presenta el instante interno de PEXPIREAT como exacto.
// PTTL/PEXPIRETIME pueden activar expiración diferida; un PASS no prueba reparación.
internal sealed class DistributedQuotaSeedExpiryDiagnostics
{
    internal sealed record Assignment(string Scope, string Kind, long PExpireAt,
        long RedisBeforeUs, long RedisAfterUs, long Result);
    internal sealed record Sample(string Scope, long SeedRedisNow, long RedisBeforeUs,
        long RegistryPttl, long RegistryPExpireTime, long BucketPttl, long BucketPExpireTime,
        long RedisAfterUs);

    private readonly List<Assignment> assignments = new(4);
    private readonly List<Sample> afterSeed = new(2);
    private string scope = "UNKNOWN";
    private long seedRedisNow;
    private Sample? preEval;
    private long? luaNow;
    private string luaBranch = "NOT_OBSERVABLE";

    internal void Begin(string scopeName, long redisNow)
    { scope = scopeName; seedRedisNow = redisNow; }

    internal async Task ExpireAsync(IDatabase database, RedisKey key, string kind, long expiry)
    {
        // Pipeline, no MULTI ni Lua de sustitución: el MISMO PEXPIREAT entre dos TIME.
        // Las mutaciones posteriores del seed siguen esperando la misma respuesta.
        var batch = database.CreateBatch();
        var before = batch.ExecuteAsync("TIME");
        var applied = batch.ExecuteAsync("PEXPIREAT", key, expiry);
        var after = batch.ExecuteAsync("TIME");
        batch.Execute();
        await Task.WhenAll(before, applied, after);
        if (assignments.Count < 4)
            assignments.Add(new(scope, kind, expiry, Time(await before), Time(await after), (long)await applied));
    }

    internal async Task AfterSeedAsync(IDatabase database, RedisKey registry, RedisKey bucket)
    {
        var sample = await CaptureAsync(database, registry, bucket, scope, seedRedisNow);
        if (afterSeed.Count < 2) afterSeed.Add(sample);
    }

    internal async Task BeforeEvalAsync(IDatabase database, RedisKey registry, RedisKey origin)
        => preEval = await CaptureAsync(database, registry, origin, "Origin", seedRedisNow);

    private static async Task<Sample> CaptureAsync(IDatabase database, RedisKey registry,
        RedisKey bucket, string scope, long seedRedisNow)
    {
        // Lecturas no atómicas, ordenadas en una conexión; cada grupo queda delimitado.
        // Solo posiciones/etiquetas se exportan; jamás keys, members ni argumentos sensibles.
        var batch = database.CreateBatch();
        var before = batch.ExecuteAsync("TIME");
        var registryPttl = batch.ExecuteAsync("PTTL", registry);
        var registryExpiry = batch.ExecuteAsync("PEXPIRETIME", registry);
        var bucketPttl = batch.ExecuteAsync("PTTL", bucket);
        var bucketExpiry = batch.ExecuteAsync("PEXPIRETIME", bucket);
        var after = batch.ExecuteAsync("TIME");
        batch.Execute();
        await Task.WhenAll(before, registryPttl, registryExpiry, bucketPttl, bucketExpiry, after);
        return new(scope, seedRedisNow, Time(await before), (long)await registryPttl,
            (long)await registryExpiry, (long)await bucketPttl, (long)await bucketExpiry, Time(await after));
    }

    private static long Time(RedisResult result)
    {
        var values = (RedisResult[])result!;
        return checked((long)values[0] * 1000000 + (long)values[1]);
    }

    internal void Returned(string branch, long now) { luaBranch = branch; luaNow = now; }

    internal JsonElement Evidence() => JsonSerializer.SerializeToElement(new
    {
        schema = 1, nonAtomic = true, expiryReadsMayTriggerLazyExpiration = true,
        executionTime = "REDIS_TIME_BRACKET_NOT_EXACT_INSTANT",
        assignments, afterSeed, preEval, luaNow, luaBranch
    });
}
