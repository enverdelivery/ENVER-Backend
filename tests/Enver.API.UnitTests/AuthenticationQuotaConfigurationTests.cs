using Enver.API.Features.Authentication.Application.Quotas;

namespace Enver.API.UnitTests;

public sealed class AuthenticationQuotaConfigurationTests
{
    [Fact]
    public void Equivalent_snapshots_compare_content_and_not_object_reference_or_rule_order()
    {
        var first = CapacityData.Snapshot();
        var reordered = new AuthenticationQuotaPolicyDescriptor(first.Policy.Revision, first.Policy.Rules.Reverse());
        var second = CapacityData.Snapshot(policy: reordered);
        Assert.NotSame(first, second);
        Assert.True(first.Matches(second));
        var request = CapacityData.LoginRequest();
        Assert.Null(AuthenticationQuotaCapacityContract.GetPreDispatchFailure(request, first, second, true));
    }

    [Theory]
    [InlineData("environment")]
    [InlineData("configuration-version")]
    [InlineData("identity-reference")]
    [InlineData("policy-revision")]
    [InlineData("definitions-with-same-revision")]
    [InlineData("capacity")]
    [InlineData("memory")]
    public void Divergent_snapshots_fail_closed_even_if_some_revision_strings_match(string difference)
    {
        var configured = CapacityData.Snapshot();
        var active = difference switch
        {
            "environment" => CapacityData.Snapshot(environment: "staging"),
            "configuration-version" => CapacityData.Snapshot(version: "config-2"),
            "identity-reference" => CapacityData.Snapshot(identity: "identity-format-2"),
            "policy-revision" => CapacityData.Snapshot(policy: new("next-policy", configured.Policy.Rules)),
            "definitions-with-same-revision" => CapacityData.Snapshot(policy: QuotaTestData.Policy(configured.Policy.Revision, 7)),
            "capacity" => CapacityData.Snapshot(max: 4000),
            _ => CapacityData.Snapshot(budget: 2 * configured.Capacity.MemoryBudgetBytes)
        };
        Assert.False(configured.Matches(active));
        var result = AuthenticationQuotaCapacityContract.GetPreDispatchFailure(CapacityData.LoginRequest(), configured, active, true);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, result!.Status);
        Assert.Null(result.RetryAfter);
        Assert.False(result.ResultUncertain);
    }

    [Fact]
    public void Matching_snapshot_does_not_replace_052_policy_or_topology_checks()
    {
        var snapshot = CapacityData.Snapshot();
        var request = CapacityData.LoginRequest();
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
            AuthenticationQuotaCapacityContract.GetPreDispatchFailure(request, snapshot, null, true)!.Status);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
            AuthenticationQuotaCapacityContract.GetPreDispatchFailure(request, snapshot, snapshot, false)!.Status);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
            AuthenticationQuotaCapacityContract.GetPreDispatchFailure(QuotaTestData.Request(revision: "unknown"), snapshot, snapshot, true)!.Status);
    }

    [Theory]
    [InlineData(4000)]
    [InlineData(6000)]
    public void Synthetic_future_capacity_transitions_preserve_state_and_require_coordination_and_memory_approval(int nextLimit)
    {
        var previous = CapacityData.Snapshot();
        // Valor sintético para probar el protocolo, NO aprobación de otro dimensionamiento operativo.
        var next = CapacityData.Snapshot(version: "config-2", max: nextLimit);
        var transition = new AuthenticationQuotaCapacityTransition(previous, next);
        Assert.Same(previous, transition.Previous);
        Assert.Same(next, transition.Next);
        Assert.False(transition.RequiresWindowDrain);
        Assert.Equal(new[] { AuthenticationQuotaCapacityTransitionStep.PrepareApprovedCapacityAndMemoryBudget,
            AuthenticationQuotaCapacityTransitionStep.StopOrFenceOldConfigurationAdmissions, AuthenticationQuotaCapacityTransitionStep.ResolveInFlightOperations,
            AuthenticationQuotaCapacityTransitionStep.PreserveAllActiveState, AuthenticationQuotaCapacityTransitionStep.ActivateSingleSnapshotAcrossInstances,
            AuthenticationQuotaCapacityTransitionStep.ReopenWithVerifiedConfigurationAndContinuity }, AuthenticationQuotaCapacityTransition.RequiredSteps);
        Assert.Throws<NotSupportedException>(() => ((IList<AuthenticationQuotaCapacityTransitionStep>)AuthenticationQuotaCapacityTransition.RequiredSteps).Clear());
        Assert.False(previous.Matches(next));
        Assert.DoesNotContain(typeof(AuthenticationQuotaCapacityTransition).GetMethods(), method => method.Name is "Activate" or "Reset" or "Evict");
    }

    [Fact]
    public void Reduction_below_occupancy_preserves_existing_scopes_and_blocks_new_until_whole_request_fits()
    {
        var transition = new AuthenticationQuotaCapacityTransition(CapacityData.Snapshot(), CapacityData.Snapshot(version: "lower", max: 4000));
        var identity = CapacityData.Identity();
        var occupied = new AuthenticationQuotaCapacityState(5000, true);
        var active = CapacityData.Observation(identity, CapacityData.Now);
        var existing = AuthenticationQuotaCapacityContract.Evaluate(transition.Next.Capacity, occupied, [identity], [active], CapacityData.Now);
        Assert.True(existing.CapacitySufficient);
        var blocked = AuthenticationQuotaCapacityContract.Evaluate(transition.Next.Capacity, occupied,
            [identity with { OpaqueId = new string('b', 43) }], [], CapacityData.Now);
        Assert.False(blocked.CapacitySufficient);
        Assert.Empty(blocked.NewPartitions);
        Assert.Equal(5000, occupied.ActivePartitionCount);
        Assert.Equal(CapacityData.Now, active.LastEventUtc);
        Assert.False(AuthenticationQuotaCapacityContract.Evaluate(transition.Next.Capacity, new(4000, true), [identity], [], CapacityData.Now).CapacitySufficient);
        Assert.True(AuthenticationQuotaCapacityContract.Evaluate(transition.Next.Capacity, new(3999, true), [identity], [], CapacityData.Now).CapacitySufficient);
    }

    [Fact]
    public void Increased_capacity_is_not_used_under_an_old_snapshot()
    {
        var previous = CapacityData.Snapshot();
        var next = CapacityData.Snapshot(version: "higher", max: 6000, budget: 2 * previous.Capacity.MemoryBudgetBytes);
        _ = new AuthenticationQuotaCapacityTransition(previous, next);
        var request = CapacityData.LoginRequest();
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable,
            AuthenticationQuotaCapacityContract.GetPreDispatchFailure(request, next, previous, true)!.Status);
        Assert.Null(AuthenticationQuotaCapacityContract.GetPreDispatchFailure(request, next, next, true));
        Assert.True(AuthenticationQuotaCapacityContract.EvaluateRequest(next.Capacity, new(5000, true), request, [], CapacityData.Now).CapacitySufficient);
    }

    [Fact]
    public void Capacity_only_transition_cannot_bypass_policy_or_identity_drain()
    {
        var previous = CapacityData.Snapshot();
        AuthenticationQuotaConfigurationSnapshot[] invalid = [CapacityData.Snapshot(max: 4000),
            CapacityData.Snapshot(version: "next", identity: "changed-identity"),
            CapacityData.Snapshot(version: "next", environment: "staging"),
            CapacityData.Snapshot(version: "next", policy: new("next-policy", previous.Policy.Rules)),
            CapacityData.Snapshot(version: "next", policy: QuotaTestData.Policy(previous.Policy.Revision, 7))];
        foreach (var next in invalid) Assert.Throws<ArgumentException>(() => new AuthenticationQuotaCapacityTransition(previous, next));
    }

    [Fact]
    public void Policy_revision_does_not_reset_capacity_or_change_partition_identity()
    {
        var request = CapacityData.LoginRequest();
        var nextPolicy = new AuthenticationQuotaPolicyDescriptor("next-policy", AuthenticationQuotaPolicyDescriptor.AcademicInitial.Rules);
        var nextRequest = new AuthenticationQuotaConsumeRequest(request.Operation, "next-attempt", nextPolicy.Revision, request.Scopes);
        var occupied = new AuthenticationQuotaCapacityState(5000, true);
        var before = AuthenticationQuotaCapacityContract.EvaluateRequest(CapacityData.Descriptor(), occupied, request, [], CapacityData.Now);
        var after = AuthenticationQuotaCapacityContract.EvaluateRequest(CapacityData.Descriptor(), occupied, nextRequest, [], CapacityData.Now);
        Assert.Equal(request.Scopes.Select(request.GetPartitionIdentity), nextRequest.Scopes.Select(nextRequest.GetPartitionIdentity));
        Assert.Equal(before.NewPartitionCount, after.NewPartitionCount);
        Assert.Equal(AuthenticationQuotaStoreStatus.Unavailable, before.Failure!.Status);
        Assert.Equal(before.Failure, after.Failure);
        Assert.Equal(5000, occupied.ActivePartitionCount);
        Assert.DoesNotContain(typeof(AuthenticationQuotaPolicyDescriptor).GetProperties(), property => property.Name is "Capacity" or "MaxActivePartitions" or "MemoryBudgetBytes");
    }

    [Theory]
    [InlineData("", "config-1", "identity-1")]
    [InlineData("Testing", "config-1", "identity-1")]
    [InlineData("testing", "", "identity-1")]
    [InlineData("testing", "config-1", "")]
    [InlineData("testing", "config\n1", "identity-1")]
    public void Snapshot_requires_canonical_trusted_environment_and_explicit_nonsecret_references(string environment, string version, string identity) =>
        Assert.Throws<ArgumentException>(() => CapacityData.Snapshot(environment: environment, version: version, identity: identity));
}
