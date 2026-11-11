using System.Globalization;
using Enver.API.Infrastructure.Redis;
using Enver.Testing;
using StackExchange.Redis;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class RedisAuthenticationQuotaClockTests(AuthenticationDependenciesFixture fixture)
{
    private const long Last=1794425930038133L;
    private const long Window=900000000;

    [Theory]
    [InlineData(1L)]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-7887L)]
    [InlineData(-604800000000L)]
    public async Task Actual_Lua_clamps_one_microsecond_historical_and_large_rollbacks(long relative)
    {
        await using var s=await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var clock=await Clock.CreateAsync(s);
        clock.Now=Last+relative;
        var request=s.Request();
        var result=await clock.SendAsync(request);
        var expected=Math.Max(clock.Now,Last);
        Assert.Equal("admitted",result.Status);
        Assert.Equal(expected,result.NowMicroseconds);
        Assert.Equal(expected,(long)await s.Database.HashGetAsync(s.Configuration.Keys.Metadata,"last"));
        Assert.Equal(expected+Window,(long)await s.Database.HashGetAsync(s.Configuration.Keys.Metadata,"horizon"));
        Assert.Equal(2,await s.CountAsync());
        foreach(var scope in request.Scopes)
        {
            var entries=await s.Database.SortedSetRangeByRankWithScoresAsync(s.Key(request,scope));
            Assert.Single(entries);
            Assert.Equal((double)expected,entries[0].Score);
            Assert.Equal(clock.PhysicalExpiry(expected+Window),await ExpiryAsync(s,s.Key(request,scope)));
        }
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(7887L)]
    [InlineData(604800000000L)]
    public async Task Rollback_never_shortens_TTL_horizon_or_RetryAfter_and_joint_rejection_never_mutates(long rollback)
    {
        await using var s=await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var clock=await Clock.CreateAsync(s);
        var first=s.Request();
        Assert.Equal("admitted",(await clock.SendAsync(first)).Status);
        clock.Now=Last+5000000;
        Assert.Equal("admitted",(await clock.SendAsync(s.Request())).Status);
        var priorLast=(long)await s.Database.HashGetAsync(s.Configuration.Keys.Metadata,"last");
        var priorHorizon=(long)await s.Database.HashGetAsync(s.Configuration.Keys.Metadata,"horizon");
        var keys=first.Scopes.Select(scope=>s.Key(first,scope)).Append(s.Configuration.Keys.Registry).ToArray();
        var expiries=await Task.WhenAll(keys.Select(key=>ExpiryAsync(s,key)));
        clock.Now=priorLast-rollback;
        var third=await clock.SendAsync(s.Request());
        Assert.Equal("admitted",third.Status);
        Assert.Equal(priorLast,third.NowMicroseconds);
        Assert.Equal(priorLast,(long)await s.Database.HashGetAsync(s.Configuration.Keys.Metadata,"last"));
        Assert.Equal(priorHorizon,(long)await s.Database.HashGetAsync(s.Configuration.Keys.Metadata,"horizon"));
        Assert.Equal(expiries,await Task.WhenAll(keys.Select(key=>ExpiryAsync(s,key))));
        var blocked=s.Request(identities:new Dictionary<Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaScopeType,int>
        {
            [Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaScopeType.Email]=10,
            [Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaScopeType.Origin]=999
        });
        var protectedKeys=keys.Append(s.Configuration.Keys.Metadata).Concat(blocked.Scopes.Select(scope=>s.Key(blocked,scope))).Distinct().ToArray();
        var before=await Task.WhenAll(protectedKeys.Select(key=>s.Database.KeyDumpAsync(key)));
        var rejection=await clock.SendAsync(blocked);
        Assert.Equal("rejected",rejection.Status);
        Assert.Equal("quota",rejection.Category);
        Assert.Equal((Last+Window-clock.Now+999999)/1000000,rejection.RetryAfterSeconds);
        Assert.True(rejection.RetryAfterSeconds>0);
        Assert.Equal(before,await Task.WhenAll(protectedKeys.Select(key=>s.Database.KeyDumpAsync(key))));
        Assert.Equal(expiries,await Task.WhenAll(keys.Select(key=>ExpiryAsync(s,key))));
        Assert.Equal(2,await s.CountAsync());
        foreach(var scope in first.Scopes) Assert.Equal(3,await s.Database.SortedSetLengthAsync(s.Key(first,scope)));
    }

    [Fact]
    public async Task Exact_capacity_and_concurrent_joint_admissions_survive_rollback_without_partial_creation()
    {
        await using var s=await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var clock=await Clock.CreateAsync(s);
        for(var i=1;i<=2499;i++) Assert.Equal("admitted",(await clock.SendAsync(s.Request(i))).Status);
        Assert.Equal(4998,await s.CountAsync());
        clock.Now=Last-7887;
        var requests=Enumerable.Range(3000,8).Select(i=>s.Request(i)).ToArray();
        var results=await Task.WhenAll(requests.Select(request=>clock.SendAsync(request)));
        Assert.Single(results,r=>r.Status=="admitted");
        Assert.Equal(7,results.Count(r=>r.Status=="unavailable" && r.Category=="capacity"));
        Assert.Equal(5000,await s.CountAsync());
        Assert.Equal(5000,await s.Database.SortedSetLengthAsync(s.Configuration.Keys.Registry));
        for(var i=0;i<requests.Length;i++)
            foreach(var scope in requests[i].Scopes)
                Assert.Equal(results[i].Status=="admitted",await s.Database.KeyExistsAsync(s.Key(requests[i],scope)));
    }

    [Theory]
    [InlineData("metadata-type")]
    [InlineData("registry-type")]
    [InlineData("bucket-type")]
    [InlineData("hlen")]
    [InlineData("metadata-ttl")]
    [InlineData("protocol")]
    [InlineData("snapshot")]
    [InlineData("generation")]
    [InlineData("state")]
    [InlineData("dirty")]
    [InlineData("count")]
    [InlineData("horizon")]
    [InlineData("last")]
    [InlineData("missing-member")]
    [InlineData("future-event")]
    [InlineData("overflow")]
    public async Task Structural_corruption_still_fails_closed_before_any_write(string mutation)
    {
        await using var s=await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var clock=await Clock.CreateAsync(s);
        var initial=s.Request();
        Assert.Equal("admitted",(await clock.SendAsync(initial)).Status);
        var bucket=s.Key(initial,initial.Scopes[0]);
        switch(mutation)
        {
            case "metadata-type": await s.Database.StringSetAsync(s.Configuration.Keys.Metadata,"invalid"); break;
            case "registry-type": await s.Database.StringSetAsync(s.Configuration.Keys.Registry,"invalid"); break;
            case "bucket-type": await s.Database.StringSetAsync(bucket,"invalid"); break;
            case "hlen": await s.Database.HashSetAsync(s.Configuration.Keys.Metadata,"extra","invalid"); break;
            case "metadata-ttl": await s.Database.KeyExpireAsync(s.Configuration.Keys.Metadata,TimeSpan.FromMinutes(1)); break;
            case "protocol": case "snapshot": case "generation": case "state": case "dirty":
                await s.Database.HashSetAsync(s.Configuration.Keys.Metadata,mutation,"invalid"); break;
            case "count": await s.Database.HashSetAsync(s.Configuration.Keys.Metadata,"count",3); break;
            case "horizon": await s.Database.HashSetAsync(s.Configuration.Keys.Metadata,"horizon",Last+Window+1); break;
            case "last": await s.Database.HashSetAsync(s.Configuration.Keys.Metadata,"last","-1"); break;
            case "missing-member":
                await s.Database.SortedSetRemoveAsync(s.Configuration.Keys.Registry,s.Member(initial,initial.Scopes[0]));
                await s.Database.HashSetAsync(s.Configuration.Keys.Metadata,"count",1); break;
            case "future-event":
                await s.Database.SortedSetAddAsync(bucket,initial.InternalAttemptId,Last+1); break;
            case "overflow":
                await s.Database.HashSetAsync(s.Configuration.Keys.Metadata,"last",RedisAuthenticationQuotaConfiguration.MaximumExactInteger); break;
        }
        var keys=initial.Scopes.Select(scope=>s.Key(initial,scope)).Append(s.Configuration.Keys.Metadata).Append(s.Configuration.Keys.Registry).ToArray();
        var before=await Task.WhenAll(keys.Select(key=>s.Database.KeyDumpAsync(key)));
        var reply=await clock.SendAsync(s.Request());
        Assert.Equal("unavailable",reply.Status);
        Assert.Equal("state",reply.Category);
        Assert.Equal(before,await Task.WhenAll(keys.Select(key=>s.Database.KeyDumpAsync(key))));
    }

    private static async Task<long> ExpiryAsync(RedisAuthenticationQuotaTestScope scope,string key)=>(long)await scope.Database.ExecuteAsync("PEXPIRETIME",key);

    // Copia de prueba: solo TIME y la traslación de época de expiraciones. El cuerpo productivo
    // se concatena sin reemplazar ninguna condición. Ningún reloj/configuración del servidor cambia.
    private sealed class Clock(RedisAuthenticationQuotaTestScope scope,long offsetMilliseconds)
    {
        internal long Now=Last;
        internal static async Task<Clock> CreateAsync(RedisAuthenticationQuotaTestScope scope)
        {
            var clock=new Clock(scope,await scope.NowAsync()/1000-Last/1000);
            // Ambiente recién creado, sin consumos: ejecutar initialize con el mismo Lua y reloj sintético.
            await scope.Database.KeyDeleteAsync(scope.Configuration.Keys.Metadata);
            Assert.Equal("initialized",(await clock.ExecuteAsync(RedisAuthenticationQuotaScriptProtocol.Encode(scope.Configuration,"initialize"))).Status);
            return clock;
        }
        internal long PhysicalExpiry(long horizon)=>(horizon+999)/1000+offsetMilliseconds;
        internal Task<RedisAuthenticationQuotaReply> SendAsync(Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaConsumeRequest request)=>
            ExecuteAsync(RedisAuthenticationQuotaScriptProtocol.Encode(scope.Configuration,"evaluate",request));
        private async Task<RedisAuthenticationQuotaReply> ExecuteAsync(RedisAuthenticationQuotaInvocation invocation)
        {
            var time=Now;
            var shim="local original=redis; local redis={call=function(command,...) local args={...}; " +
                $"if command=='TIME' then return {{'{time/1000000}','{time%1000000}'}} " +
                $"elseif command=='PEXPIREAT' then args[2]=args[2]+{offsetMilliseconds.ToString(CultureInfo.InvariantCulture)}; return original.call(command,unpack(args)) " +
                $"elseif command=='PEXPIRETIME' then local expiry=original.call(command,unpack(args)); if expiry<0 then return expiry end; return expiry-{offsetMilliseconds.ToString(CultureInfo.InvariantCulture)} " +
                "else return original.call(command,unpack(args)) end end};\n";
            var raw=await scope.Database.ScriptEvaluateAsync(shim+RedisAuthenticationQuotaScriptProtocol.ScriptText,invocation.Keys,invocation.Arguments,CommandFlags.DemandMaster);
            return RedisAuthenticationQuotaScriptProtocol.Parse(raw);
        }
    }
}
