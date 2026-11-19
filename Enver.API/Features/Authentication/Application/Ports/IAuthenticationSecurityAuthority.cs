using Enver.API.Http;

namespace Enver.API.Features.Authentication.Application.Ports;

/// <summary>
/// Autoridad compartida de coordinación de seguridad. No representa revocaciones individuales.
/// </summary>
public interface IAuthenticationSecurityAuthority
{
    ValueTask<AuthenticationSecurityAuthorityReadResult> ReadAsync(RequestBudget requestBudget);

    ValueTask<AuthenticationSecurityAuthorityPotentialIssuanceResult> RegisterPotentialIssuanceAsync(
        RegisterPotentialIssuanceRequest request,
        RequestBudget requestBudget);

    ValueTask<AuthenticationSecurityAuthorityCloseResult> MergeClosedAsync(
        CloseAuthenticationSecurityRequest request,
        RequestBudget requestBudget);

    ValueTask<AuthenticationSecurityAuthorityReopenResult> TryReopenAsync(
        TryReopenAuthenticationSecurityRequest request,
        RequestBudget requestBudget);
}

public enum AuthenticationSecurityGlobalState
{
    Open,
    Closed
}

public enum AuthenticationSecurityAuthorityResultStatus
{
    Available,
    Conflict,
    Unavailable,
    Uncertain
}

/// <summary>
/// Estado operacional global. La ausencia del registro se expresa solo en el resultado de lectura.
/// </summary>
public sealed record AuthenticationSecurityAuthorityState
{
    public AuthenticationSecurityAuthorityState(
        AuthenticationSecurityGlobalState state,
        long generation,
        long revision,
        DateTimeOffset? lastPossibleIssuanceUtc,
        DateTimeOffset? quarantineUntilUtc,
        int? capturedLifetimeSeconds,
        int? capturedSkewSeconds,
        int? capturedOperationalMarginSeconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(generation);
        ArgumentOutOfRangeException.ThrowIfNegative(revision);

        if (state == AuthenticationSecurityGlobalState.Open)
        {
            if ((lastPossibleIssuanceUtc is not null && lastPossibleIssuanceUtc.Value.Offset != TimeSpan.Zero) || quarantineUntilUtc is not null ||
                capturedLifetimeSeconds is not null || capturedSkewSeconds is not null ||
                capturedOperationalMarginSeconds is not null)
            {
                throw new ArgumentException("Un estado Open no conserva metadatos de cuarentena.");
            }
        }
        else if (lastPossibleIssuanceUtc is null || quarantineUntilUtc is null ||
            capturedLifetimeSeconds is null || capturedSkewSeconds is null ||
            capturedOperationalMarginSeconds is null ||
            lastPossibleIssuanceUtc.Value.Offset != TimeSpan.Zero ||
            quarantineUntilUtc.Value.Offset != TimeSpan.Zero ||
            capturedLifetimeSeconds <= 0 || capturedSkewSeconds < 0 || capturedOperationalMarginSeconds < 0 ||
            quarantineUntilUtc < lastPossibleIssuanceUtc)
        {
            throw new ArgumentException("Un estado Closed requiere metadatos de cuarentena UTC válidos.");
        }

        if (state == AuthenticationSecurityGlobalState.Closed)
        {
            _ = checked(capturedLifetimeSeconds!.Value + capturedSkewSeconds!.Value + capturedOperationalMarginSeconds!.Value);
        }

        State = state;
        Generation = generation;
        Revision = revision;
        LastPossibleIssuanceUtc = lastPossibleIssuanceUtc;
        QuarantineUntilUtc = quarantineUntilUtc;
        CapturedLifetimeSeconds = capturedLifetimeSeconds;
        CapturedSkewSeconds = capturedSkewSeconds;
        CapturedOperationalMarginSeconds = capturedOperationalMarginSeconds;
    }

    public AuthenticationSecurityGlobalState State { get; }
    public long Generation { get; }
    public long Revision { get; }
    public DateTimeOffset? LastPossibleIssuanceUtc { get; }
    public DateTimeOffset? QuarantineUntilUtc { get; }
    public int? CapturedLifetimeSeconds { get; }
    public int? CapturedSkewSeconds { get; }
    public int? CapturedOperationalMarginSeconds { get; }

    public TimeSpan CapturedHorizon => State == AuthenticationSecurityGlobalState.Closed
        ? TimeSpan.FromSeconds(checked(CapturedLifetimeSeconds!.Value + CapturedSkewSeconds!.Value + CapturedOperationalMarginSeconds!.Value))
        : TimeSpan.Zero;
}

public sealed record CloseAuthenticationSecurityRequest
{
    public CloseAuthenticationSecurityRequest(
        long expectedGeneration,
        long expectedRevision,
        DateTimeOffset lastPossibleIssuanceUtc,
        int capturedLifetimeSeconds,
        int capturedSkewSeconds,
        int capturedOperationalMarginSeconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(expectedGeneration);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
        if (lastPossibleIssuanceUtc.Offset != TimeSpan.Zero || capturedLifetimeSeconds <= 0 ||
            capturedSkewSeconds < 0 || capturedOperationalMarginSeconds < 0)
        {
            throw new ArgumentException("La solicitud de cierre contiene valores de cuarentena inválidos.");
        }

        _ = checked(capturedLifetimeSeconds + capturedSkewSeconds + capturedOperationalMarginSeconds);
        ExpectedGeneration = expectedGeneration;
        ExpectedRevision = expectedRevision;
        LastPossibleIssuanceUtc = lastPossibleIssuanceUtc;
        CapturedLifetimeSeconds = capturedLifetimeSeconds;
        CapturedSkewSeconds = capturedSkewSeconds;
        CapturedOperationalMarginSeconds = capturedOperationalMarginSeconds;
    }

    public long ExpectedGeneration { get; }
    public long ExpectedRevision { get; }
    public DateTimeOffset LastPossibleIssuanceUtc { get; }
    public int CapturedLifetimeSeconds { get; }
    public int CapturedSkewSeconds { get; }
    public int CapturedOperationalMarginSeconds { get; }

    public TimeSpan CapturedHorizon => TimeSpan.FromSeconds(checked(CapturedLifetimeSeconds + CapturedSkewSeconds + CapturedOperationalMarginSeconds));
}

public sealed record RegisterPotentialIssuanceRequest
{
    public RegisterPotentialIssuanceRequest(long expectedGeneration, long expectedRevision, DateTimeOffset latestPossibleIssuanceUtc)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(expectedGeneration);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
        if (latestPossibleIssuanceUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("El límite de emisión potencial debe estar en UTC.", nameof(latestPossibleIssuanceUtc));
        }

        ExpectedGeneration = expectedGeneration;
        ExpectedRevision = expectedRevision;
        LatestPossibleIssuanceUtc = latestPossibleIssuanceUtc;
    }

    public long ExpectedGeneration { get; }
    public long ExpectedRevision { get; }
    public DateTimeOffset LatestPossibleIssuanceUtc { get; }
}

public sealed record TryReopenAuthenticationSecurityRequest
{
    public TryReopenAuthenticationSecurityRequest(long expectedGeneration, long expectedRevision, DateTimeOffset observedNowUtc)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(expectedGeneration);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
        if (observedNowUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("El instante de reapertura debe estar en UTC.", nameof(ObservedNowUtc));
        }

        ExpectedGeneration = expectedGeneration;
        ExpectedRevision = expectedRevision;
        ObservedNowUtc = observedNowUtc;
    }

    public long ExpectedGeneration { get; }
    public long ExpectedRevision { get; }
    public DateTimeOffset ObservedNowUtc { get; }
}

public readonly record struct AuthenticationSecurityAuthorityReadResult(
    AuthenticationSecurityAuthorityResultStatus Status,
    AuthenticationSecurityAuthorityState? State = null,
    BudgetCancellationCause CancellationCause = BudgetCancellationCause.None)
{
    public bool RecordExists => State is not null;
    public bool ResultUncertain => Status == AuthenticationSecurityAuthorityResultStatus.Uncertain;
}

public readonly record struct AuthenticationSecurityAuthorityCloseResult(
    AuthenticationSecurityAuthorityResultStatus Status,
    AuthenticationSecurityAuthorityState? State = null,
    BudgetCancellationCause CancellationCause = BudgetCancellationCause.None)
{
    public bool ResultUncertain => Status == AuthenticationSecurityAuthorityResultStatus.Uncertain;
}

public readonly record struct AuthenticationSecurityAuthorityPotentialIssuanceResult(
    AuthenticationSecurityAuthorityResultStatus Status,
    AuthenticationSecurityAuthorityState? State = null,
    BudgetCancellationCause CancellationCause = BudgetCancellationCause.None)
{
    public bool ResultUncertain => Status == AuthenticationSecurityAuthorityResultStatus.Uncertain;
}

public readonly record struct AuthenticationSecurityAuthorityReopenResult(
    AuthenticationSecurityAuthorityResultStatus Status,
    AuthenticationSecurityAuthorityState? State = null,
    BudgetCancellationCause CancellationCause = BudgetCancellationCause.None)
{
    public bool ResultUncertain => Status == AuthenticationSecurityAuthorityResultStatus.Uncertain;
}
