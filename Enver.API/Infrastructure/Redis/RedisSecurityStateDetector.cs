namespace Enver.API.Infrastructure.Redis;

/// <summary>
/// Propaga señales reales y saneadas de Redis hacia la admisión de seguridad.
/// No determina integridad, continuidad ni reapertura de confianza.
/// </summary>
public sealed class RedisSecurityStateDetector : IDisposable
{
    private readonly RedisConnections connections;
    private readonly RedisSecurityStateGuard guard;
    private readonly TimeProvider clock;
    private int disposed;

    public RedisSecurityStateDetector(
        RedisConnections connections,
        RedisSecurityStateGuard guard,
        TimeProvider? clock = null)
    {
        this.connections = connections ?? throw new ArgumentNullException(nameof(connections));
        this.guard = guard ?? throw new ArgumentNullException(nameof(guard));
        this.clock = clock ?? TimeProvider.System;
        connections.ConnectivityChanged += OnConnectivityChanged;
    }

    /// <summary>
    /// Señal observada localmente con timestamp monotónico para evidencia y pruebas.
    /// </summary>
    public event EventHandler<RedisSecurityStateObservation>? SignalObserved;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 0)
        {
            connections.ConnectivityChanged -= OnConnectivityChanged;
        }
    }

    private void OnConnectivityChanged(object? sender, RedisConnectionSignal signal)
    {
        var observation = new RedisSecurityStateObservation(signal, clock.GetTimestamp());
        guard.ObserveConnectivitySignal(signal);
        SignalObserved?.Invoke(this, observation);
    }
}

/// <summary>Observación local de conectividad; no acredita estado compartido íntegro.</summary>
public readonly record struct RedisSecurityStateObservation(RedisConnectionSignal Signal, long Timestamp);
