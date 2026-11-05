namespace Enver.API.Configuration;

/// <summary>Configuración declarativa; no crea un cliente MongoDB.</summary>
public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDb";
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string UsersCollectionName { get; set; } = string.Empty;
    public int OperationTimeoutMilliseconds { get; set; }
    public int ConnectionTimeoutMilliseconds { get; set; }
    public string WriteConcernCandidate { get; set; } = string.Empty;
    public string ReadConcernCandidate { get; set; } = string.Empty;
    public string ReadPreference { get; set; } = string.Empty;
}
