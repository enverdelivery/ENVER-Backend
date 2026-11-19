using Enver.API.Configuration;
using Enver.API.Features.Authentication.Application.Ports;
using Enver.API.Features.Authentication.Core;
using Enver.API.Http;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Enver.API.Infrastructure.MongoDB;

/// <summary>Persistencia aislable; sin retry, caché, reparación ni traducción HTTP.</summary>
public sealed class MongoUserStore : IUserStore
{
    private readonly IMongoDatabase database;
    private readonly IMongoCollection<UserDocument> users;
    private readonly int operationTimeout;

    public MongoUserStore(IMongoDatabase database, MongoDbOptions options)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(options);
        if (options.OperationTimeoutMilliseconds <= 0 || options.UsersCollectionName != "users" ||
            database.DatabaseNamespace.DatabaseName != options.DatabaseName ||
            !database.Settings.ReadConcern.Equals(ReadConcern.Majority) ||
            !database.Settings.ReadPreference.Equals(ReadPreference.Primary) ||
            !database.Settings.WriteConcern.Equals(new WriteConcern("majority",
                wTimeout: TimeSpan.FromMilliseconds(options.OperationTimeoutMilliseconds), journal: true)) ||
            database.Client.Settings.RetryWrites)
        {
            throw new ArgumentException("Configuración de persistencia MongoDB incompatible.");
        }

        this.database = database;
        users = database.GetCollection<UserDocument>(options.UsersCollectionName);
        operationTimeout = options.OperationTimeoutMilliseconds;
    }

    public ValueTask<UserReadResult> FindByNormalizedEmailAsync(string normalizedEmail, RequestBudget budget) =>
        FindAsync(Builders<UserDocument>.Filter.Eq(user => user.NormalizedEmail, normalizedEmail), budget);

    public ValueTask<UserReadResult> FindByIdAsync(Guid id, RequestBudget budget) =>
        FindAsync(Builders<UserDocument>.Filter.Eq(user => user.Id, id), budget);

    private async ValueTask<UserReadResult> FindAsync(FilterDefinition<UserDocument> filter, RequestBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);
        using var dependency = budget.BeginDependency(operationTimeout);
        try
        {
            dependency.ThrowIfCancellationRequested();
            using var cursor = await users.FindAsync(filter,
                new FindOptions<UserDocument> { Collation = Collation.Simple, Limit = 2 }, dependency.Token)
                .WaitAsync(dependency.Token).ConfigureAwait(false);
            var found = await cursor.ToListAsync(dependency.Token).WaitAsync(dependency.Token).ConfigureAwait(false);
            dependency.ThrowIfCancellationRequested();
            return found.Count switch
            {
                0 => new(UserReadStatus.NotFound),
                1 => new(UserReadStatus.Found, found[0].ToUser()),
                _ => new(UserReadStatus.Unavailable)
            };
        }
        catch
        {
            // No exponer mensajes/valores BSON ni convertir fallos en inexistencia.
            return new(UserReadStatus.Unavailable, cancellationCause: dependency.Cause);
        }
    }

    public async ValueTask<UserInsertResult> InsertAsync(User user, RequestBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);
        using var dependency = budget.BeginDependency(operationTimeout);
        var dispatched = false;
        try
        {
            dependency.ThrowIfCancellationRequested();
            var document = UserDocument.FromUser(user).ToBsonDocument();
            // RunCommand no hereda concerns automáticamente. Especificar el aprobado de 023.
            // Conserva keyPattern estructurado que InsertOne puede descartar fuera de errInfo.
            var command = new BsonDocument
            {
                { "insert", "users" },
                { "documents", new BsonArray { document } },
                { "ordered", true },
                { "writeConcern", database.Settings.WriteConcern.ToBsonDocument() }
            };
            dependency.ThrowIfCancellationRequested();
            dependency.MarkDispatched();
            dispatched = true;
            var response = await database.RunCommandAsync<BsonDocument>(command, ReadPreference.Primary, dependency.Token)
                .WaitAsync(dependency.Token).ConfigureAwait(false);
            dependency.ThrowIfCancellationRequested();
            return new(ClassifyInsertResponse(response));
        }
        catch (MongoWriteException error) when (error.WriteConcernError is null && error.WriteError is not null)
        {
            var details = error.WriteError.Details;
            return new(error.WriteError.Category == ServerErrorCategory.DuplicateKey &&
                details is not null && IsEmailKey(details.GetValue("keyPattern", BsonNull.Value))
                    ? UserInsertStatus.Conflict : UserInsertStatus.Unavailable);
        }
        catch (MongoCommandException error) when (error.Code is 2 or 9 or 13 or 18 or 59 or 72)
        {
            // Rechazos concluyentes del comando (argumentos, autorización, comando/opciones).
            // Timeout, cambio de primary y errores de transporte NO pertenecen a esta lista.
            return new(UserInsertStatus.Unavailable);
        }
        catch
        {
            return new(dispatched ? UserInsertStatus.Uncertain : UserInsertStatus.Unavailable, dependency.Cause);
        }
    }

    public async ValueTask<UserCredentialUpdateResult> CompareExchangePasswordHashAsync(
        Guid id,
        string expectedPasswordHash,
        UserStatus expectedStatus,
        string newPasswordHash,
        DateTime updatedAtUtc,
        RequestBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);
        using var dependency = budget.BeginDependency(operationTimeout);
        var dispatched = false;
        try
        {
            dependency.ThrowIfCancellationRequested();
            if (id.Version != 4 || (id.ToByteArray()[8] & 0xc0) != 0x80 ||
                expectedPasswordHash is null || newPasswordHash is null ||
                expectedStatus is not (UserStatus.Active or UserStatus.Disabled) || updatedAtUtc.Kind != DateTimeKind.Utc)
            {
                return new(UserCredentialUpdateStatus.Unavailable);
            }
            var instant = DateTimeOffset.FromUnixTimeMilliseconds(new DateTimeOffset(updatedAtUtc).ToUnixTimeMilliseconds()).UtcDateTime;
            var filter = Builders<UserDocument>.Filter.Eq(user => user.Id, id) &
                Builders<UserDocument>.Filter.Eq(user => user.PasswordHash, expectedPasswordHash) &
                Builders<UserDocument>.Filter.Eq(user => user.Status, expectedStatus);
            var update = Builders<UserDocument>.Update.Set(user => user.PasswordHash, newPasswordHash)
                .Set(user => user.UpdatedAt, instant);
            dependency.ThrowIfCancellationRequested();
            // Frontera conservadora: invocar puede enviar; no demuestra envío físico.
            dependency.MarkDispatched();
            dispatched = true;
            var result = await users.UpdateOneAsync(filter, update,
                new UpdateOptions { IsUpsert = false, Collation = Collation.Simple }, dependency.Token)
                .WaitAsync(dependency.Token).ConfigureAwait(false);
            dependency.ThrowIfCancellationRequested();
            if (!result.IsAcknowledged || !result.IsModifiedCountAvailable)
                return new(UserCredentialUpdateStatus.Uncertain);
            return new((result.MatchedCount, result.ModifiedCount) switch
            {
                (1, 1) => UserCredentialUpdateStatus.Updated,
                (0, 0) => UserCredentialUpdateStatus.Conflict,
                (1, 0) => UserCredentialUpdateStatus.Unavailable,
                _ => UserCredentialUpdateStatus.Uncertain
            });
        }
        catch (MongoWriteException error) when (error.WriteConcernError is null && error.WriteError is not null)
        {
            return new(UserCredentialUpdateStatus.Unavailable);
        }
        catch (MongoCommandException error) when (error.Code is 2 or 9 or 13 or 18 or 59 or 72)
        {
            return new(UserCredentialUpdateStatus.Unavailable);
        }
        catch
        {
            // No exponer detalles de excepción, filtros ni hashes fuera del adapter.
            return new(dispatched ? UserCredentialUpdateStatus.Uncertain : UserCredentialUpdateStatus.Unavailable, dependency.Cause);
        }
    }

    private static UserInsertStatus ClassifyInsertResponse(BsonDocument response)
    {
        if (!response.TryGetValue("ok", out var ok) || !ok.IsNumeric || ok.ToDouble() != 1 ||
            response.Contains("writeConcernError") || !response.TryGetValue("n", out var count) || !count.IsNumeric)
        {
            return UserInsertStatus.Uncertain;
        }

        if (response.TryGetValue("writeErrors", out var errors))
        {
            if (!errors.IsBsonArray || errors.AsBsonArray.Count != 1 || count.ToInt64() != 0 ||
                !errors[0].IsBsonDocument)
            {
                return UserInsertStatus.Uncertain;
            }

            var error = errors[0].AsBsonDocument;
            var isDuplicate = error.TryGetValue("code", out var code) && code.IsNumeric && code.ToInt32() == 11000;
            var isFirst = error.TryGetValue("index", out var index) && index.IsNumeric && index.ToInt32() == 0;
            return isDuplicate && isFirst && IsEmailKey(error.GetValue("keyPattern", BsonNull.Value))
                ? UserInsertStatus.Conflict : UserInsertStatus.Unavailable;
        }

        return count.ToInt64() == 1 ? UserInsertStatus.Confirmed : UserInsertStatus.Uncertain;
    }

    private static bool IsEmailKey(BsonValue value) => value.IsBsonDocument && value.AsBsonDocument.ElementCount == 1 &&
        value.AsBsonDocument.TryGetValue("normalizedEmail", out var direction) && direction.IsNumeric && direction.ToDouble() == 1;
}
