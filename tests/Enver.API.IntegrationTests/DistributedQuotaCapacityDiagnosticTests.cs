using Enver.API.Infrastructure.Redis;
using Enver.Testing;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class DistributedQuotaCapacityDiagnosticTests(AuthenticationDependenciesFixture fixture)
{
    [Fact]
    public async Task Observer_distinguishes_three_hint_removals_from_two_new_registry_members()
    {
        await using var data = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        // Fixture sintético separado: dos miembros vigentes retienen el registry mientras
        // tres residuos conocidos prueban el observador. No reproduce ni prueba el fallo CI.
        Assert.Equal(Enver.API.Features.Authentication.Application.Quotas.AuthenticationQuotaStoreStatus.Admitted,
            (await data.SendAsync(data.Request(99))).Status);
        var now = await data.NowAsync();
        var candidates = new List<string>();
        for (var i = 10; i < 13; i++)
        {
            var seeded = data.Request(i);
            var scope = seeded.Scopes[0];
            var staleHorizon = now - 1000;
            var timestamp = staleHorizon - RedisAuthenticationQuotaConfiguration.WindowMicroseconds(scope.Definition.Window);
            await data.SeedBucketAsync(data.Key(seeded, scope), data.Member(seeded, scope),
                [new(Guid.NewGuid().ToString("N"), timestamp)], staleHorizon);
            candidates.Add(data.Member(seeded, scope));
        }
        await data.SynchronizeMetadataAsync();
        var invocation = RedisAuthenticationQuotaScriptProtocol.Encode(data.Configuration, "evaluate", data.Request(), candidates);
        var diagnostic = new DistributedQuotaCapacityDiagnostics(data.Configuration.Keys.Metadata);
        var reply = RedisAuthenticationQuotaScriptProtocol.Parse(await diagnostic.ExecuteAsync(data.Database, data.Configuration, invocation));
        Assert.Equal("admitted", reply.Status);
        var evidence = (await diagnostic.TransitionEvidenceAsync())!.Value;
        Assert.Equal(5, evidence.GetProperty("before").GetProperty("Count").GetInt64());
        Assert.Equal(4, evidence.GetProperty("after").GetProperty("Count").GetInt64());
        Assert.Equal(3, evidence.GetProperty("staleHintMembersObserved").GetInt32());
        Assert.Equal(3, evidence.GetProperty("hintMembersDisappeared").GetInt32());
        Assert.Equal(2, evidence.GetProperty("newRegistryMembersObserved").GetInt32());
    }

    [Fact]
    public async Task Observer_preserves_EVAL_inputs_and_reply_and_measures_two_new_members()
    {
        await using var data = await RedisAuthenticationQuotaTestScope.CreateAsync(fixture);
        var diagnostic = new DistributedQuotaCapacityDiagnostics(data.Configuration.Keys.Metadata);
        var request = data.Request();
        var invocation = RedisAuthenticationQuotaScriptProtocol.Encode(data.Configuration, "evaluate", request);
        var keys = invocation.Keys.ToArray();
        var arguments = invocation.Arguments.ToArray();
        diagnostic.Arm();
        var raw = await diagnostic.ExecuteAsync(data.Database, data.Configuration, invocation);
        var reply = RedisAuthenticationQuotaScriptProtocol.Parse(raw);
        Assert.True(reply.IsSemantic);
        Assert.Equal("admitted", reply.Status);
        Assert.Equal(keys, invocation.Keys);
        Assert.Equal(arguments, invocation.Arguments);
        Assert.Equal(6, ((StackExchange.Redis.RedisResult[])raw!).Length);
        var evidence = (await diagnostic.TransitionEvidenceAsync())!.Value;
        Assert.Equal(0, evidence.GetProperty("before").GetProperty("Count").GetInt64());
        Assert.Equal(0, evidence.GetProperty("before").GetProperty("Cardinality").GetInt64());
        Assert.Equal(2, evidence.GetProperty("after").GetProperty("Count").GetInt64());
        Assert.Equal(2, evidence.GetProperty("after").GetProperty("Cardinality").GetInt64());
        Assert.Equal(2, evidence.GetProperty("requestedScopes").GetInt32());
        Assert.Equal(2, evidence.GetProperty("newRegistryMembersObserved").GetInt32());
        Assert.Equal(0, evidence.GetProperty("hintMembersDisappeared").GetInt32());
        Assert.Equal(1, evidence.GetProperty("admittedRequests").GetInt32());
        Assert.DoesNotContain(data.Configuration.Keys.Prefix, evidence.GetRawText());
        Assert.True(evidence.GetRawText().Length < 2000);
    }
}
