using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;

namespace Enver.Testing;

/// <summary>Fake compartido para pruebas; modela CAS, no es un adapter productivo.</summary>
public sealed class AuthenticationSecurityAuthorityFake : IAuthenticationSecurityAuthority
{
    private readonly object sync = new();
    private AuthenticationSecurityAuthorityState? state;
    private readonly Queue<AuthenticationSecurityAuthorityResultStatus> readOutcomes = [];
    private readonly Queue<AuthenticationSecurityAuthorityResultStatus> closeOutcomes = [];
    private readonly Queue<AuthenticationSecurityAuthorityResultStatus> reopenOutcomes = [];

    public AuthenticationSecurityAuthorityFake(AuthenticationSecurityAuthorityState? initialState = null) => state = initialState;

    public AuthenticationSecurityAuthorityState? State
    {
        get { lock (sync) return state; }
    }

    public void EnqueueReadOutcome(AuthenticationSecurityAuthorityResultStatus status) => Enqueue(readOutcomes, status);
    public void EnqueueCloseOutcome(AuthenticationSecurityAuthorityResultStatus status) => Enqueue(closeOutcomes, status);
    public void EnqueueReopenOutcome(AuthenticationSecurityAuthorityResultStatus status) => Enqueue(reopenOutcomes, status);

    public ValueTask<AuthenticationSecurityAuthorityReadResult> ReadAsync(RequestBudget requestBudget)
    {
        lock (sync)
        {
            var outcome = Dequeue(readOutcomes);
            return ValueTask.FromResult(new AuthenticationSecurityAuthorityReadResult(outcome, outcome == AuthenticationSecurityAuthorityResultStatus.Available ? state : null));
        }
    }

    public ValueTask<AuthenticationSecurityAuthorityCloseResult> MergeClosedAsync(CloseAuthenticationSecurityRequest request, RequestBudget requestBudget)
    {
        lock (sync)
        {
            var outcome = Dequeue(closeOutcomes);
            if (outcome != AuthenticationSecurityAuthorityResultStatus.Available)
            {
                return ValueTask.FromResult(new AuthenticationSecurityAuthorityCloseResult(outcome));
            }

            if (state is null || state.Generation != request.ExpectedGeneration || state.Revision != request.ExpectedRevision)
            {
                return ValueTask.FromResult(new AuthenticationSecurityAuthorityCloseResult(AuthenticationSecurityAuthorityResultStatus.Conflict));
            }

            var nextGeneration = state.State == AuthenticationSecurityGlobalState.Open ? checked(state.Generation + 1) : state.Generation;
            var (lifetime, skew, margin) = SelectHorizon(state, request);
            var lastPossible = state.LastPossibleIssuanceUtc is { } existing && existing > request.LastPossibleIssuanceUtc
                ? existing : request.LastPossibleIssuanceUtc;
            var candidateQuarantine = lastPossible.Add(TimeSpan.FromSeconds(checked(lifetime + skew + margin)));
            var quarantineUntil = state.QuarantineUntilUtc is { } existingQuarantine && existingQuarantine > candidateQuarantine
                ? existingQuarantine : candidateQuarantine;
            state = new AuthenticationSecurityAuthorityState(AuthenticationSecurityGlobalState.Closed, nextGeneration, checked(state.Revision + 1),
                lastPossible, quarantineUntil, lifetime, skew, margin);
            return ValueTask.FromResult(new AuthenticationSecurityAuthorityCloseResult(AuthenticationSecurityAuthorityResultStatus.Available, state));
        }
    }

    public ValueTask<AuthenticationSecurityAuthorityReopenResult> TryReopenAsync(TryReopenAuthenticationSecurityRequest request, RequestBudget requestBudget)
    {
        lock (sync)
        {
            var outcome = Dequeue(reopenOutcomes);
            if (outcome != AuthenticationSecurityAuthorityResultStatus.Available)
            {
                return ValueTask.FromResult(new AuthenticationSecurityAuthorityReopenResult(outcome));
            }

            if (state is not { State: AuthenticationSecurityGlobalState.Closed } closed || closed.Generation != request.ExpectedGeneration ||
                closed.Revision != request.ExpectedRevision || request.ObservedNowUtc < closed.QuarantineUntilUtc)
            {
                return ValueTask.FromResult(new AuthenticationSecurityAuthorityReopenResult(AuthenticationSecurityAuthorityResultStatus.Conflict));
            }

            state = new AuthenticationSecurityAuthorityState(AuthenticationSecurityGlobalState.Open, closed.Generation, checked(closed.Revision + 1), null, null, null, null, null);
            return ValueTask.FromResult(new AuthenticationSecurityAuthorityReopenResult(AuthenticationSecurityAuthorityResultStatus.Available, state));
        }
    }

    private void Enqueue(Queue<AuthenticationSecurityAuthorityResultStatus> outcomes, AuthenticationSecurityAuthorityResultStatus status)
    {
        lock (sync) outcomes.Enqueue(status);
    }

    private static AuthenticationSecurityAuthorityResultStatus Dequeue(Queue<AuthenticationSecurityAuthorityResultStatus> outcomes) =>
        outcomes.Count == 0 ? AuthenticationSecurityAuthorityResultStatus.Available : outcomes.Dequeue();

    private static (int Lifetime, int Skew, int Margin) SelectHorizon(AuthenticationSecurityAuthorityState current, CloseAuthenticationSecurityRequest request)
    {
        if (current.State == AuthenticationSecurityGlobalState.Closed && current.CapturedHorizon >= request.CapturedHorizon)
        {
            return (current.CapturedLifetimeSeconds!.Value, current.CapturedSkewSeconds!.Value, current.CapturedOperationalMarginSeconds!.Value);
        }

        return (request.CapturedLifetimeSeconds, request.CapturedSkewSeconds, request.CapturedOperationalMarginSeconds);
    }
}
