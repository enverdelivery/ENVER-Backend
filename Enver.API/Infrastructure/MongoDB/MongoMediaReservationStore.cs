using Enver.API.Configuration;
using Enver.API.Features.Media;
using Enver.API.Http;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Enver.API.Infrastructure.MongoDB;

public sealed class MongoMediaReservationStore(IMongoDatabase database, MongoDbOptions options) : IMediaReservationStore
{
    public const string Reservations = "mediaReservations", Quotas = "mediaReservationQuotas";
    private readonly IMongoCollection<BsonDocument> reservations = database.GetCollection<BsonDocument>(Reservations);
    private readonly IMongoCollection<BsonDocument> quotas = database.GetCollection<BsonDocument>(Quotas);
    private volatile bool ttlReady;
    private readonly SemaphoreSlim ttlGate = new(1);

    public async Task<QuotaOutcome> ChargeAsync(Guid owner, DateTime windowStart, DateTime windowEnd, int limit, RequestBudget budget)
    {
        using var operation = budget.BeginDependency(options.OperationTimeoutMilliseconds);
        try
        {
            operation.ThrowIfCancellationRequested();
            if (!ttlReady)
            {
                await ttlGate.WaitAsync(operation.Token);
                try
                {
                    if (!ttlReady)
                    {
                        await quotas.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("expiresAt"),
                            new CreateIndexOptions { Name = "quota_expiry", ExpireAfter = TimeSpan.Zero }), cancellationToken: operation.Token).WaitAsync(operation.Token);
                        ttlReady = true;
                    }
                }
                finally { ttlGate.Release(); }
            }
            var id = new BsonDocument { { "ownerId", Uuid(owner) }, { "windowStart", windowStart } };
            var f = Builders<BsonDocument>.Filter;
            var filter = f.Eq("_id", id) & f.Lt("count", limit);
            var update = Builders<BsonDocument>.Update.Inc("count", 1).SetOnInsert("expiresAt", windowEnd.AddDays(1));
            Task<UpdateResult> ApplyAsync(bool upsert) => quotas.UpdateOneAsync(filter, update,
                new UpdateOptions { IsUpsert = upsert }, operation.Token).WaitAsync(operation.Token);
            UpdateResult result;
            try { result = await ApplyAsync(true); }
            catch (MongoWriteException ex) when (ex.WriteConcernError is null && ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            { operation.ThrowIfCancellationRequested(); result = await ApplyAsync(false); }
            operation.ThrowIfCancellationRequested();
            return result.ModifiedCount == 1 || result.UpsertedId is not null ? QuotaOutcome.Granted : QuotaOutcome.Exceeded;
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException or OperationCanceledException)
        { return QuotaOutcome.Unavailable; }
    }

    public async Task<bool> ReserveAsync(MediaReservation reservation, RequestBudget budget)
    {
        using var operation = budget.BeginDependency(options.OperationTimeoutMilliseconds);
        try
        {
            operation.ThrowIfCancellationRequested();
            await reservations.InsertOneAsync(new BsonDocument
            {
                { "_id", Uuid(reservation.MediaId) }, { "ownerId", Uuid(reservation.OwnerId) },
                { "objectKey", reservation.ObjectKey }, { "contentType", reservation.ContentType }, { "status", "pending" },
                { "createdAt", reservation.CreatedAt }, { "uploadExpiresAt", reservation.UploadExpiresAt },
                { "finalKey", BsonNull.Value }, { "verifiedSize", BsonNull.Value }
            }, cancellationToken: operation.Token).WaitAsync(operation.Token);
            operation.ThrowIfCancellationRequested();
            return true;
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException or OperationCanceledException) { return false; }
    }

    private static BsonBinaryData Uuid(Guid id) => new(id, GuidRepresentation.Standard);
}
