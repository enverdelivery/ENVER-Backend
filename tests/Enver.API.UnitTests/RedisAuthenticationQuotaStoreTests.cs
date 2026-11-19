using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Application.Quotas;
using Enver.API.Http;
using Enver.API.Infrastructure.Redis;
using StackExchange.Redis;

namespace Enver.API.UnitTests.RedisQuotas;

public sealed class RedisAuthenticationQuotaStoreTests
{
    [Theory]
    [InlineData("admitted", "ok", 0, AuthenticationQuotaStoreStatus.Admitted)]
    [InlineData("rejected", "quota", 2, AuthenticationQuotaStoreStatus.Rejected)]
    [InlineData("unavailable", "capacity", 0, AuthenticationQuotaStoreStatus.Unavailable)]
    public async Task Semantic_results_are_mapped_once(string status, string category, int retry, AuthenticationQuotaStoreStatus expected)
    {
        var calls = 0;
        await using var connections = new RedisConnections(QuotaTestData.Options());
        var store = QuotaTestData.Store(connections, execute: (_, invocation) =>
        {
            calls++;
            Assert.Equal("enver.auth.quota", invocation.Arguments[0].ToString());
            Assert.Equal("5000", invocation.Arguments[7].ToString());
            Assert.Equal("67108864", invocation.Arguments[8].ToString());
            Assert.Equal(4, invocation.Keys.Length);
            return ValueTask.FromResult(QuotaTestData.Reply(status, category, retry));
        });
        using var budget = new RequestBudget(1000, CancellationToken.None);
        var result = await store.EvaluateAndConsumeAsync(QuotaTestData.Request(), budget);
        Assert.Equal(expected, result.Status);
        Assert.Equal(retry > 0 ? TimeSpan.FromSeconds(retry) : (TimeSpan?)null, result.RetryAfter);
        Assert.Equal(1, calls);
    }

    [Theory]
    [InlineData(false, AuthenticationSecurityAdmission.Unavailable)]
    [InlineData(true, AuthenticationSecurityAdmission.Unavailable)]
    [InlineData(false, AuthenticationSecurityAdmission.Uncertain)]
    [InlineData(true, AuthenticationSecurityAdmission.Uncertain)]
    public async Task Both_security_boundaries_are_required(bool global, AuthenticationSecurityAdmission admission)
    {
        var calls = 0;
        await using var connections = new RedisConnections(QuotaTestData.Options());
        var store = QuotaTestData.Store(connections, global ? QuotaTestData.Open : new QuotaTestData.State(admission),
            global ? new QuotaTestData.State(admission) : QuotaTestData.Open, (_, _) => { calls++; throw new InvalidOperationException(); });
        using var budget = new RequestBudget(1000, CancellationToken.None);
        var result = await store.EvaluateAndConsumeAsync(QuotaTestData.Request(), budget);
        Assert.Equal(admission == AuthenticationSecurityAdmission.Uncertain ? AuthenticationQuotaStoreStatus.Uncertain : AuthenticationQuotaStoreStatus.Unavailable, result.Status);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Revision_or_definition_mismatch_never_reads_hints_or_dispatches()
    {
        await using var connections = new RedisConnections(QuotaTestData.Options());
        var calls = 0;
        var store = new RedisAuthenticationQuotaStore(connections, QuotaTestData.Options(), QuotaTestData.Configuration(), QuotaTestData.Open, QuotaTestData.Open,
            (_, _) => { calls++; throw new InvalidOperationException(); }, () => { calls++; throw new InvalidOperationException(); });
        foreach (var request in new[] { QuotaTestData.Request(revision: "forged"), QuotaTestData.Request(forgedDefinition: true) })
        {
            using var budget = new RequestBudget(1000, CancellationToken.None);
            Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await store.EvaluateAndConsumeAsync(request, budget)).Status);
        }
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Pre_dispatch_cancellation_is_unavailable()
    {
        await using var connections = new RedisConnections(QuotaTestData.Options());
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        using var budget = new RequestBudget(1000, canceled.Token);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, (await QuotaTestData.Store(connections).EvaluateAndConsumeAsync(QuotaTestData.Request(), budget)).Status);
    }

    [Fact]
    public async Task Post_dispatch_exception_is_uncertain_without_retry_or_read_back()
    {
        var calls = 0;
        await using var connections = new RedisConnections(QuotaTestData.Options());
        var store = QuotaTestData.Store(connections, execute: (_, _) => { calls++; throw new InvalidOperationException("synthetic private data"); });
        using var budget = new RequestBudget(1000, CancellationToken.None);
        var result = await store.EvaluateAndConsumeAsync(QuotaTestData.Request(), budget);
        Assert.Equal(AuthenticationQuotaStoreStatus.Uncertain, result.Status);
        Assert.Equal(1, calls);
        Assert.Null(result.RetryAfter);
    }

    [Fact]
    public async Task Post_dispatch_timeout_is_uncertain()
    {
        var calls = 0;
        await using var connections = new RedisConnections(QuotaTestData.Options());
        var store = QuotaTestData.Store(connections, execute: async (dependency, _) =>
        {
            calls++;
            await Task.Delay(Timeout.InfiniteTimeSpan, dependency.Token);
            return QuotaTestData.Reply("admitted", "ok");
        });
        using var budget = new RequestBudget(1000, CancellationToken.None);
        var result = await store.EvaluateAndConsumeAsync(QuotaTestData.Request(), budget);
        Assert.Equal(AuthenticationQuotaStoreStatus.Uncertain, result.Status);
        Assert.Equal(BudgetCancellationCause.DependencyTimeout, result.CancellationCause);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Malformed_response_is_uncertain()
    {
        await using var connections = new RedisConnections(QuotaTestData.Options());
        var store = QuotaTestData.Store(connections, execute: (_, _) => ValueTask.FromResult(RedisResult.Create((RedisValue)"unexpected")));
        using var budget = new RequestBudget(1000, CancellationToken.None);
        Assert.Equal(AuthenticationQuotaStoreStatus.Uncertain, (await store.EvaluateAndConsumeAsync(QuotaTestData.Request(), budget)).Status);
    }
}

internal static class QuotaTestData
{
    internal static string Opaque(int seed)
    {
        var bytes = new byte[32];
        BitConverter.GetBytes(seed).CopyTo(bytes, 0);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
    internal static readonly State Open = new(AuthenticationSecurityAdmission.Verifiable);
    internal static RedisOptions Options() => new() { ConnectionString = "127.0.0.1:1", OperationTimeoutMilliseconds = 50, ConnectionTimeoutMilliseconds = 50 };
    internal static RedisAuthenticationQuotaConfiguration Configuration(string? generation = null) => new(new("testing", "config-1", "identity-1",
        AuthenticationQuotaPolicyDescriptor.AcademicInitial, AuthenticationQuotaCapacityDescriptor.ForAcademicActivation(5000)), generation ?? "10000000000000000000000000000001");
    internal static AuthenticationQuotaConsumeRequest Request(string revision = "academic-initial", bool forgedDefinition = false) =>
        new(AuthenticationOperation.Register, Guid.NewGuid().ToString("N"), revision,
        [new(AuthenticationQuotaScopeType.Email, Opaque(1), forgedDefinition ? new(4, TimeSpan.FromMinutes(15)) : new(3, TimeSpan.FromMinutes(15))),
            new(AuthenticationQuotaScopeType.Origin, Opaque(2), new(20, TimeSpan.FromMinutes(15)))]);
    internal static RedisResult Reply(string status, string category, long retry = 0, string version = "1", long now = 1000000) =>
        RedisResult.Create(new RedisResult[] { RedisResult.Create((RedisValue)"enver.auth.quota"), RedisResult.Create((RedisValue)version),
            RedisResult.Create((RedisValue)status), RedisResult.Create((RedisValue)category), RedisResult.Create((RedisValue)retry), RedisResult.Create((RedisValue)now) });
    internal static RedisAuthenticationQuotaStore Store(RedisConnections connections, IAuthenticationSecurityState? local = null,
        IAuthenticationSecurityState? global = null, Func<DependencyBudget, RedisAuthenticationQuotaInvocation, ValueTask<RedisResult>>? execute = null) =>
        new(connections, Options(), Configuration(), local ?? Open, global ?? Open, execute ?? ((_, _) => ValueTask.FromResult(Reply("admitted", "ok"))), () => ValueTask.FromResult(Array.Empty<string>()));
    internal sealed class State(AuthenticationSecurityAdmission admission) : IAuthenticationSecurityState
    {
        public ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget requestBudget) => ValueTask.FromResult(new AuthenticationSecurityDecision(admission));
    }
}
