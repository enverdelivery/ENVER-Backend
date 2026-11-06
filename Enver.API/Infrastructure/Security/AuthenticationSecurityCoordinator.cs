using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Http;

namespace Enver.API.Infrastructure.Security;

/// <summary>
/// Coordina admisión fail-closed frente a una autoridad externa. Redis no puede reabrir este estado.
/// </summary>
public sealed class AuthenticationSecurityCoordinator : IAuthenticationSecurityState
{
    private readonly IAuthenticationSecurityAuthority authority;
    private readonly TimeProvider clock;
    private readonly int lifetimeSeconds;
    private readonly int skewSeconds;
    private readonly int operationalMarginSeconds;
    private readonly object sync = new();
    private readonly Dictionary<Guid, DateTimeOffset> emissions = [];
    private DateTimeOffset? localLastPossibleIssuanceUtc;
    private long confirmedClosedGeneration = -1;
    private long confirmedClosedRevision = -1;
    private int closed;

    public AuthenticationSecurityCoordinator(
        IAuthenticationSecurityAuthority authority,
        JwtOptions jwtOptions,
        RedisOptions redisOptions,
        TimeProvider? clock = null)
    {
        this.authority = authority ?? throw new ArgumentNullException(nameof(authority));
        ArgumentNullException.ThrowIfNull(jwtOptions);
        ArgumentNullException.ThrowIfNull(redisOptions);
        if (jwtOptions.LifetimeSeconds <= 0 || jwtOptions.ClockSkewSeconds is null or < 0 || redisOptions.RevocationOperationalMarginSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(jwtOptions), "Las opciones efectivas de cuarentena deben ser válidas.");
        }

        lifetimeSeconds = jwtOptions.LifetimeSeconds;
        skewSeconds = jwtOptions.ClockSkewSeconds.Value;
        operationalMarginSeconds = redisOptions.RevocationOperationalMarginSeconds;
        _ = checked(lifetimeSeconds + skewSeconds + operationalMarginSeconds);
        this.clock = clock ?? TimeProvider.System;
    }

    public TimeSpan CapturedHorizon => TimeSpan.FromSeconds(checked(lifetimeSeconds + skewSeconds + operationalMarginSeconds));

    /// <summary>Cierra el latch antes de cualquier E/S. Una señal Redis sana posterior no lo abre.</summary>
    public async ValueTask<AuthenticationSecurityAuthorityCloseResult> CloseAsync(RequestBudget requestBudget)
    {
        ArgumentNullException.ThrowIfNull(requestBudget);
        CloseLocalLatch();

        var read = await authority.ReadAsync(requestBudget).ConfigureAwait(false);
        if (read.Status != AuthenticationSecurityAuthorityResultStatus.Available || read.State is null)
        {
            return new AuthenticationSecurityAuthorityCloseResult(read.Status == AuthenticationSecurityAuthorityResultStatus.Available
                ? AuthenticationSecurityAuthorityResultStatus.Unavailable : read.Status, null, read.CancellationCause);
        }

        return await MergeClosedWithSingleConflictResolutionAsync(read.State, requestBudget).ConfigureAwait(false);
    }

    public async ValueTask<AuthenticationSecurityDecision> EvaluateAsync(RequestBudget requestBudget)
    {
        ArgumentNullException.ThrowIfNull(requestBudget);
        var read = await authority.ReadAsync(requestBudget).ConfigureAwait(false);
        if (read.Status != AuthenticationSecurityAuthorityResultStatus.Available)
        {
            CloseLocalLatch();
            return Deny(read.Status, read.CancellationCause);
        }

        if (read.State is null || read.State.State == AuthenticationSecurityGlobalState.Closed)
        {
            CloseLocalLatch();
            if (read.State is not null)
            {
                RememberClosed(read.State);
            }

            return new AuthenticationSecurityDecision(AuthenticationSecurityAdmission.Unavailable);
        }

        if (CanObserveExplicitReopen(read.State))
        {
            Interlocked.Exchange(ref closed, 0);
        }

        return Volatile.Read(ref closed) == 0
            ? new AuthenticationSecurityDecision(AuthenticationSecurityAdmission.Verifiable)
            : new AuthenticationSecurityDecision(AuthenticationSecurityAdmission.Unavailable);
    }

    /// <summary>Registra primero el límite de emisión potencial en la autoridad compartida.</summary>
    public async ValueTask<AuthenticationSecurityEmissionLease?> BeginEmissionAsync(RequestBudget requestBudget)
    {
        ArgumentNullException.ThrowIfNull(requestBudget);
        if (Volatile.Read(ref closed) != 0)
        {
            return null;
        }

        var read = await authority.ReadAsync(requestBudget).ConfigureAwait(false);
        if (read.Status != AuthenticationSecurityAuthorityResultStatus.Available || read.State is not { State: AuthenticationSecurityGlobalState.Open } state)
        {
            CloseLocalLatch();
            return null;
        }

        DateTimeOffset latestPossibleIssuanceUtc;
        try
        {
            if (requestBudget.Remaining <= TimeSpan.Zero)
            {
                CloseLocalLatch();
                return null;
            }

            latestPossibleIssuanceUtc = clock.GetUtcNow().Add(requestBudget.Remaining);
        }
        catch (ArgumentOutOfRangeException)
        {
            CloseLocalLatch();
            return null;
        }

        var registered = await RegisterPotentialIssuanceWithSingleConflictResolutionAsync(state, latestPossibleIssuanceUtc, requestBudget).ConfigureAwait(false);
        if (registered.Status != AuthenticationSecurityAuthorityResultStatus.Available || registered.State is not { State: AuthenticationSecurityGlobalState.Open })
        {
            CloseLocalLatch();
            return null;
        }

        lock (sync)
        {
            if (Volatile.Read(ref closed) != 0)
            {
                return null;
            }

            var id = Guid.NewGuid();
            emissions.Add(id, latestPossibleIssuanceUtc);
            localLastPossibleIssuanceUtc = Max(localLastPossibleIssuanceUtc, latestPossibleIssuanceUtc);
            return new AuthenticationSecurityEmissionLease(this, id, latestPossibleIssuanceUtc);
        }
    }

    public void CompleteEmission(AuthenticationSecurityEmissionLease lease) => EndEmission(lease);

    public void FailEmission(AuthenticationSecurityEmissionLease lease) => EndEmission(lease);

    /// <summary>Operación de control explícita; no se invoca desde señales de conectividad Redis.</summary>
    public async ValueTask<AuthenticationSecurityAuthorityReopenResult> TryReopenAfterQuarantineAsync(RequestBudget requestBudget)
    {
        ArgumentNullException.ThrowIfNull(requestBudget);
        var read = await authority.ReadAsync(requestBudget).ConfigureAwait(false);
        if (read.Status != AuthenticationSecurityAuthorityResultStatus.Available || read.State is not { State: AuthenticationSecurityGlobalState.Closed } state)
        {
            return new AuthenticationSecurityAuthorityReopenResult(read.Status == AuthenticationSecurityAuthorityResultStatus.Available
                ? AuthenticationSecurityAuthorityResultStatus.Unavailable : read.Status, null, read.CancellationCause);
        }

        if (HasEmissionsInFlight() || clock.GetUtcNow() < state.QuarantineUntilUtc)
        {
            return new AuthenticationSecurityAuthorityReopenResult(AuthenticationSecurityAuthorityResultStatus.Unavailable);
        }

        return await ReopenWithSingleConflictResolutionAsync(state, requestBudget).ConfigureAwait(false);
    }

    private async ValueTask<AuthenticationSecurityAuthorityCloseResult> MergeClosedWithSingleConflictResolutionAsync(
        AuthenticationSecurityAuthorityState state,
        RequestBudget requestBudget)
    {
        var first = await authority.MergeClosedAsync(CreateCloseRequest(state), requestBudget).ConfigureAwait(false);
        if (first.Status != AuthenticationSecurityAuthorityResultStatus.Conflict)
        {
            RememberClosedFromResult(first);
            return first;
        }

        var reread = await authority.ReadAsync(requestBudget).ConfigureAwait(false);
        if (reread.Status != AuthenticationSecurityAuthorityResultStatus.Available || reread.State is null)
        {
            return new AuthenticationSecurityAuthorityCloseResult(reread.Status == AuthenticationSecurityAuthorityResultStatus.Available
                ? AuthenticationSecurityAuthorityResultStatus.Unavailable : reread.Status, null, reread.CancellationCause);
        }

        var second = await authority.MergeClosedAsync(CreateCloseRequest(reread.State), requestBudget).ConfigureAwait(false);
        RememberClosedFromResult(second);
        return second;
    }

    private async ValueTask<AuthenticationSecurityAuthorityPotentialIssuanceResult> RegisterPotentialIssuanceWithSingleConflictResolutionAsync(
        AuthenticationSecurityAuthorityState state,
        DateTimeOffset latestPossibleIssuanceUtc,
        RequestBudget requestBudget)
    {
        var first = await authority.RegisterPotentialIssuanceAsync(
            new RegisterPotentialIssuanceRequest(state.Generation, state.Revision, latestPossibleIssuanceUtc), requestBudget).ConfigureAwait(false);
        if (first.Status != AuthenticationSecurityAuthorityResultStatus.Conflict)
        {
            return first;
        }

        var reread = await authority.ReadAsync(requestBudget).ConfigureAwait(false);
        if (reread.Status != AuthenticationSecurityAuthorityResultStatus.Available || reread.State is not { State: AuthenticationSecurityGlobalState.Open } current)
        {
            return new AuthenticationSecurityAuthorityPotentialIssuanceResult(
                reread.Status == AuthenticationSecurityAuthorityResultStatus.Available
                    ? AuthenticationSecurityAuthorityResultStatus.Unavailable : reread.Status,
                null,
                reread.CancellationCause);
        }

        try
        {
            if (requestBudget.Remaining <= TimeSpan.Zero)
            {
                return new AuthenticationSecurityAuthorityPotentialIssuanceResult(AuthenticationSecurityAuthorityResultStatus.Unavailable);
            }

            latestPossibleIssuanceUtc = clock.GetUtcNow().Add(requestBudget.Remaining);
        }
        catch (ArgumentOutOfRangeException)
        {
            return new AuthenticationSecurityAuthorityPotentialIssuanceResult(AuthenticationSecurityAuthorityResultStatus.Unavailable);
        }

        return await authority.RegisterPotentialIssuanceAsync(
            new RegisterPotentialIssuanceRequest(current.Generation, current.Revision, latestPossibleIssuanceUtc), requestBudget).ConfigureAwait(false);
    }

    private async ValueTask<AuthenticationSecurityAuthorityReopenResult> ReopenWithSingleConflictResolutionAsync(
        AuthenticationSecurityAuthorityState state,
        RequestBudget requestBudget)
    {
        var first = await authority.TryReopenAsync(CreateReopenRequest(state), requestBudget).ConfigureAwait(false);
        if (first.Status != AuthenticationSecurityAuthorityResultStatus.Conflict)
        {
            OpenOnlyAfterConfirmedResult(first);
            return first;
        }

        var reread = await authority.ReadAsync(requestBudget).ConfigureAwait(false);
        if (reread.Status != AuthenticationSecurityAuthorityResultStatus.Available || reread.State is not { State: AuthenticationSecurityGlobalState.Closed } current ||
            HasEmissionsInFlight() || clock.GetUtcNow() < current.QuarantineUntilUtc)
        {
            return new AuthenticationSecurityAuthorityReopenResult(reread.Status == AuthenticationSecurityAuthorityResultStatus.Available
                ? AuthenticationSecurityAuthorityResultStatus.Unavailable : reread.Status, null, reread.CancellationCause);
        }

        var second = await authority.TryReopenAsync(CreateReopenRequest(current), requestBudget).ConfigureAwait(false);
        OpenOnlyAfterConfirmedResult(second);
        return second;
    }

    private CloseAuthenticationSecurityRequest CreateCloseRequest(AuthenticationSecurityAuthorityState state)
    {
        DateTimeOffset possible;
        lock (sync)
        {
            possible = Max(localLastPossibleIssuanceUtc, clock.GetUtcNow());
            localLastPossibleIssuanceUtc = Max(localLastPossibleIssuanceUtc, possible);
        }

        return new CloseAuthenticationSecurityRequest(
            state.Generation,
            state.Revision,
            possible,
            lifetimeSeconds,
            skewSeconds,
            operationalMarginSeconds);
    }

    private TryReopenAuthenticationSecurityRequest CreateReopenRequest(AuthenticationSecurityAuthorityState state) =>
        new(state.Generation, state.Revision, clock.GetUtcNow());

    private void RememberClosedFromResult(AuthenticationSecurityAuthorityCloseResult result)
    {
        if (result.Status == AuthenticationSecurityAuthorityResultStatus.Available && result.State is { State: AuthenticationSecurityGlobalState.Closed } state)
        {
            RememberClosed(state);
        }
    }

    private void OpenOnlyAfterConfirmedResult(AuthenticationSecurityAuthorityReopenResult result)
    {
        if (result.Status == AuthenticationSecurityAuthorityResultStatus.Available && result.State is { State: AuthenticationSecurityGlobalState.Open })
        {
            Interlocked.Exchange(ref closed, 0);
        }
    }

    private bool CanObserveExplicitReopen(AuthenticationSecurityAuthorityState state)
    {
        lock (sync)
        {
            return confirmedClosedGeneration >= 0 && state.Generation == confirmedClosedGeneration &&
                state.Revision > confirmedClosedRevision;
        }
    }

    private void RememberClosed(AuthenticationSecurityAuthorityState state)
    {
        lock (sync)
        {
            confirmedClosedGeneration = state.Generation;
            confirmedClosedRevision = state.Revision;
        }
    }

    private bool HasEmissionsInFlight()
    {
        lock (sync)
        {
            return emissions.Count != 0;
        }
    }

    private void EndEmission(AuthenticationSecurityEmissionLease lease)
    {
        ArgumentNullException.ThrowIfNull(lease);
        if (!ReferenceEquals(lease.Owner, this))
        {
            throw new ArgumentException("La emisión no pertenece a este coordinador.", nameof(lease));
        }

        lock (sync)
        {
            emissions.Remove(lease.Id);
        }
    }

    private void CloseLocalLatch() => Interlocked.Exchange(ref closed, 1);

    private static AuthenticationSecurityDecision Deny(AuthenticationSecurityAuthorityResultStatus status, BudgetCancellationCause cause) =>
        new(status == AuthenticationSecurityAuthorityResultStatus.Uncertain
            ? AuthenticationSecurityAdmission.Uncertain
            : AuthenticationSecurityAdmission.Unavailable, cause);

    private static DateTimeOffset Max(DateTimeOffset? first, DateTimeOffset second) => first is null || second > first ? second : first.Value;
}

/// <summary>Handle opaco para completar o fallar una emisión futura, sin representar JWT alguno.</summary>
public sealed class AuthenticationSecurityEmissionLease
{
    internal AuthenticationSecurityEmissionLease(AuthenticationSecurityCoordinator owner, Guid id, DateTimeOffset latestPossibleIssuanceUtc)
    {
        Owner = owner;
        Id = id;
        LatestPossibleIssuanceUtc = latestPossibleIssuanceUtc;
    }

    internal AuthenticationSecurityCoordinator Owner { get; }
    internal Guid Id { get; }
    public DateTimeOffset LatestPossibleIssuanceUtc { get; }
}
