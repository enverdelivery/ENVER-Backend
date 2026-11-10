namespace Enver.API.Features.Authentication.Application.Quotas;

public enum AuthenticationQuotaCapacityIntegrationEvidence
{
    AtomicQuotaAndCapacityMutation,
    ConcurrentInstancesRespectGlobalLimit,
    BoundedEventsBucketsAndRegistry,
    ExpirationAndSerializedRelease,
    ReconciliationAndLossRecoveryWithoutReset,
    PostDispatchUncertainty,
    MeasuredMemoryWithinBudgetIncludingHousekeeping,
    CoexistenceWithoutWeakeningRevocations
}

/// <summary>
/// Presupuesto GLOBAL por ambiente/finalidad auth:ratelimit, compartido por las siete cuotas/instancias.
/// No mide usuarios ni acredita bytes reales. Memoria: buckets, eventos, registro, metadatos y housekeeping;
/// excluye la garantía sobre el proceso Redis completo, SO, API y otros subsistemas. No deriva de una fixture.
/// Las evidencias enumeradas son OBLIGACIONES pendientes de 021/055, nunca resultados acreditados por 054.
/// Si la medición no cabe, reducir capacidad o aprobar más presupuesto antes de despliegue; no degradar revocaciones.
/// </summary>
public sealed class AuthenticationQuotaCapacityDescriptor
{
    public const int AcademicMaxActivePartitions = 5000;
    public const long AuthenticationQuotaMemoryBudgetBytes = 67108864;

    /// <summary>
    /// Constructor de contratos, también para transiciones futuras/sintéticas. Positividad NO aprueba
    /// otro dimensionamiento ni autoriza activación. La configuración académica usa ForAcademicActivation.
    /// </summary>
    public AuthenticationQuotaCapacityDescriptor(int maxActivePartitions, long memoryBudgetBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxActivePartitions);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(memoryBudgetBytes);
        MaxActivePartitions = maxActivePartitions;
        MemoryBudgetBytes = memoryBudgetBytes;
    }

    public int MaxActivePartitions { get; }
    public long MemoryBudgetBytes { get; }

    /// <summary>Sin default correctivo: null no dimensionado, cero/negativos u otro valor no activan esta configuración.</summary>
    public static AuthenticationQuotaCapacityDescriptor ForAcademicActivation(int? configuredMaxActivePartitions)
    {
        if (configuredMaxActivePartitions != AcademicMaxActivePartitions)
            throw new ArgumentException("Capacidad académica explícita aprobada requerida.");
        return new(configuredMaxActivePartitions.Value, AuthenticationQuotaMemoryBudgetBytes);
    }

    public static IReadOnlyList<AuthenticationQuotaCapacityIntegrationEvidence> RequiredIntegrationEvidence { get; } =
        Array.AsReadOnly(Enum.GetValues<AuthenticationQuotaCapacityIntegrationEvidence>());

    public override string ToString() => nameof(AuthenticationQuotaCapacityDescriptor);
}
