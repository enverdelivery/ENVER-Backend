using Amazon.DynamoDBv2;
using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Aws;
using Enver.Testing;

namespace Enver.API.IntegrationTests;

[Collection(AuthenticationDependenciesCollection.Name)]
public sealed class DynamoDbAuthenticationSecurityAuthorityTests(AuthenticationDependenciesFixture dependencies)
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Missing_item_is_not_open_and_bootstrap_is_explicit()
    {
        using var client = dependencies.CreateDynamoDbClient();
        await dependencies.DeleteAuthenticationSecurityAuthorityAsync(client);
        var authority = Authority(client);
        using var budget = Budget();

        var missing = await authority.ReadAsync(budget);
        var registration = await authority.RegisterPotentialIssuanceAsync(new RegisterPotentialIssuanceRequest(0, 0, T0), budget);
        await dependencies.BootstrapAuthenticationSecurityAuthorityAsync(client);
        var open = await authority.ReadAsync(budget);

        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, missing.Status);
        Assert.Null(missing.State);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Unavailable, registration.Status);
        Assert.Equal(AuthenticationSecurityGlobalState.Open, open.State!.State);
        Assert.Equal(0, open.State.Generation);
        Assert.Equal(0, open.State.Revision);
    }

    [Fact]
    public async Task Register_potential_issuance_is_monotonic_and_keeps_open_generation()
    {
        using var client = dependencies.CreateDynamoDbClient();
        await ResetAsync(client);
        var authority = Authority(client);
        using var budget = Budget();

        var first = await authority.RegisterPotentialIssuanceAsync(new RegisterPotentialIssuanceRequest(0, 0, T0.AddSeconds(10)), budget);
        var equal = await authority.RegisterPotentialIssuanceAsync(new RegisterPotentialIssuanceRequest(0, 1, T0.AddSeconds(10)), budget);
        var lower = await authority.RegisterPotentialIssuanceAsync(new RegisterPotentialIssuanceRequest(0, 2, T0.AddSeconds(1)), budget);
        var higher = await authority.RegisterPotentialIssuanceAsync(new RegisterPotentialIssuanceRequest(0, 3, T0.AddSeconds(20)), budget);

        Assert.All(new[] { first, equal, lower, higher }, result => Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, result.Status));
        Assert.Equal(T0.AddSeconds(10), first.State!.LastPossibleIssuanceUtc);
        Assert.Equal(T0.AddSeconds(10), equal.State!.LastPossibleIssuanceUtc);
        Assert.Equal(T0.AddSeconds(10), lower.State!.LastPossibleIssuanceUtc);
        Assert.Equal(T0.AddSeconds(20), higher.State!.LastPossibleIssuanceUtc);
        Assert.All(new[] { first.State, equal.State, lower.State, higher.State }, state => Assert.Equal(AuthenticationSecurityGlobalState.Open, state!.State));
        Assert.Equal(0, higher.State.Generation);
        Assert.Equal(4, higher.State.Revision);
        Assert.Null(higher.State.QuarantineUntilUtc);
    }

    [Fact]
    public async Task Close_preserves_maxima_and_reopen_requires_complete_quarantine()
    {
        using var client = dependencies.CreateDynamoDbClient();
        await ResetAsync(client);
        var authority = Authority(client);
        using var budget = Budget();

        var close = await authority.MergeClosedAsync(new CloseAuthenticationSecurityRequest(0, 0, T0.AddSeconds(10), 900, 30, 60), budget);
        var merge = await authority.MergeClosedAsync(new CloseAuthenticationSecurityRequest(1, 1, T0, 1, 0, 0), budget);
        var early = await authority.TryReopenAsync(new TryReopenAuthenticationSecurityRequest(1, 2, close.State!.QuarantineUntilUtc!.Value.AddMilliseconds(-1)), budget);
        var reopen = await authority.TryReopenAsync(new TryReopenAuthenticationSecurityRequest(1, 2, close.State.QuarantineUntilUtc.Value), budget);

        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, close.Status);
        Assert.Equal(1, close.State!.Generation);
        Assert.Equal(1, close.State.Revision);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, merge.Status);
        Assert.Equal(T0.AddSeconds(10), merge.State!.LastPossibleIssuanceUtc);
        Assert.Equal(close.State.QuarantineUntilUtc, merge.State.QuarantineUntilUtc);
        Assert.Equal(2, merge.State.Revision);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Conflict, early.Status);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, reopen.Status);
        Assert.Equal(AuthenticationSecurityGlobalState.Open, reopen.State!.State);
        Assert.Equal(1, reopen.State.Generation);
        Assert.Equal(3, reopen.State.Revision);
        Assert.Null(reopen.State.LastPossibleIssuanceUtc);
    }

    [Fact]
    public async Task Two_independent_clients_enforce_cas_and_register_close_races_fail_closed()
    {
        using var firstClient = dependencies.CreateDynamoDbClient();
        using var secondClient = dependencies.CreateDynamoDbClient();
        await ResetAsync(firstClient);
        var first = Authority(firstClient);
        var second = Authority(secondClient);
        using var budget = Budget();

        var snapshot = await first.ReadAsync(budget);
        var writerA = first.RegisterPotentialIssuanceAsync(new RegisterPotentialIssuanceRequest(0, 0, T0.AddSeconds(30)), budget).AsTask();
        var writerB = second.RegisterPotentialIssuanceAsync(new RegisterPotentialIssuanceRequest(0, 0, T0.AddSeconds(40)), budget).AsTask();
        var writers = await Task.WhenAll(writerA, writerB);
        var current = await first.ReadAsync(budget);
        var staleClose = await second.MergeClosedAsync(new CloseAuthenticationSecurityRequest(0, 0, T0, 900, 30, 60), budget);
        var closed = await second.MergeClosedAsync(new CloseAuthenticationSecurityRequest(current.State!.Generation, current.State.Revision, T0, 900, 30, 60), budget);
        var staleRegister = await first.RegisterPotentialIssuanceAsync(new RegisterPotentialIssuanceRequest(0, current.State.Revision, T0.AddHours(1)), budget);

        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, snapshot.Status);
        Assert.Single(writers, result => result.Status == AuthenticationSecurityAuthorityResultStatus.Available);
        Assert.Single(writers, result => result.Status == AuthenticationSecurityAuthorityResultStatus.Conflict);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Conflict, staleClose.Status);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, closed.Status);
        Assert.Equal(AuthenticationSecurityGlobalState.Closed, closed.State!.State);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Unavailable, staleRegister.Status);
        Assert.True(closed.State.LastPossibleIssuanceUtc >= T0.AddSeconds(30));
    }

    private DynamoDbAuthenticationSecurityAuthority Authority(IAmazonDynamoDB client) => new(client, new AuthenticationSecurityAuthorityOptions
    {
        TableName = dependencies.DynamoDbTableName,
        OperationTimeoutMilliseconds = 500
    });

    private async Task ResetAsync(IAmazonDynamoDB client)
    {
        await dependencies.DeleteAuthenticationSecurityAuthorityAsync(client);
        await dependencies.BootstrapAuthenticationSecurityAuthorityAsync(client);
    }

    private static RequestBudget Budget() => new(10_000, CancellationToken.None);
}
