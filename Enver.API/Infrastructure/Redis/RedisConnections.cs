using Enver.API.Configuration;
using StackExchange.Redis;

namespace Enver.API.Infrastructure.Redis;

/// <summary>
/// Propietario de larga vida de una conexión Redis para un destino lógico.
/// No confirma disponibilidad, integridad ni autorización de operaciones de seguridad.
/// </summary>
public sealed class RedisConnections : IAsyncDisposable, IDisposable
{
    private readonly ConfigurationOptions configuration;
    private readonly object synchronization = new();
    private Task<IConnectionMultiplexer>? initialization;
    private bool disposed;

    public RedisConnections(RedisOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        configuration = CreateConfigurationOptions(options);
    }

    /// <summary>
    /// Señales saneadas de conectividad. No contienen claves, secretos ni detalles de conexión.
    /// </summary>
    public event EventHandler<RedisConnectionSignal>? ConnectivityChanged;

    /// <summary>
    /// Obtiene el único multiplexer reutilizable del destino lógico.
    /// Cancelar esta espera no cancela la conexión o comandos Redis ya despachados.
    /// </summary>
    public async Task<IConnectionMultiplexer> GetMultiplexerAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Task<IConnectionMultiplexer> pending;
        lock (synchronization)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            pending = initialization ??= ConnectAsync();
        }

        return await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public static ConfigurationOptions CreateConfigurationOptions(RedisOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new ArgumentException("Redis requiere una conexión externa.", nameof(options));
        }

        if (options.ConnectionTimeoutMilliseconds <= 0 || options.OperationTimeoutMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Redis requiere timeouts positivos.");
        }

        var configuration = ConfigurationOptions.Parse(options.ConnectionString);
        configuration.AbortOnConnectFail = false;
        configuration.ConnectTimeout = options.ConnectionTimeoutMilliseconds;
        configuration.SyncTimeout = options.OperationTimeoutMilliseconds;
        configuration.AsyncTimeout = options.OperationTimeoutMilliseconds;
        configuration.BacklogPolicy = BacklogPolicy.FailFast;
        configuration.ReconnectRetryPolicy = new ExponentialRetry(options.ConnectionTimeoutMilliseconds);
        configuration.IncludeDetailInExceptions = false;
        return configuration;
    }

    public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

    public async ValueTask DisposeAsync()
    {
        Task<IConnectionMultiplexer>? pending;
        lock (synchronization)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            pending = initialization;
        }

        if (pending is null)
        {
            return;
        }

        try
        {
            var multiplexer = await pending.ConfigureAwait(false);
            DetachSignals(multiplexer);
            multiplexer.Dispose();
        }
        catch
        {
            // Una inicialización fallida no deja un multiplexer propietario que liberar.
        }
    }

    private async Task<IConnectionMultiplexer> ConnectAsync()
    {
        var multiplexer = await ConnectionMultiplexer.ConnectAsync(configuration).ConfigureAwait(false);
        AttachSignals(multiplexer);
        return multiplexer;
    }

    private void AttachSignals(IConnectionMultiplexer multiplexer)
    {
        multiplexer.ConnectionFailed += OnConnectionFailed;
        multiplexer.ConnectionRestored += OnConnectionRestored;
        multiplexer.ErrorMessage += OnErrorMessage;
        multiplexer.InternalError += OnInternalError;
    }

    private void DetachSignals(IConnectionMultiplexer multiplexer)
    {
        multiplexer.ConnectionFailed -= OnConnectionFailed;
        multiplexer.ConnectionRestored -= OnConnectionRestored;
        multiplexer.ErrorMessage -= OnErrorMessage;
        multiplexer.InternalError -= OnInternalError;
    }

    private void OnConnectionFailed(object? sender, ConnectionFailedEventArgs args) => Publish(RedisConnectionSignal.ConnectionFailed);
    private void OnConnectionRestored(object? sender, ConnectionFailedEventArgs args) => Publish(RedisConnectionSignal.ConnectionRestored);
    private void OnErrorMessage(object? sender, RedisErrorEventArgs args) => Publish(RedisConnectionSignal.ErrorMessage);
    private void OnInternalError(object? sender, InternalErrorEventArgs args) => Publish(RedisConnectionSignal.InternalError);

    private void Publish(RedisConnectionSignal signal)
    {
        var handlers = ConnectivityChanged;
        if (handlers is null)
        {
            return;
        }

        foreach (EventHandler<RedisConnectionSignal> handler in handlers.GetInvocationList())
        {
            try
            {
                handler(this, signal);
            }
            catch
            {
                // Los observadores no deben afectar la conexión compartida.
            }
        }
    }
}

public enum RedisConnectionSignal
{
    ConnectionFailed,
    ConnectionRestored,
    ErrorMessage,
    InternalError
}
