using Enver.API.Features.Authentication.Application.Quotas;

namespace Enver.API.UnitTests;

public sealed class AuthenticationQuotaRecoveryContractTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(-599999999L, true)]
    [InlineData(-600000000L, false)]
    [InlineData(-600000001L, false)]
    public void Activity_uses_open_lower_bound_closed_upper_bound(long relativeTicks, bool expected)
    {
        var state = CapacityData.Observation(CapacityData.Identity(), CapacityData.Now.AddTicks(relativeTicks));
        Assert.Equal(expected, state.IsActiveAt(CapacityData.Now));
        Assert.Equal(state.LastEventUtc!.Value.AddMinutes(1), state.InactiveAtUtc);
    }

    [Fact]
    public void No_events_is_inactive_only_with_verified_state_not_because_a_residual_key_exists()
    {
        var emptyVerified = CapacityData.Observation(CapacityData.Identity(), null);
        Assert.False(emptyVerified.IsActiveAt(CapacityData.Now));
        Assert.Null(emptyVerified.InactiveAtUtc);
        var unknown = CapacityData.Observation(CapacityData.Identity(), null, verifiable: false);
        Assert.Null(unknown.IsActiveAt(CapacityData.Now));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReleaseCapacity(unknown, unknown, CapacityData.Now, true));
        Assert.True(AuthenticationQuotaRecoveryContract.CanReleaseCapacity(emptyVerified, emptyVerified, CapacityData.Now, true));
    }

    [Fact]
    public void Future_event_or_unknown_state_is_never_misclassified_as_releasable()
    {
        var future = CapacityData.Observation(CapacityData.Identity(), CapacityData.Now.AddTicks(1));
        var unknown = CapacityData.Observation(CapacityData.Identity(), CapacityData.Now.AddMinutes(-2), verifiable: false);
        foreach (var state in new[] { future, unknown })
        {
            Assert.Null(state.IsActiveAt(CapacityData.Now));
            Assert.False(AuthenticationQuotaRecoveryContract.CanReleaseCapacity(state, state, CapacityData.Now, true));
        }
    }

    [Fact]
    public void Only_verified_inactive_unchanged_state_can_release_while_serialized_with_consumption()
    {
        var identity = CapacityData.Identity();
        var expired = CapacityData.Observation(identity, CapacityData.Now.AddMinutes(-1));
        var same = CapacityData.Observation(identity, CapacityData.Now.AddMinutes(-1));
        Assert.True(AuthenticationQuotaRecoveryContract.CanReleaseCapacity(expired, same, CapacityData.Now, true));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReleaseCapacity(expired, same, CapacityData.Now, false));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReleaseCapacity(expired,
            CapacityData.Observation(identity, CapacityData.Now, version: 2), CapacityData.Now, true));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReleaseCapacity(expired,
            CapacityData.Observation(identity, expired.LastEventUtc, version: 2), CapacityData.Now, true));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReleaseCapacity(expired,
            CapacityData.Observation(identity, expired.LastEventUtc, verifiable: false), CapacityData.Now, true));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReleaseCapacity(expired,
            CapacityData.Observation(identity with { OpaqueId = new string('b', 43) }, expired.LastEventUtc), CapacityData.Now, true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-599999999L)]
    public void Active_partitions_cannot_be_evicted_to_make_room(long relativeTicks)
    {
        var active = CapacityData.Observation(CapacityData.Identity(), CapacityData.Now.AddTicks(relativeTicks));
        Assert.True(active.IsActiveAt(CapacityData.Now));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReleaseCapacity(active, active, CapacityData.Now, true));
    }

    [Fact]
    public void Retention_is_finite_last_event_plus_window_without_added_revocation_margin_or_local_clock()
    {
        var definition = new AuthenticationQuotaDefinition(3, TimeSpan.FromSeconds(7));
        var state = CapacityData.Observation(CapacityData.Identity(), CapacityData.Now, definition: definition);
        Assert.Equal(CapacityData.Now.AddSeconds(7), state.InactiveAtUtc);
        Assert.True(state.IsActiveAt(CapacityData.Now.AddSeconds(7).AddTicks(-1)));
        Assert.False(state.IsActiveAt(CapacityData.Now.AddSeconds(7)));
        Assert.Throws<ArgumentException>(() => CapacityData.Observation(CapacityData.Identity(), DateTimeOffset.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CapacityData.Observation(CapacityData.Identity(), CapacityData.Now, version: -1));
    }

    [Theory]
    [InlineData(7, 11, 11)]
    [InlineData(1800, 1200, 1800)]
    public void Lost_counter_quarantine_derives_maximum_affected_window_not_token_quarantine(int oldWindow, int newWindow, int expected)
    {
        var oldPolicy = QuotaTestData.Policy("old", oldWindow);
        var next = QuotaTestData.Policy("next", newWindow);
        var last = CapacityData.Now;
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterLostCounters(oldPolicy, next, last,
            last.AddSeconds(expected).AddTicks(-1), true, true, true));
        Assert.True(AuthenticationQuotaRecoveryContract.CanReopenAfterLostCounters(oldPolicy, next, last,
            last.AddSeconds(expected), true, true, true));
    }

    [Theory]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void Waiting_alone_cannot_reopen_without_global_stop_inflight_resolution_and_reconciliation(bool stopped, bool resolved, bool reconciled)
    {
        var policy = AuthenticationQuotaPolicyDescriptor.AcademicInitial;
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterLostCounters(policy, policy, CapacityData.Now,
            CapacityData.Now.AddDays(1), stopped, resolved, reconciled));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterVerifiedReconstruction(stopped, resolved, true, reconciled));
    }

    [Fact]
    public void Missing_last_possible_consumption_or_time_prevents_reopen_and_alert_does_not_start_horizon()
    {
        var policy = QuotaTestData.Policy("same", 120);
        var alert = CapacityData.Now;
        var last = alert.AddSeconds(30);
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterLostCounters(policy, policy, null, alert.AddDays(1), true, true, true));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterLostCounters(policy, policy, last, null, true, true, true));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterLostCounters(policy, policy, last, alert, true, true, true));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterLostCounters(policy, policy, last, alert.AddSeconds(120), true, true, true));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterVerifiedReconstruction(true, true, false, true));
        Assert.True(AuthenticationQuotaRecoveryContract.CanReopenAfterVerifiedReconstruction(true, true, true, true));
    }

    [Theory]
    [InlineData(AuthenticationPseudonymizationChange.Key)]
    [InlineData(AuthenticationPseudonymizationChange.CanonicalRepresentation)]
    public void Identity_changes_need_053_drain_and_reconciled_capacity_before_reopen(AuthenticationPseudonymizationChange change)
    {
        var policy = AuthenticationQuotaPolicyDescriptor.AcademicInitial;
        var transition = new AuthenticationPseudonymizationTransition(policy, policy, change);
        var last = CapacityData.Now;
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterIdentityDrain(transition, last, last + transition.DrainWindow - TimeSpan.FromTicks(1), true, true, true));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterIdentityDrain(transition, last, last + transition.DrainWindow, true, true, false));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterIdentityDrain(transition, last, last + transition.DrainWindow, false, true, true));
        Assert.False(AuthenticationQuotaRecoveryContract.CanReopenAfterIdentityDrain(transition, last, last + transition.DrainWindow, true, false, true));
        Assert.True(AuthenticationQuotaRecoveryContract.CanReopenAfterIdentityDrain(transition, last, last + transition.DrainWindow, true, true, true));
    }

    [Fact]
    public void Recovery_has_ordered_obligations_and_no_runtime_reset_release_timer_or_eviction()
    {
        Assert.Equal(new[] { AuthenticationQuotaRecoveryStep.StopAdmissionsAcrossInstances, AuthenticationQuotaRecoveryStep.ResolveInFlightOperations,
            AuthenticationQuotaRecoveryStep.EstablishLastPossibleConsumption, AuthenticationQuotaRecoveryStep.ReconstructAndValidateOrWaitApplicableWindow,
            AuthenticationQuotaRecoveryStep.ReconcileCapacity, AuthenticationQuotaRecoveryStep.ReopenOnlyWithVerifiableContinuity }, AuthenticationQuotaRecoveryContract.RequiredSteps);
        Assert.DoesNotContain(typeof(AuthenticationQuotaRecoveryContract).GetMethods(), method => method.Name is "Reset" or "Evict" or "Release" or "Reopen" or "StartTimer");
        Assert.Throws<NotSupportedException>(() => ((IList<AuthenticationQuotaRecoveryStep>)AuthenticationQuotaRecoveryContract.RequiredSteps).Clear());
    }
}
