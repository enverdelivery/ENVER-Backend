using Enver.API.Configuration;
using Enver.API.Features.Users.Application;
using Enver.API.Features.Users.Core;
using Enver.API.Http;
using MongoDB.Driver;

namespace Enver.API.Infrastructure.MongoDB;

public sealed class MongoUserProfileStore(IMongoDatabase database, MongoDbOptions options) : IUserProfileStore
{
    // Mongo supplies the unique _id index. No startup creation or additional index is needed.
    public const string CollectionName = "userProfiles";
    private readonly IMongoCollection<UserProfileDocument> profiles = database.GetCollection<UserProfileDocument>(CollectionName);

    public async Task<ProfileStoreResult> ReadAsync(Guid id, RequestBudget budget)
    {
        using var operation = budget.BeginDependency(options.OperationTimeoutMilliseconds);
        try
        {
            operation.ThrowIfCancellationRequested();
            var value = await profiles.Find(x => x.Id == id).FirstOrDefaultAsync(operation.Token).WaitAsync(operation.Token);
            operation.ThrowIfCancellationRequested();
            return value is null ? new(ProfileStoreStatus.Missing) : new(ProfileStoreStatus.Confirmed, value.ToProfile());
        }
        catch { return new(ProfileStoreStatus.Unavailable); }
    }

    public async Task<ProfileStoreResult> UpdateAsync(Guid id, ProfileChange change, RequestBudget budget)
    {
        using var operation = budget.BeginDependency(options.OperationTimeoutMilliseconds);
        try
        {
            var updates = Builders<UserProfileDocument>.Update;
            var fields = new List<UpdateDefinition<UserProfileDocument>>();
            if (change.FullName is not null) fields.Add(updates.Set(x => x.FullName, change.FullName));
            if (change.HasPhone) fields.Add(updates.Set(x => x.Phone, change.Phone));
            if (change.ActiveRole is { } mode) fields.Add(updates.Set(x => x.ActiveRole, mode.ToString()));
            var update = updates.Combine(fields);
            Task<UserProfileDocument> ApplyAsync(bool upsert)
            {
                operation.ThrowIfCancellationRequested();
                return profiles.FindOneAndUpdateAsync(x => x.Id == id, update,
                    new FindOneAndUpdateOptions<UserProfileDocument> { IsUpsert = upsert, ReturnDocument = ReturnDocument.After },
                    operation.Token).WaitAsync(operation.Token);
            }

            UserProfileDocument? value;
            try { value = await ApplyAsync(true); }
            catch (MongoCommandException ex) when (ex.Code == 11000)
            {
                // A definitive duplicate is not an uncertain ACK. Reconcile once without upsert.
                value = await ApplyAsync(false);
            }
            operation.ThrowIfCancellationRequested();
            return value is null ? new(ProfileStoreStatus.Unavailable) : new(ProfileStoreStatus.Confirmed, value.ToProfile());
        }
        catch { return new(ProfileStoreStatus.Unavailable); }
    }
}
