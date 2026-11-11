using Enver.API.Infrastructure.Redis;
using StackExchange.Redis;

namespace Enver.API.UnitTests.RedisQuotas;

// Contrato de fuente y propiedades de referencia; la ejecución del Lua se prueba en IT.
public sealed class RedisQuotaLogicalClockTests
{
    [Theory]
    [InlineData(1794425930038134L,1794425930038133L,1794425930038134L)]
    [InlineData(1794425930038133L,1794425930038133L,1794425930038133L)]
    [InlineData(1794425930038132L,1794425930038133L,1794425930038133L)]
    [InlineData(1794425930030246L,1794425930038133L,1794425930038133L)]
    [InlineData(1793821130038133L,1794425930038133L,1794425930038133L)]
    public void Source_clamps_validated_wall_time_and_reference_never_moves_back(long redisNow,long last,long expected)
    {
        var script=RedisAuthenticationQuotaScriptProtocol.ScriptText;
        Assert.Contains("local now = meta and math.max(redisNow, meta.last) or redisNow",script);
        Assert.DoesNotContain("now < meta.last",script);
        Assert.Equal(expected,Math.Max(redisNow,last));
        Assert.True(expected>=last);
        Assert.Contains("'last', number(now)",script);
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(7887L)]
    [InlineData(604800000000L)]
    public void Reference_rollback_preserves_horizons_and_increases_wall_wait_without_reopening_quota(long rollback)
    {
        const long last=1794425930038133L,window=900000000;
        var redisNow=last-rollback;
        var effectiveNow=Math.Max(redisNow,last);
        var priorHorizon=last+window;
        Assert.Equal(priorHorizon,effectiveNow+window);
        Assert.True(last>effectiveNow-window); // El evento continúa dentro de ventana.
        Assert.True((priorHorizon+999)/1000 >= (last+window+999)/1000);
        Assert.Equal(window+rollback,priorHorizon-redisNow);
        Assert.True((priorHorizon-redisNow+999999)/1000000 >= window/1000000);
    }

    [Fact]
    public void Source_preserves_one_TIME_bounds_validation_before_mutation_and_wall_delay_conversion()
    {
        var script=RedisAuthenticationQuotaScriptProtocol.ScriptText;
        Assert.Equal(1,script.Split("redis.call('TIME')").Length-1);
        Assert.Contains("if redisNow < 0 or redisNow > MAX then return unavailable('state') end",script);
        Assert.Contains("if maximumWindow > MAX - now then return unavailable('state') end",script);
        Assert.Contains("tonumber(relevant[2]) + window - redisNow",script);
        Assert.Contains("count - #releases + newCount > 5000",script);
        Assert.Contains("'PEXPIREAT', item.key, math.ceil((now + item.window) / 1000)",script);
        Assert.True(script.IndexOf("math.max(redisNow, meta.last)",StringComparison.Ordinal)<script.IndexOf("-- FASE DE ESCRITURA",StringComparison.Ordinal));
    }

    [Fact]
    public void Six_field_protocol_accepts_effective_time_without_new_version_or_mapping()
    {
        const long effective=1794425930038133L;
        var raw=RedisResult.Create(new[]{"enver.auth.quota","1","admitted","ok","0",effective.ToString(System.Globalization.CultureInfo.InvariantCulture)}
            .Select(v=>RedisResult.Create((RedisValue)v)).ToArray());
        var parsed=RedisAuthenticationQuotaScriptProtocol.Parse(raw);
        Assert.True(parsed.IsSemantic);
        Assert.Equal("admitted",parsed.Status);
        Assert.Equal(effective,parsed.NowMicroseconds);
    }
}
