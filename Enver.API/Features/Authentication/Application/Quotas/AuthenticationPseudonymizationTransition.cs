namespace Enver.API.Features.Authentication.Application.Quotas;

public enum AuthenticationPseudonymizationChange { Key, CanonicalRepresentation }

/// <summary>
/// Contrato puro, no coordinador ni autorización runtime. Abarca clave o cambios de formato/dominio/versión.
/// Una sola configuración admite; no current/previous concurrentes, dual-read/consume o reset de contadores.
/// Sin prueba global de cese y resolución de operaciones inciertas, no se establece el inicio del drenaje.
/// </summary>
public sealed class AuthenticationPseudonymizationTransition
{
    public AuthenticationPseudonymizationTransition(AuthenticationQuotaPolicyDescriptor oldPolicy,
        AuthenticationQuotaPolicyDescriptor newPolicy, AuthenticationPseudonymizationChange change)
    {
        ArgumentNullException.ThrowIfNull(oldPolicy);
        ArgumentNullException.ThrowIfNull(newPolicy);
        if (!Enum.IsDefined(change)) throw new ArgumentOutOfRangeException(nameof(change));
        OldPolicy = oldPolicy;
        NewPolicy = newPolicy;
        Change = change;
        DrainWindow = oldPolicy.MaximumWindow > newPolicy.MaximumWindow ? oldPolicy.MaximumWindow : newPolicy.MaximumWindow;
    }

    public AuthenticationQuotaPolicyDescriptor OldPolicy { get; }
    public AuthenticationQuotaPolicyDescriptor NewPolicy { get; }
    public AuthenticationPseudonymizationChange Change { get; }
    public TimeSpan DrainWindow { get; }
    // Mismas fases de drenaje que 052, aplicadas aquí a configuración HMAC/representación, no a una key Redis.
    public IReadOnlyList<AuthenticationQuotaPolicyTransitionStep> RequiredSteps => AuthenticationQuotaPolicyTransition.RequiredSteps;

    /// <summary>
    /// Solo aritmética del horizonte desde último consumo posible, no desde despliegue.
    /// Los instantes deben proceder de evidencia compartida confiable. true NO autoriza activación:
    /// aún se exige cese global, in-flight resueltos, activación coordinada y continuidad verificable.
    /// </summary>
    public bool HasDrainWindowElapsed(DateTimeOffset? lastPossibleConsumption, DateTimeOffset? sharedNow) =>
        lastPossibleConsumption is { } last && sharedNow is { } now && now >= last && now - last >= DrainWindow;
}
