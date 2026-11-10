using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using StackExchange.Redis;

namespace Enver.API.UnitTests.RedisQuotas;

public sealed class RedisAuthenticationQuotaScriptProtocolTests
{
    [Theory]
    [InlineData("admitted", "ok", 1, "1")]
    [InlineData("rejected", "quota", 0, "1")]
    [InlineData("unavailable", "capacity", 1, "1")]
    [InlineData("admitted", "unknown", 0, "1")]
    [InlineData("admitted", "ok", 0, "2")]
    [InlineData("unknown", "ok", 0, "1")]
    public void Unknown_or_inconsistent_results_are_uncertain(string status, string category, long retry, string version)
    {
        var parsed = RedisAuthenticationQuotaScriptProtocol.Parse(QuotaTestData.Reply(status, category, retry, version));
        Assert.False(parsed.IsSemantic);
        Assert.Equal(AuthenticationQuotaStoreStatus.Uncertain, parsed.ToConsumeResult().Status);
    }

    [Fact]
    public void Encoding_is_deterministic_and_candidates_are_only_declared_hints()
    {
        var config = QuotaTestData.Configuration();
        var request = QuotaTestData.Request();
        var member = $"login:email:{QuotaTestData.Opaque(3)}";
        var first = RedisAuthenticationQuotaScriptProtocol.Encode(config, "evaluate", request, [member, member]);
        var second = RedisAuthenticationQuotaScriptProtocol.Encode(config, "evaluate", request, [member]);
        Assert.Equal(first.Arguments, second.Arguments);
        Assert.Equal(first.Keys, second.Keys);
        Assert.Equal(5, first.Keys.Length);
        Assert.Equal(CommandFlags.DemandMaster, RedisAuthenticationQuotaScriptProtocol.Flags);
        Assert.Equal("1", first.Arguments[14].ToString());
        Assert.DoesNotContain(request.InternalAttemptId, first.ToString());
        Assert.DoesNotContain(request.Scopes[0].OpaqueId, first.ToString());
    }

    [Fact]
    public void Exact_microsecond_windows_and_range_are_enforced()
    {
        Assert.Equal(900000000, RedisAuthenticationQuotaConfiguration.WindowMicroseconds(TimeSpan.FromMinutes(15)));
        Assert.Throws<ArgumentException>(() => RedisAuthenticationQuotaConfiguration.WindowMicroseconds(TimeSpan.FromTicks(1)));
        Assert.Throws<ArgumentException>(() => RedisAuthenticationQuotaConfiguration.WindowMicroseconds(TimeSpan.MaxValue));
        var parsed = RedisAuthenticationQuotaScriptProtocol.Parse(QuotaTestData.Reply("admitted", "ok", now: RedisAuthenticationQuotaConfiguration.MaximumExactInteger + 1));
        Assert.False(parsed.IsSemantic);
        var now = DateTimeOffset.UnixEpoch.AddSeconds(10);
        var definition = new AuthenticationQuotaDefinition(1, TimeSpan.FromSeconds(1));
        Assert.Null(AuthenticationQuotaRetryAfter.ForScope(now, definition, [now.AddSeconds(-1)]));
        Assert.Equal(TimeSpan.FromSeconds(1), AuthenticationQuotaRetryAfter.ForScope(now, definition, [now.AddSeconds(-1).AddTicks(10)]));
    }

    [Fact]
    public void Snapshot_and_generation_are_captured_not_derived_from_request()
    {
        var first = QuotaTestData.Configuration();
        var second = QuotaTestData.Configuration("20000000000000000000000000000002");
        Assert.True(first.Snapshot.Matches(second.Snapshot));
        Assert.NotEqual(RedisAuthenticationQuotaScriptProtocol.Encode(first, "evaluate", QuotaTestData.Request()).Arguments[9],
            RedisAuthenticationQuotaScriptProtocol.Encode(second, "evaluate", QuotaTestData.Request()).Arguments[9]);
        Assert.Equal(first.Keys.Metadata, second.Keys.Metadata);
        Assert.Throws<ArgumentException>(() => RedisAuthenticationQuotaScriptProtocol.Encode(first, "evaluate", QuotaTestData.Request("forged")));
    }

    [Fact]
    public void Script_is_embedded_uses_one_TIME_and_no_undeclared_dynamic_key_access()
    {
        var script = RedisAuthenticationQuotaScriptProtocol.ScriptText;
        Assert.Equal(1, script.Split("redis.call('TIME')").Length - 1);
        Assert.DoesNotContain("FLUSH", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("'dirty', '1'", script);
        Assert.Contains("'PEXPIREAT'", script);
        Assert.Contains("'collision'", script);
    }
}
