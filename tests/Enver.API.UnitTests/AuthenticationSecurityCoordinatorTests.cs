using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;
using Enver.API.Infrastructure.Security;
using Enver.Testing;

namespace Enver.API.UnitTests;

public sealed class AuthenticationSecurityCoordinatorTests
{
    private static readonly DateTimeOffset T0 = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Absent_authority_record_denies_by_default()
    {
        var coordinator = Coordinator(new AuthenticationSecurityAuthorityFake(), new ManualTimeProvider(T0));

        var decision = await EvaluateAsync(coordinator);

        Assert.False(decision.CanContinue);
        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, decision.Admission);
    }

    [Fact]
    public async Task Unavailable_and_uncertain_authority_deny_and_close_latch()
    {
        var authority = new AuthenticationSecurityAuthorityFake(Open());
        authority.EnqueueReadOutcome(AuthenticationSecurityAuthorityResultStatus.Unavailable);
        var coordinator = Coordinator(authority, new ManualTimeProvider(T0));

        var unavailable = await EvaluateAsync(coordinator);
        authority.EnqueueReadOutcome(AuthenticationSecurityAuthorityResultStatus.Uncertain);
        var uncertain = await EvaluateAsync(coordinator);

        Assert.Equal(AuthenticationSecurityAdmission.Unavailable, unavailable.Admission);
        Assert.Equal(AuthenticationSecurityAdmission.Uncertain, uncertain.Admission);
        Assert.Null(await coordinator.BeginEmissionAsync(Budget()));
    }

    [Fact]
    public async Task Closed_authority_denies_and_open_authority_admits()
    {
        var closed = Coordinator(new AuthenticationSecurityAuthorityFake(Closed(T0)), new ManualTimeProvider(T0));
        var open = Coordinator(new AuthenticationSecurityAuthorityFake(Open()), new ManualTimeProvider(T0));

        Assert.False((await EvaluateAsync(closed)).CanContinue);
        Assert.True((await EvaluateAsync(open)).CanContinue);
    }

    [Fact]
    public async Task Close_latches_before_authority_failure_and_positive_state_cannot_reopen_it()
    {
        var authority = new AuthenticationSecurityAuthorityFake(Open());
        authority.EnqueueReadOutcome(AuthenticationSecurityAuthorityResultStatus.Unavailable);
        var coordinator = Coordinator(authority, new ManualTimeProvider(T0));

        var close = await coordinator.CloseAsync(Budget());
        var decision = await EvaluateAsync(coordinator);

        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Unavailable, close.Status);
        Assert.False(decision.CanContinue);
    }

    [Fact]
    public async Task Begin_complete_and_fail_emissions_release_local_tracking_without_lowering_maximum()
    {
        var time = new ManualTimeProvider(T0);
        var authority = new AuthenticationSecurityAuthorityFake(Open());
        var coordinator = Coordinator(authority, time);

        var first = await coordinator.BeginEmissionAsync(Budget(time, 1_000));
        Assert.NotNull(first);
        coordinator.CompleteEmission(first!);
        time.Advance(TimeSpan.FromSeconds(10));
        var second = await coordinator.BeginEmissionAsync(Budget(time, 1_000));
        Assert.NotNull(second);
        coordinator.FailEmission(second!);
        time.SetUtcNow(T0.AddSeconds(5));

        var close = await coordinator.CloseAsync(Budget());

        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, close.Status);
        Assert.Equal(T0.AddSeconds(11), authority.State!.LastPossibleIssuanceUtc);
    }

    [Fact]
    public async Task Potential_emission_remains_conservative_after_uncertain_close()
    {
        var time = new ManualTimeProvider(T0);
        var authority = new AuthenticationSecurityAuthorityFake(Open());
        var coordinator = Coordinator(authority, time);
        var lease = await coordinator.BeginEmissionAsync(Budget(time, 1_000));
        Assert.NotNull(lease);
        authority.EnqueueReadOutcome(AuthenticationSecurityAuthorityResultStatus.Uncertain);

        var uncertain = await coordinator.CloseAsync(Budget());
        coordinator.FailEmission(lease!);
        time.Advance(TimeSpan.FromSeconds(1));
        var confirmed = await coordinator.CloseAsync(Budget());

        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Uncertain, uncertain.Status);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, confirmed.Status);
        Assert.Equal(T0.AddSeconds(1), authority.State!.LastPossibleIssuanceUtc);
    }

    [Fact]
    public async Task Horizon_and_quarantine_are_derived_from_effective_options()
    {
        var time = new ManualTimeProvider(T0);
        var authority = new AuthenticationSecurityAuthorityFake(Open());
        var coordinator = Coordinator(authority, time, lifetime: 900, skew: 30, margin: 60);

        var close = await coordinator.CloseAsync(Budget());

        Assert.Equal(TimeSpan.FromSeconds(990), coordinator.CapturedHorizon);
        Assert.Equal(T0.AddSeconds(990), close.State!.QuarantineUntilUtc);
        Assert.Equal(900, close.State.CapturedLifetimeSeconds);
        Assert.Equal(30, close.State.CapturedSkewSeconds);
        Assert.Equal(60, close.State.CapturedOperationalMarginSeconds);
    }

    [Fact]
    public async Task Smaller_configuration_does_not_shorten_active_quarantine_and_larger_one_extends_it()
    {
        var time = new ManualTimeProvider(T0);
        var authority = new AuthenticationSecurityAuthorityFake(Closed(T0, lifetime: 900, skew: 30, margin: 60));
        var smaller = Coordinator(authority, time, lifetime: 900, skew: 0, margin: 0);

        var retained = await smaller.CloseAsync(Budget());
        var retainedUntil = retained.State!.QuarantineUntilUtc;
        var larger = Coordinator(authority, time, lifetime: 1000, skew: 30, margin: 60);
        var extended = await larger.CloseAsync(Budget());

        Assert.Equal(T0.AddSeconds(990), retainedUntil);
        Assert.Equal(T0.AddSeconds(1090), extended.State!.QuarantineUntilUtc);
        Assert.True(extended.State.QuarantineUntilUtc >= retainedUntil);
    }

    [Fact]
    public async Task Close_increments_generation_once_and_revision_for_each_shared_mutation()
    {
        var authority = new AuthenticationSecurityAuthorityFake(Open(generation: 4, revision: 9));
        var coordinator = Coordinator(authority, new ManualTimeProvider(T0));

        var first = await coordinator.CloseAsync(Budget());
        var second = await coordinator.CloseAsync(Budget());

        Assert.Equal(5, first.State!.Generation);
        Assert.Equal(10, first.State.Revision);
        Assert.Equal(5, second.State!.Generation);
        Assert.Equal(11, second.State.Revision);
    }

    [Fact]
    public async Task A_single_conflict_is_reread_and_merged_but_second_conflict_stays_closed()
    {
        var successfulAuthority = new AuthenticationSecurityAuthorityFake(Open());
        successfulAuthority.EnqueueCloseOutcome(AuthenticationSecurityAuthorityResultStatus.Conflict);
        var successful = Coordinator(successfulAuthority, new ManualTimeProvider(T0));
        var success = await successful.CloseAsync(Budget());

        var failedAuthority = new AuthenticationSecurityAuthorityFake(Open());
        failedAuthority.EnqueueCloseOutcome(AuthenticationSecurityAuthorityResultStatus.Conflict);
        failedAuthority.EnqueueCloseOutcome(AuthenticationSecurityAuthorityResultStatus.Conflict);
        var failed = Coordinator(failedAuthority, new ManualTimeProvider(T0));
        var conflict = await failed.CloseAsync(Budget());

        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, success.Status);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Conflict, conflict.Status);
        Assert.False((await EvaluateAsync(failed)).CanContinue);
    }

    [Fact]
    public async Task Potential_issuance_rereads_and_recomputes_once_after_a_conflict()
    {
        var time = new ManualTimeProvider(T0);
        var authority = new AuthenticationSecurityAuthorityFake(Open());
        authority.EnqueuePotentialIssuanceOutcome(AuthenticationSecurityAuthorityResultStatus.Conflict);
        var coordinator = Coordinator(authority, time);

        var lease = await coordinator.BeginEmissionAsync(Budget(time, 1_000));

        Assert.NotNull(lease);
        Assert.Equal(AuthenticationSecurityGlobalState.Open, authority.State!.State);
        Assert.Equal(1, authority.State.Revision);
        Assert.Equal(T0.AddSeconds(1), authority.State.LastPossibleIssuanceUtc);
    }

    [Fact]
    public async Task Quarantine_and_in_flight_emissions_prevent_reopen_then_explicit_CAS_reopens()
    {
        var time = new ManualTimeProvider(T0);
        var authority = new AuthenticationSecurityAuthorityFake(Open());
        var coordinator = Coordinator(authority, time);
        var lease = await coordinator.BeginEmissionAsync(Budget(time, 1_000));
        Assert.NotNull(lease);
        _ = await coordinator.CloseAsync(Budget());
        time.Advance(TimeSpan.FromSeconds(991));

        var withInFlight = await coordinator.TryReopenAfterQuarantineAsync(Budget());
        coordinator.CompleteEmission(lease!);
        var reopened = await coordinator.TryReopenAfterQuarantineAsync(Budget());

        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Unavailable, withInFlight.Status);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Available, reopened.Status);
        Assert.Equal(AuthenticationSecurityGlobalState.Open, authority.State!.State);
        Assert.True((await EvaluateAsync(coordinator)).CanContinue);
    }

    [Fact]
    public async Task Clock_moving_back_never_advances_reopen_and_clock_moving_forward_still_requires_authority_CAS()
    {
        var time = new ManualTimeProvider(T0);
        var authority = new AuthenticationSecurityAuthorityFake(Open());
        var coordinator = Coordinator(authority, time);
        _ = await coordinator.CloseAsync(Budget());
        time.SetUtcNow(T0.AddSeconds(-1));

        var backwards = await coordinator.TryReopenAfterQuarantineAsync(Budget());
        time.SetUtcNow(T0.AddHours(2));
        authority.EnqueueReopenOutcome(AuthenticationSecurityAuthorityResultStatus.Uncertain);
        var uncertain = await coordinator.TryReopenAfterQuarantineAsync(Budget());

        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Unavailable, backwards.Status);
        Assert.Equal(AuthenticationSecurityAuthorityResultStatus.Uncertain, uncertain.Status);
        Assert.False((await EvaluateAsync(coordinator)).CanContinue);
    }

    private static AuthenticationSecurityCoordinator Coordinator(AuthenticationSecurityAuthorityFake authority, TimeProvider time, int lifetime = 900, int skew = 30, int margin = 60) =>
        new(authority, new JwtOptions { LifetimeSeconds = lifetime, ClockSkewSeconds = skew }, new RedisOptions { RevocationOperationalMarginSeconds = margin }, time);

    private static AuthenticationSecurityAuthorityState Open(long generation = 0, long revision = 0) =>
        new(AuthenticationSecurityGlobalState.Open, generation, revision, null, null, null, null, null);

    private static AuthenticationSecurityAuthorityState Closed(DateTimeOffset lastPossible, long generation = 1, long revision = 1, int lifetime = 900, int skew = 30, int margin = 60) =>
        new(AuthenticationSecurityGlobalState.Closed, generation, revision, lastPossible, lastPossible.AddSeconds(lifetime + skew + margin), lifetime, skew, margin);

    private static RequestBudget Budget() => new(10_000_000, CancellationToken.None);
    private static RequestBudget Budget(TimeProvider clock, int milliseconds) => new(milliseconds, CancellationToken.None, clock);
    private static async ValueTask<AuthenticationSecurityDecision> EvaluateAsync(AuthenticationSecurityCoordinator coordinator)
    {
        using var budget = Budget();
        return await coordinator.EvaluateAsync(budget);
    }

    private sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset current = utcNow;
        private long timestamp;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => timestamp;
        public override DateTimeOffset GetUtcNow() => current;
        public void Advance(TimeSpan duration) { current = current.Add(duration); timestamp += duration.Ticks; }
        public void SetUtcNow(DateTimeOffset value) => current = value;
    }
}
