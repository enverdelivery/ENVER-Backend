namespace Enver.API.Features.Authentication.Application.Quotas;

/// <summary>
/// Observación inmutable, no lector/registro físico. LastEventUtc es el máximo de TODOS los eventos
/// admitidos de la partición en estado acreditado; null representa ausencia VERIFICADA solo si IsVerifiable.
/// StateVersion identifica el estado observado para no liberar una partición renovada. No es PolicyRevision
/// ni forma parte de la identidad. El caller futuro debe acreditar continuidad y procedencia compartida.
/// </summary>
public sealed class AuthenticationQuotaPartitionState
{
    public AuthenticationQuotaPartitionState(AuthenticationQuotaPartitionIdentity identity,
        AuthenticationQuotaDefinition definition, DateTimeOffset? lastEventUtc, long stateVersion, bool isVerifiable)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(definition);
        if (!AuthenticationQuotaCapacityContract.IsValidIdentity(identity))
            throw new ArgumentException("Identidad de partición incompatible.");
        ArgumentOutOfRangeException.ThrowIfNegative(stateVersion);
        Identity = identity;
        Definition = definition;
        LastEventUtc = lastEventUtc?.ToUniversalTime();
        StateVersion = stateVersion;
        IsVerifiable = isVerifiable;
        try { InactiveAtUtc = LastEventUtc?.Add(definition.Window); }
        catch (ArgumentOutOfRangeException) { throw new ArgumentException("Horizonte de retención no representable."); }
    }

    public AuthenticationQuotaPartitionIdentity Identity { get; }
    public AuthenticationQuotaDefinition Definition { get; }
    public DateTimeOffset? LastEventUtc { get; }
    public DateTimeOffset? InactiveAtUtc { get; }
    public long StateVersion { get; }
    public bool IsVerifiable { get; }

    /// <summary>
    /// Actividad en (sharedNow - Window, sharedNow]. null significa NO VERIFICABLE, nunca inactivo.
    /// sharedNow debe proceder del mismo Redis TIME que ventanas/capacidad en 021; aquí no se consulta reloj.
    /// </summary>
    public bool? IsActiveAt(DateTimeOffset sharedNow)
    {
        if (!IsVerifiable || LastEventUtc > sharedNow) return null;
        return LastEventUtc is { } last && sharedNow.UtcTicks - last.UtcTicks < Definition.Window.Ticks;
    }

    public override string ToString() => nameof(AuthenticationQuotaPartitionState);
}
