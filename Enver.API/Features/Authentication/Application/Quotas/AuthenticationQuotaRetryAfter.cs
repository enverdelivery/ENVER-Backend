namespace Enver.API.Features.Authentication.Application.Quotas;

/// <summary>
/// Cálculo puro de referencia para 021, no un limitador local ni un lector de Redis.
/// En ejecución distribuida now procederá de un único Redis TIME compartido por todos los ámbitos.
/// En UT se inyectan instantes sintéticos. No consulta UTC local ni TimeProvider.
/// </summary>
public static class AuthenticationQuotaRetryAfter
{
    /// <summary>
    /// Ventana (now - Window, now]; eventos en el límite inferior están expirados.
    /// Se suministra un timestamp por evento distinto; timestamps iguales son posibles.
    /// El evento relevante ocupa n - limit + 1 (base 1), no siempre el primero.
    /// null significa que este ámbito no bloquea; no confirma la admisión conjunta.
    /// </summary>
    public static TimeSpan? ForScope(DateTimeOffset now, AuthenticationQuotaDefinition definition,
        IEnumerable<DateTimeOffset> eventTimes)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(eventTimes);
        var times = eventTimes.ToArray();
        if (times.Any(time => time > now))
            throw new ArgumentException("El estado contiene eventos futuros respecto al tiempo compartido.", nameof(eventTimes));
        var live = times.Where(time => now.UtcTicks - time.UtcTicks < definition.Window.Ticks)
            .OrderBy(time => time).ToArray();
        if (live.Length < definition.PermitLimit) return null;
        var relevant = live[live.Length - definition.PermitLimit];
        var delayTicks = definition.Window.Ticks - (now.UtcTicks - relevant.UtcTicks);
        return RoundUpToSeconds(TimeSpan.FromTicks(delayTicks));
    }

    /// <summary>Máximo entre ámbitos bloqueantes; no revela cuál bloqueó ni promete éxito futuro.</summary>
    public static TimeSpan? Maximum(IEnumerable<TimeSpan?> scopeDelays)
    {
        ArgumentNullException.ThrowIfNull(scopeDelays);
        TimeSpan? maximum = null;
        foreach (var delay in scopeDelays)
        {
            if (delay is null) continue;
            if (delay <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(scopeDelays));
            if (maximum is null || delay > maximum) maximum = delay;
        }
        return maximum is null ? null : RoundUpToSeconds(maximum.Value);
    }

    private static TimeSpan RoundUpToSeconds(TimeSpan positiveDelay)
    {
        var seconds = ((positiveDelay.Ticks - 1) / TimeSpan.TicksPerSecond) + 1;
        // Si el redondeo no es representable se falla, nunca se trunca a un RetryAfter menor.
        return TimeSpan.FromTicks(checked(seconds * TimeSpan.TicksPerSecond));
    }
}
