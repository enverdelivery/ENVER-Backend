namespace Enver.API.Features.Authentication.Application.Quotas;

public enum AuthenticationQuotaRecoveryStep
{
    StopAdmissionsAcrossInstances,
    ResolveInFlightOperations,
    EstablishLastPossibleConsumption,
    ReconstructAndValidateOrWaitApplicableWindow,
    ReconcileCapacity,
    ReopenOnlyWithVerifiableContinuity
}

/// <summary>
/// Contrato puro de recuperación, no limpieza, temporizador o prueba de cese global. Sin eviction activa,
/// LRU/LFU, reset, liberación por timeout, fallback local ni dependencia exclusiva de keyspace notifications.
/// Retención finita: último evento + ventana aplicable, sin margen de revocación ni refresh por rechazo.
/// El mantenimiento lazy/batch futuro será acotado/serializado y retirará estado residual antes de liberar su plaza.
/// Estado desalineado exige reconstrucción confiable o cierre por horizonte seguro; 020 no registra consumos de cuotas.
/// Los bool/instantes suministrados son precondiciones a ACREDITAR por 021, no evidencias creadas por este contrato.
/// </summary>
public static class AuthenticationQuotaRecoveryContract
{
    public static IReadOnlyList<AuthenticationQuotaRecoveryStep> RequiredSteps { get; } =
        Array.AsReadOnly(Enum.GetValues<AuthenticationQuotaRecoveryStep>());

    public static bool CanReleaseCapacity(AuthenticationQuotaPartitionState observed, AuthenticationQuotaPartitionState current,
        DateTimeOffset sharedNow, bool serializedWithConsumption)
    {
        ArgumentNullException.ThrowIfNull(observed);
        ArgumentNullException.ThrowIfNull(current);
        return serializedWithConsumption && observed.Identity == current.Identity &&
            observed.StateVersion == current.StateVersion && observed.Definition == current.Definition &&
            observed.LastEventUtc == current.LastEventUtc && observed.IsActiveAt(sharedNow) == false && current.IsActiveAt(sharedNow) == false;
    }

    /// <summary>
    /// Horizonte desde ÚLTIMO CONSUMO POSIBLE, no primera alerta/restart. Tiempo de autoridad compartida.
    /// No restaura eventos ni vacía el registro; true no ejecuta ni autoriza reapertura de otros controles.
    /// El riesgo de pérdida completamente silenciosa no observable sigue siendo residual de infraestructura.
    /// </summary>
    public static bool CanReopenAfterLostCounters(AuthenticationQuotaPolicyDescriptor previous, AuthenticationQuotaPolicyDescriptor next,
        DateTimeOffset? lastPossibleConsumption, DateTimeOffset? sharedNow, bool globalAdmissionsStopped,
        bool inFlightResolved, bool capacityReconciled)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(next);
        var horizon = previous.MaximumWindow > next.MaximumWindow ? previous.MaximumWindow : next.MaximumWindow;
        return globalAdmissionsStopped && inFlightResolved && capacityReconciled &&
            lastPossibleConsumption is { } last && sharedNow is { } now && now >= last && now - last >= horizon;
    }

    public static bool CanReopenAfterVerifiedReconstruction(bool globalAdmissionsStopped, bool inFlightResolved,
        bool reconstructionValidated, bool capacityReconciled) =>
        globalAdmissionsStopped && inFlightResolved && reconstructionValidated && capacityReconciled;

    /// <summary>053 exige drenar, sin current/previous activos; además reconciliar capacidad antes de reabrir.</summary>
    public static bool CanReopenAfterIdentityDrain(AuthenticationPseudonymizationTransition transition,
        DateTimeOffset? lastPossibleConsumption, DateTimeOffset? sharedNow, bool globalAdmissionsStopped,
        bool inFlightResolved, bool capacityReconciled)
    {
        ArgumentNullException.ThrowIfNull(transition);
        return globalAdmissionsStopped && inFlightResolved && capacityReconciled &&
            transition.HasDrainWindowElapsed(lastPossibleConsumption, sharedNow);
    }
}
