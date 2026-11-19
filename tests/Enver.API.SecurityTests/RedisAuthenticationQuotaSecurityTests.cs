using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using StackExchange.Redis;

namespace Enver.API.SecurityTests;

// Los marcadores/pérdidas físicos y lifecycle se acreditan además en RedisAuthenticationQuotaRecoveryTests.
public sealed class RedisAuthenticationQuotaSecurityTests
{
    private static RedisOptions Options() => new() { ConnectionString = "127.0.0.1:1", OperationTimeoutMilliseconds = 50, ConnectionTimeoutMilliseconds = 50 };
    private static RedisAuthenticationQuotaConfiguration Config() => new(new("testing", "config", "identity", AuthenticationQuotaPolicyDescriptor.AcademicInitial,
        AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(5000)), "10000000000000000000000000000001");
    private static AuthenticationQuotaConsumeRequest Request(string revision = "academic-initial", string? identity = null) => new(AuthenticationOperation.Register,
        "20000000000000000000000000000002", revision,
        [new(AuthenticationQuotaScopeType.Email, identity ?? new string('A', 43), new(3, TimeSpan.FromMinutes(15))),
            new(AuthenticationQuotaScopeType.Origin, new string('A', 43), new(20, TimeSpan.FromMinutes(15)))]);

    [Theory]
    [InlineData("private+tag@uttt.edu.mx")]
    [InlineData("203.0.113.10")]
    [InlineData("Bearer.synthetic.secret")]
    public void Raw_identities_never_become_keys_or_error_details(string raw)
    {
        var exception = Assert.Throws<ArgumentException>(() => RedisAuthenticationQuotaScriptProtocol.Encode(Config(), "evaluate", Request(identity: raw)));
        Assert.DoesNotContain(raw, exception.Message);
    }

    [Fact]
    public void Diagnostics_do_not_print_pseudonyms_attempts_or_configuration()
    {
        var config = Config();
        var request = Request();
        var invocation = RedisAuthenticationQuotaScriptProtocol.Encode(config, "evaluate", request);
        var result = RedisAuthenticationQuotaScriptProtocol.Parse(RedisResult.Create((RedisValue)"sensitive invalid result"));
        foreach (var value in new[] { config.ToString(), config.Keys.ToString(), invocation.ToString(), result.ToString() })
        {
            Assert.DoesNotContain(request.Scopes[0].OpaqueId, value);
            Assert.DoesNotContain(request.InternalAttemptId, value);
            Assert.DoesNotContain("sensitive", value);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Local_and_global_closed_each_prevent_dispatch(bool localClosed)
    {
        await using var connections = new RedisConnections(Options());
        var calls = 0;
        var store = new RedisAuthenticationQuotaStore(connections, Options(), Config(), new State(!localClosed), new State(localClosed),
            (_, _) => { calls++; throw new InvalidOperationException("private data"); }, () => ValueTask.FromResult(Array.Empty<string>()));
        using var budget = new RequestBudget(1000, CancellationToken.None);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await store.EvaluateAndConsumeAsync(Request(), budget)).Status);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Forged_revision_and_unavailable_Redis_never_admit()
    {
        await using var connections = new RedisConnections(Options());
        var store = new RedisAuthenticationQuotaStore(connections, Options(), Config(), new State(true), new State(true));
        using (var budget = new RequestBudget(1000, CancellationToken.None))
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await store.EvaluateAndConsumeAsync(Request("forged"), budget)).Status);
        using (var budget = new RequestBudget(1000, CancellationToken.None))
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await store.EvaluateAndConsumeAsync(Request(), budget)).Status);
    }

    [Fact]
    public void Forged_capacity_and_generation_cannot_be_configured()
    {
        Assert.Throws<ArgumentException>(() => new RedisAuthenticationQuotaConfiguration(new("testing", "config", "identity", AuthenticationQuotaPolicyDescriptor.AcademicInitial,
            new(6000, 67108864)), "10000000000000000000000000000001"));
        Assert.Throws<ArgumentException>(() => new RedisAuthenticationQuotaConfiguration(Config().Snapshot, "forged"));
    }

    private sealed class State(bool open) : IAuthenticationSecurityState
    {
        public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget requestBudget) => ValueTask.FromResult(new AuthenticationSecurityDecision(
            open ? AuthenticationSecurityAdmission.Verifiable : AuthenticationSecurityAdmission.Unavailable));
    }
}
