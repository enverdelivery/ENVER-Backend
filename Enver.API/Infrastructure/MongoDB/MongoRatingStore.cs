using Enver.API.Configuration;
using Enver.API.Features.Ratings;
using Enver.API.Http;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Enver.API.Infrastructure.MongoDB;

public sealed class MongoRatingStore(IMongoDatabase database, MongoDbOptions options) : IRatingStore
{
    public const string CollectionName = "ratings";
    private readonly IMongoCollection<Row> collection = database.GetCollection<Row>(CollectionName)
        .WithWriteConcern(WriteConcern.WMajority).WithReadConcern(ReadConcern.Majority).WithReadPreference(ReadPreference.Primary);
    private readonly SemaphoreSlim indexGate = new(1);
    private volatile bool indexReady;

    public async Task<RatingInsert> InsertAsync(Rating rating, RequestBudget budget)
    {
        using var operation = budget.BeginDependency(options.OperationTimeoutMilliseconds);
        try
        {
            await EnsureIndexAsync(operation.Token);
            operation.ThrowIfCancellationRequested();
            await collection.InsertOneAsync(new Row(rating.Id, rating.OrderId, rating.AuthorId, rating.RecipientId,
                rating.Score, rating.Comment, rating.CreatedAt), cancellationToken: operation.Token);
            operation.ThrowIfCancellationRequested();
            return RatingInsert.Confirmed;
        }
        catch (MongoWriteException ex) when (ex.WriteConcernError is null && ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        { return RatingInsert.Duplicate; }
        catch (Exception ex) when (ex is MongoException or TimeoutException or OperationCanceledException or FormatException)
        { return RatingInsert.Unavailable; }
    }

    public async Task<Rating?> FindByRelationAsync(Guid order, Guid author, Guid recipient, RequestBudget budget)
    {
        using var operation = budget.BeginDependency(options.OperationTimeoutMilliseconds);
        try
        {
            var row = await collection.Find(r => r.OrderId == order && r.AuthorId == author && r.RecipientId == recipient)
                .FirstOrDefaultAsync(operation.Token);
            operation.ThrowIfCancellationRequested();
            return row is null ? null : new(row.Id, row.OrderId, row.AuthorId, row.RecipientId, row.Score, row.Comment, row.CreatedAt);
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException or OperationCanceledException or FormatException)
        { return null; }
    }

    private async Task EnsureIndexAsync(CancellationToken token)
    {
        if (indexReady) return;
        await indexGate.WaitAsync(token);
        try
        {
            if (indexReady) return;
            var keys = Builders<Row>.IndexKeys.Ascending(r => r.OrderId).Ascending(r => r.AuthorId).Ascending(r => r.RecipientId);
            var index = new CreateIndexModel<Row>(keys, new() { Name = "rating_relation", Unique = true });
            await collection.Indexes.CreateOneAsync(index, cancellationToken: token);
            indexReady = true;
        }
        finally { indexGate.Release(); }
    }

    [method: BsonConstructor]
    private sealed record Row(
        [property: BsonId, BsonGuidRepresentation(GuidRepresentation.Standard)] Guid Id,
        [property: BsonElement("orderId"), BsonGuidRepresentation(GuidRepresentation.Standard)] Guid OrderId,
        [property: BsonElement("authorId"), BsonGuidRepresentation(GuidRepresentation.Standard)] Guid AuthorId,
        [property: BsonElement("recipientId"), BsonGuidRepresentation(GuidRepresentation.Standard)] Guid RecipientId,
        [property: BsonElement("score")] int Score,
        [property: BsonElement("comment")] string? Comment,
        [property: BsonElement("createdAt")] DateTime CreatedAt);
}
