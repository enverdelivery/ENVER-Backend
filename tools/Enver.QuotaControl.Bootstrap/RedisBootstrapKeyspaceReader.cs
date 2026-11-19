using System.Net;
using Enver.API.Configuration;
using Enver.API.Infrastructure.Redis;
using StackExchange.Redis;

namespace Enver.QuotaControl.Bootstrap;

// Capacidad cerrada: no expone el multiplexer administrativo, IServer ni comandos arbitrarios.
internal interface IBootstrapKeyspaceReader
{
    ValueTask<string> ReadAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Conexión efímera exclusiva de INFO keyspace. No se entrega a guards ni al lifecycle.
/// AllowAdmin solo desbloquea el control cliente; la ACL Redis sigue siendo la autoridad.
/// </summary>
internal sealed class RedisBootstrapKeyspaceReader : IBootstrapKeyspaceReader
{
    private readonly ConfigurationOptions configuration;
    private readonly string endpoint;
    private readonly Func<ConfigurationOptions, Task<IConnectionMultiplexer>> connect;

    internal RedisBootstrapKeyspaceReader(RedisOptions options, string endpoint)
        : this(options, endpoint, static async configuration =>
            await ConnectionMultiplexer.ConnectAsync(configuration).ConfigureAwait(false))
    {
    }

    // Solo permite dobles sin sockets en las pruebas; producción usa el constructor anterior.
    internal RedisBootstrapKeyspaceReader(RedisOptions options, string endpoint,
        Func<ConfigurationOptions, Task<IConnectionMultiplexer>> connect)
    {
        configuration = RedisConnections.CreateConfigurationOptions(options);
        configuration.AllowAdmin = true;
        this.endpoint = endpoint;
        this.connect = connect;
    }

    public async ValueTask<string> ReadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pending = connect(configuration.Clone());
        IConnectionMultiplexer? multiplexer = null;
        try
        {
            multiplexer = await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            var endpoints = multiplexer.GetEndPoints(configuredOnly: true);
            var matches = endpoints.Where(IsContractEndpoint).ToArray();
            if (endpoints.Length != 1 || matches.Length != 1)
                throw new InvalidOperationException("keyspace-primary-unavailable");

            var server = multiplexer.GetServer(matches[0]);
            if (!IsContractEndpoint(server.EndPoint) || !server.IsConnected ||
                server.ServerType != ServerType.Standalone || server.IsReplica)
                throw new InvalidOperationException("keyspace-primary-unavailable");

            cancellationToken.ThrowIfCancellationRequested();
            var info = await server.InfoRawAsync("keyspace", CommandFlags.DemandMaster)
                .WaitAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return info ?? throw new InvalidOperationException("keyspace-response-unavailable");
        }
        finally
        {
            if (multiplexer is not null) multiplexer.Dispose();
            else if (pending.IsCompletedSuccessfully) pending.Result.Dispose();
            else _ = DisposeWhenConnectedAsync(pending);
        }
    }

    private bool IsContractEndpoint(EndPoint value) => value is DnsEndPoint dns &&
        string.Equals(dns.Host, endpoint, StringComparison.OrdinalIgnoreCase) && dns.Port == BootstrapContract.Port;

    // Cancelar la espera no cancela ConnectAsync. Su eventual resultado sigue teniendo propietario.
    private static async Task DisposeWhenConnectedAsync(Task<IConnectionMultiplexer> pending)
    {
        try { (await pending.ConfigureAwait(false)).Dispose(); }
        catch { /* Fallo de conexión ya observado; sin retry ni logging de detalles sensibles. */ }
    }
}
