using Enver.API.Configuration;
using Enver.API.Http;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Enver.API.Infrastructure.MongoDB;

public enum MongoUsersInitializationStatus { Ready, Incompatible, Unavailable }

/// <summary>
/// Requisito de unicidad: inspecciona índices y crea el índice requerido solo si falta.
/// createIndexes puede crear implícitamente users. No repara índices ni modifica documentos.
/// El primer arranque operacional exige autorización explícita de esta mutación estructural.
/// </summary>
public sealed class MongoUsersInitializer
{
    public const string IndexName = "normalizedEmail_1";
    private readonly IMongoIndexManager<UserDocument> indexes;
    private readonly int operationTimeout;

    public MongoUsersInitializer(IMongoDatabase database, MongoDbOptions options)
    {
        // La misma frontera verifica destino/concerns; no se ejecuta ninguna operación ni se conserva un store.
        _ = new MongoUserStore(database, options);
        indexes = database.GetCollection<UserDocument>("users").Indexes;
        operationTimeout = options.OperationTimeoutMilliseconds;
    }

    public static CreateIndexModel<UserDocument> CreateIndexDefinition() => new(
        Builders<UserDocument>.IndexKeys.Ascending(user => user.NormalizedEmail),
        new CreateIndexOptions { Name = IndexName, Unique = true, Collation = Collation.Simple });

    public async ValueTask<MongoUsersInitializationStatus> InitializeAsync(RequestBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);
        using var dependency = budget.BeginDependency(operationTimeout);
        try
        {
            dependency.ThrowIfCancellationRequested();
            var before = await InspectAsync(dependency);
            if (before is not null)
            {
                return before.Value;
            }

            var duplicateData = false;
            try
            {
                await indexes.CreateOneAsync(CreateIndexDefinition(), cancellationToken: dependency.Token)
                    .WaitAsync(dependency.Token).ConfigureAwait(false);
            }
            catch (MongoCommandException error) when (error.Code == 11000)
            {
                duplicateData = true;
            }
            catch (OperationCanceledException)
            {
                return MongoUsersInitializationStatus.Unavailable;
            }
            catch
            {
                // No repetir creación: solo una reinspección acotada para una posible carrera/ACK perdido.
            }

            dependency.ThrowIfCancellationRequested();
            var after = await InspectAsync(dependency);
            return duplicateData ? MongoUsersInitializationStatus.Incompatible
                : after ?? MongoUsersInitializationStatus.Unavailable;
        }
        catch
        {
            return MongoUsersInitializationStatus.Unavailable;
        }
    }

    private async Task<MongoUsersInitializationStatus?> InspectAsync(DependencyBudget budget)
    {
        List<BsonDocument> definitions;
        try
        {
            using var cursor = await indexes.ListAsync(budget.Token).WaitAsync(budget.Token).ConfigureAwait(false);
            definitions = await cursor.ToListAsync(budget.Token).WaitAsync(budget.Token).ConfigureAwait(false);
        }
        catch (MongoCommandException error) when (error.Code == 26)
        {
            // Namespace inexistente: la creación del índice creará su colección propia.
            definitions = [];
        }

        budget.ThrowIfCancellationRequested();
        var compatible = false;
        foreach (var definition in definitions)
        {
            var named = definition.GetValue("name", "").Equals(new BsonString(IndexName));
            var key = definition.GetValue("key", BsonNull.Value);
            var related = key.IsBsonDocument && key.AsBsonDocument.Contains("normalizedEmail");
            if (!named && !related)
            {
                continue; // No cambiar ni duplicar _id u otros índices independientes.
            }

            if (!IsCompatible(definition))
            {
                return MongoUsersInitializationStatus.Incompatible;
            }

            compatible = true;
        }

        return compatible ? MongoUsersInitializationStatus.Ready : null;
    }

    public static bool IsCompatible(BsonDocument index)
    {
        if (!index.TryGetValue("key", out var key) || !key.IsBsonDocument || key.AsBsonDocument.ElementCount != 1 ||
            !key.AsBsonDocument.TryGetValue("normalizedEmail", out var direction) || !direction.IsNumeric || direction.ToDouble() != 1 ||
            !index.GetValue("unique", false).Equals(BsonBoolean.True) ||
            !index.GetValue("sparse", false).Equals(BsonBoolean.False) ||
            !index.GetValue("hidden", false).Equals(BsonBoolean.False) ||
            index.Contains("partialFilterExpression") || index.Contains("expireAfterSeconds") || index.Contains("buildUUID"))
        {
            return false;
        }

        // listIndexes omite collation cuando la efectiva es simple/binaria.
        return !index.TryGetValue("collation", out var collation) ||
            collation.IsBsonDocument && collation.AsBsonDocument.ElementCount == 1 &&
            collation.AsBsonDocument.GetValue("locale", "").Equals(new BsonString("simple"));
    }
}
