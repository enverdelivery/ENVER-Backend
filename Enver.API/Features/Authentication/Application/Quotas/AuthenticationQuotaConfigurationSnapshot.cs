namespace Enver.API.Features.Authentication.Application.Quotas;

/// <summary>
/// Snapshot de servidor, inmutable y sin secretos. Environment procede de host/configuración confiable;
/// IdentityConfigurationReference es una referencia de despliegue no secreta, NO clave ni hash de la clave HMAC.
/// El store debe acreditar qué snapshot está activo/fenced entre instancias, no confiar en datos del cliente.
/// No crea namespaces/identidades de partición al cambiar versiones. Sin hot reload independiente.
/// </summary>
public sealed class AuthenticationQuotaConfigurationSnapshot
{
    public AuthenticationQuotaConfigurationSnapshot(string environment, string configurationVersion,
        string identityConfigurationReference, AuthenticationQuotaPolicyDescriptor policy, AuthenticationQuotaCapacityDescriptor capacity)
    {
        QuotaIdentifiers.RequireValid(environment);
        QuotaIdentifiers.RequireValid(configurationVersion);
        QuotaIdentifiers.RequireValid(identityConfigurationReference);
        if (!string.Equals(environment, environment.ToLowerInvariant(), StringComparison.Ordinal))
            throw new ArgumentException("Ambiente canónico confiable requerido.");
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(capacity);
        Environment = environment;
        ConfigurationVersion = configurationVersion;
        IdentityConfigurationReference = identityConfigurationReference;
        Policy = policy;
        Capacity = capacity;
    }

    public string Environment { get; }
    public string ConfigurationVersion { get; }
    public string IdentityConfigurationReference { get; }
    public AuthenticationQuotaPolicyDescriptor Policy { get; }
    public AuthenticationQuotaCapacityDescriptor Capacity { get; }

    public bool Matches(AuthenticationQuotaConfigurationSnapshot other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return Environment == other.Environment && ConfigurationVersion == other.ConfigurationVersion &&
            IdentityConfigurationReference == other.IdentityConfigurationReference && HasSameQuotaPolicy(other) &&
            Capacity.MaxActivePartitions == other.Capacity.MaxActivePartitions && Capacity.MemoryBudgetBytes == other.Capacity.MemoryBudgetBytes;
    }

    internal bool HasSameQuotaPolicy(AuthenticationQuotaConfigurationSnapshot other) =>
        Policy.Revision == other.Policy.Revision && Policy.Rules.Count == other.Policy.Rules.Count &&
        Policy.Rules.All(rule => other.Policy.Rules.Contains(rule));

    public override string ToString() => nameof(AuthenticationQuotaConfigurationSnapshot);
}

public enum AuthenticationQuotaCapacityTransitionStep
{
    PrepareApprovedCapacityAndMemoryBudget,
    StopOrFenceOldConfigurationAdmissions,
    ResolveInFlightOperations,
    PreserveAllActiveState,
    ActivateSingleSnapshotAcrossInstances,
    ReopenWithVerifiedConfigurationAndContinuity
}

/// <summary>
/// Transición SOLO operacional de capacidad/memoria, no aprobación del dimensionamiento ni coordinador.
/// No requiere drenaje de ventanas; sí configuración única coordinada y presupuesto aprobado.
/// Reducción conserva ocupación/estado; Evaluate sigue permitiendo N=0 y bloquea nuevos hasta que quepan todos.
/// Cambios de política/identidad deben usar además los protocolos de drenaje 052/053, no este atajo.
/// </summary>
public sealed class AuthenticationQuotaCapacityTransition
{
    public AuthenticationQuotaCapacityTransition(AuthenticationQuotaConfigurationSnapshot previous, AuthenticationQuotaConfigurationSnapshot next)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(next);
        if (previous.Environment != next.Environment || previous.ConfigurationVersion == next.ConfigurationVersion ||
            previous.IdentityConfigurationReference != next.IdentityConfigurationReference || !previous.HasSameQuotaPolicy(next))
            throw new ArgumentException("La transición de capacidad requiere snapshots compatibles y versión nueva.");
        Previous = previous;
        Next = next;
    }

    public AuthenticationQuotaConfigurationSnapshot Previous { get; }
    public AuthenticationQuotaConfigurationSnapshot Next { get; }
    public bool RequiresWindowDrain => false;
    public static IReadOnlyList<AuthenticationQuotaCapacityTransitionStep> RequiredSteps { get; } =
        Array.AsReadOnly(Enum.GetValues<AuthenticationQuotaCapacityTransitionStep>());
    public override string ToString() => nameof(AuthenticationQuotaCapacityTransition);
}
