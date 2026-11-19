namespace Enver.API.Features.Authentication.Application.Quotas;

/// <summary>
/// Observación GLOBAL en un único contexto ambiente/finalidad/configuración/tiempo. No contador local.
/// Las plazas residuales todavía no liberadas con seguridad siguen cargadas, aunque ya no tengan eventos vivos.
/// IsVerifiable también exige acreditar que la ausencia de una observación requerida significa ausencia/inactividad,
/// no pérdida de datos. Un registro vacío tras restart/flush no acredita capacidad libre.
/// </summary>
public sealed class AuthenticationQuotaCapacityState
{
    public AuthenticationQuotaCapacityState(long activePartitionCount, bool isVerifiable, long pendingReleasePartitionCount = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(activePartitionCount);
        ArgumentOutOfRangeException.ThrowIfNegative(pendingReleasePartitionCount);
        ActivePartitionCount = activePartitionCount;
        PendingReleasePartitionCount = pendingReleasePartitionCount;
        AccountedPartitionCount = checked(activePartitionCount + pendingReleasePartitionCount);
        IsVerifiable = isVerifiable;
    }

    public long ActivePartitionCount { get; }
    public long PendingReleasePartitionCount { get; }
    public long AccountedPartitionCount { get; }
    public bool IsVerifiable { get; }
    public override string ToString() => nameof(AuthenticationQuotaCapacityState);
}

/// <summary>
/// Cálculo puro, NO admisión, reserva, creación, consumo ni evidencia Redis. Failure null solo indica
/// que capacidad no bloquea. NewPartitions es intención conjunta completa o vacía, nunca efectos aplicados.
/// </summary>
public sealed class AuthenticationQuotaCapacityEvaluation
{
    internal AuthenticationQuotaCapacityEvaluation(bool isVerifiable, int? existingActivePartitionCount, int? newPartitionCount,
        AuthenticationQuotaConsumeResult? failure, IEnumerable<AuthenticationQuotaPartitionIdentity> newPartitions)
    {
        IsVerifiable = isVerifiable;
        ExistingActivePartitionCount = existingActivePartitionCount;
        NewPartitionCount = newPartitionCount;
        Failure = failure;
        NewPartitions = Array.AsReadOnly(newPartitions.ToArray());
    }

    public bool IsVerifiable { get; }
    public int? ExistingActivePartitionCount { get; }
    public int? NewPartitionCount { get; }
    public AuthenticationQuotaConsumeResult? Failure { get; }
    public IReadOnlyList<AuthenticationQuotaPartitionIdentity> NewPartitions { get; }
    public bool CapacitySufficient => Failure is null;
    public override string ToString() => nameof(AuthenticationQuotaCapacityEvaluation);
}

/// <summary>
/// Especificación ejecutable pura de 054. En 021, verificar snapshot/topología/confianza, observar actividad,
/// evaluar cuotas/capacidad, crear/registrar TODOS los ámbitos nuevos, insertar eventos y actualizar retención
/// deberán compartir la operación atómica de 052, incluido el registro global. Sin reservas previas,
/// contador por instancia, transacciones entre almacenes, retry ciego, compensación o fallback local.
/// La topología debe soportar también el registro, no solo los buckets. 054 no implementa Lua ni E/S.
/// </summary>
public static class AuthenticationQuotaCapacityContract
{
    public static AuthenticationQuotaCapacityEvaluation Evaluate(AuthenticationQuotaCapacityDescriptor descriptor,
        AuthenticationQuotaCapacityState state, IEnumerable<AuthenticationQuotaPartitionIdentity> requiredPartitions,
        IEnumerable<AuthenticationQuotaPartitionState> observedPartitions, DateTimeOffset sharedNow)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(requiredPartitions);
        ArgumentNullException.ThrowIfNull(observedPartitions);
        if (!state.IsVerifiable) return Unknown();
        var required = requiredPartitions.ToArray();
        if (required.Length == 0 || required.Any(identity => !IsValidIdentity(identity))) return Unknown();
        required = required.Distinct().ToArray();
        var observations = new Dictionary<AuthenticationQuotaPartitionIdentity, AuthenticationQuotaPartitionState>();
        foreach (var observation in observedPartitions)
        {
            if (observation is null) return Unknown();
            // Observaciones ajenas no se usan como copia local de todo el registro global.
            if (!required.Contains(observation.Identity)) continue;
            if (observations.TryGetValue(observation.Identity, out var previous) &&
                (previous.StateVersion != observation.StateVersion || previous.LastEventUtc != observation.LastEventUtc ||
                 previous.Definition != observation.Definition || previous.IsVerifiable != observation.IsVerifiable)) return Unknown();
            observations[observation.Identity] = observation;
        }

        var existing = 0;
        var newPartitions = new List<AuthenticationQuotaPartitionIdentity>();
        foreach (var identity in required)
        {
            if (observations.TryGetValue(identity, out var observation))
            {
                var active = observation.IsActiveAt(sharedNow);
                if (active is null) return Unknown();
                if (active.Value) { existing++; continue; }
            }
            newPartitions.Add(identity);
        }
        if (existing > state.ActivePartitionCount) return Unknown();
        // N=0 permite ámbitos existentes incluso tras reducción por debajo de ocupación.
        // La resta evita overflow con un conteo global conservador grande.
        var sufficient = newPartitions.Count == 0 || state.AccountedPartitionCount <= (long)descriptor.MaxActivePartitions - newPartitions.Count;
        return new(true, existing, newPartitions.Count, sufficient ? null : Unavailable(),
            sufficient ? newPartitions : Array.Empty<AuthenticationQuotaPartitionIdentity>());
    }

    /// <summary>Adaptación contractual a los scopes completos de 052; no despacha ni omite su preflight.</summary>
    public static AuthenticationQuotaCapacityEvaluation EvaluateRequest(AuthenticationQuotaCapacityDescriptor descriptor,
        AuthenticationQuotaCapacityState state, AuthenticationQuotaConsumeRequest request,
        IEnumerable<AuthenticationQuotaPartitionState> observedPartitions, DateTimeOffset sharedNow)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(observedPartitions);
        var observed = observedPartitions.ToArray();
        if (observed.Any(partition => partition is null ||
            request.Scopes.Any(scope => request.GetPartitionIdentity(scope) == partition.Identity && scope.Definition != partition.Definition)))
            return Unknown();
        return Evaluate(descriptor, state, request.Scopes.Select(request.GetPartitionIdentity), observed, sharedNow);
    }

    /// <summary>
    /// Sin estado verificable no se inventa 429. Si las cuotas bloquean, preceden a falta de capacidad.
    /// null NO admite: permite a 021 efectuar la mutación conjunta, dentro del mismo contexto atómico.
    /// La intención NewPartitions calculada antes NO se persiste si este método devuelve un fallo.
    /// </summary>
    public static AuthenticationQuotaConsumeResult? GetFailureAfterQuotaEvaluation(AuthenticationQuotaCapacityEvaluation capacity,
        bool quotaStateVerifiable, TimeSpan? blockingRetryAfter)
    {
        ArgumentNullException.ThrowIfNull(capacity);
        if (!quotaStateVerifiable || !capacity.IsVerifiable || blockingRetryAfter <= TimeSpan.Zero) return Unavailable();
        return blockingRetryAfter is { } delay
            ? new(AuthenticationQuotaStoreStatus.Rejected, delay) : capacity.Failure;
    }

    /// <summary>Comparar snapshot activo acreditado, no solo strings de revisión ni flags locales.</summary>
    public static AuthenticationQuotaConsumeResult? GetPreDispatchFailure(AuthenticationQuotaConsumeRequest request,
        AuthenticationQuotaConfigurationSnapshot configured, AuthenticationQuotaConfigurationSnapshot? active,
        bool topologySupportsSingleAtomicExecution)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(configured);
        return active is null || !configured.Matches(active) ? Unavailable() :
            AuthenticationQuotaEvaluationContract.GetPreDispatchFailure(request, active.Policy, topologySupportsSingleAtomicExecution);
    }

    internal static bool IsValidIdentity(AuthenticationQuotaPartitionIdentity? identity) => identity is not null &&
        Enum.IsDefined(identity.Operation) && Enum.IsDefined(identity.ScopeType) &&
        AuthenticationQuotaPolicyDescriptor.RequiredScopes(identity.Operation).Contains(identity.ScopeType) &&
        QuotaIdentifiers.IsValid(identity.OpaqueId);

    private static AuthenticationQuotaConsumeResult Unavailable() => new(AuthenticationQuotaStoreStatus.Unavailable);
    private static AuthenticationQuotaCapacityEvaluation Unknown() => new(false, null, null, Unavailable(), []);
}
