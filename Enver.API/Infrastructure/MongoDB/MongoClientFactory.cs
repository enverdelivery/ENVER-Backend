using Enver.API.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

namespace Enver.API.Infrastructure.MongoDB;

/// <summary>
/// Propietario reutilizable de un cliente por destino. La composición debe conservar una instancia
/// por proceso/destino; no registra DI productiva ni confirma disponibilidad o escrituras.
/// </summary>
public sealed class MongoClientFactory : IDisposable
{
    private readonly MongoClientSettings settings;
    private readonly object gate = new();
    private MongoClient? client;
    private bool disposed;

    public MongoClientFactory(MongoDbOptions options) => settings = CreateSettings(options).Freeze();

    public IMongoClient Client
    {
        get
        {
            lock (gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                return client ??= new MongoClient(settings);
            }
        }
    }

    /// <summary>Snapshot independiente, sin registrar ni incluir la conexión en diagnósticos.</summary>
    public static MongoClientSettings CreateSettings(MongoDbOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.ConnectionString) ||
            options.OperationTimeoutMilliseconds <= 0 || options.ConnectionTimeoutMilliseconds <= 0 ||
            options.WriteConcernCandidate != "majority" || options.ReadConcernCandidate != "majority" ||
            options.ReadPreference != "primary")
        {
            throw new ArgumentException("Configuración MongoDB incompatible.", nameof(options));
        }

        MongoClientSettings result;
        try
        {
            // CSOT es interno en 3.13.0 y retira wtimeout del comando: no admitir su activación por URI.
            if (new ConnectionString(options.ConnectionString).AllOptionNames.Contains("timeoutMS", StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Presupuesto de conexión incompatible.");
            }

            result = MongoClientSettings.FromConnectionString(options.ConnectionString);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or MongoConfigurationException)
        {
            // No propagar mensajes del parser que puedan contener credenciales de la URI.
            throw new ArgumentException("Configuración de conexión MongoDB inválida.", nameof(options));
        }

        var operation = TimeSpan.FromMilliseconds(options.OperationTimeoutMilliseconds);
        var connection = TimeSpan.FromMilliseconds(options.ConnectionTimeoutMilliseconds);
        result.RetryWrites = false;
        result.ReadConcern = ReadConcern.Majority;
        result.ReadPreference = ReadPreference.Primary;
        result.WriteConcern = new WriteConcern("majority", wTimeout: operation, journal: true);
        result.ConnectTimeout = connection;
        result.ServerSelectionTimeout = connection;
        result.SocketTimeout = operation;
        result.WaitQueueTimeout = operation;
        // Sin activar CSOT: límites por fase finitos y ACK explícito. Presupuesto total del flujo en 024+.
        // TLS, certificados y topología proceden de la configuración externa, sin excepciones de test.
        return result;
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            client?.Dispose();
        }
    }
}
