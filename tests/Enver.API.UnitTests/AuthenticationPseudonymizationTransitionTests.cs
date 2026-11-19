using Enver.API.Features.Authentication.Application.Quotas;

namespace Enver.API.UnitTests;

public sealed class AuthenticationPseudonymizationTransitionTests
{
    [Theory]
    [InlineData(7, 11, 11)]
    [InlineData(1800, 1200, 1800)]
    [InlineData(1200, 1800, 1800)]
    public void Horizon_is_derived_from_both_policies(int oldWindow, int newWindow, int expected)
    {
        var oldPolicy = QuotaTestData.Policy("old", oldWindow);
        var newPolicy = QuotaTestData.Policy("new", newWindow);
        var transition = new AuthenticationPseudonymizationTransition(oldPolicy, newPolicy, AuthenticationPseudonymizationChange.Key);
        Assert.Equal(TimeSpan.FromSeconds(expected), transition.DrainWindow);
        Assert.Same(oldPolicy, transition.OldPolicy);
        Assert.Same(newPolicy, transition.NewPolicy);
    }

    [Theory]
    [InlineData(AuthenticationPseudonymizationChange.Key)]
    [InlineData(AuthenticationPseudonymizationChange.CanonicalRepresentation)]
    public void Key_and_representation_domain_version_changes_require_same_drain_without_changing_revision(AuthenticationPseudonymizationChange change)
    {
        var policy = AuthenticationQuotaPolicyDescriptor.AcademicInitial;
        var transition = new AuthenticationPseudonymizationTransition(policy, policy, change);
        Assert.Equal(change, transition.Change);
        Assert.Same(policy, transition.OldPolicy);
        Assert.Same(policy, transition.NewPolicy);
        Assert.Equal(policy.MaximumWindow, transition.DrainWindow);
        Assert.Equal(AuthenticationQuotaPolicyTransition.RequiredSteps, transition.RequiredSteps);
        Assert.Equal(AuthenticationQuotaPolicyTransitionStep.StopNewAdmissionsAcrossInstances, transition.RequiredSteps[1]);
        Assert.Equal(AuthenticationQuotaPolicyTransitionStep.ResolveInFlightOperations, transition.RequiredSteps[2]);
        Assert.Equal(AuthenticationQuotaPolicyTransitionStep.EstablishLastPossibleConsumption, transition.RequiredSteps[3]);
        Assert.Equal(AuthenticationQuotaPolicyTransitionStep.ReopenOnlyWithVerifiableContinuity, transition.RequiredSteps[^1]);
    }

    [Fact]
    public void Horizon_cannot_elapse_before_last_possible_consumption_or_start_from_deployment()
    {
        var policy = QuotaTestData.Policy("same-revision", 120);
        var transition = new AuthenticationPseudonymizationTransition(policy, policy, AuthenticationPseudonymizationChange.Key);
        var deployment = new DateTimeOffset(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);
        var lastPossibleConsumption = deployment.AddSeconds(30);
        Assert.False(transition.HasDrainWindowElapsed(lastPossibleConsumption, deployment.AddSeconds(120)));
        Assert.False(transition.HasDrainWindowElapsed(lastPossibleConsumption, lastPossibleConsumption.AddSeconds(120).AddTicks(-1)));
        Assert.True(transition.HasDrainWindowElapsed(lastPossibleConsumption, lastPossibleConsumption.AddSeconds(120)));
        Assert.False(transition.HasDrainWindowElapsed(null, deployment.AddDays(1)));
        Assert.False(transition.HasDrainWindowElapsed(lastPossibleConsumption, null));
        Assert.False(transition.HasDrainWindowElapsed(lastPossibleConsumption, deployment));
        // No Activate/Reopen/Reset runtime: el cálculo aislado no acredita coordinación global.
        Assert.DoesNotContain(typeof(AuthenticationPseudonymizationTransition).GetMethods(), method => method.Name is "Activate" or "Reopen" or "Reset");
    }

    [Fact]
    public void Invalid_transition_kind_is_rejected() => Assert.Throws<ArgumentOutOfRangeException>(() =>
        new AuthenticationPseudonymizationTransition(AuthenticationQuotaPolicyDescriptor.AcademicInitial,
            AuthenticationQuotaPolicyDescriptor.AcademicInitial, (AuthenticationPseudonymizationChange)999));
}
