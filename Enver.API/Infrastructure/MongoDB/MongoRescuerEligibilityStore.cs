using Enver.API.Configuration;
using Enver.API.Features.Users.Application;
using Enver.API.Features.Users.Validation;
using Enver.API.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using static Enver.API.Infrastructure.MongoDB.RescuerEligibilityDocument;

namespace Enver.API.Infrastructure.MongoDB;

public sealed class MongoRescuerEligibilityStore(IMongoDatabase database, MongoDbOptions options) : IRescuerEnrollmentStore
{
    public const string Grants = "rescuerEligibility";
    public const string Events = "rescuerEligibilityEvents";
    private static readonly WriteConcern Durable = new("majority", journal: true);
    private readonly IMongoCollection<BsonDocument> grants = database.GetCollection<BsonDocument>(Grants)
        .WithReadPreference(ReadPreference.Primary).WithReadConcern(ReadConcern.Linearizable).WithWriteConcern(Durable);
    private readonly IMongoCollection<BsonDocument> events = database.GetCollection<BsonDocument>(Events)
        .WithReadPreference(ReadPreference.Primary).WithReadConcern(ReadConcern.Linearizable).WithWriteConcern(Durable);
    private volatile bool prepared;

    // Explicit setup only in the isolated host. Neither the store nor this DDL is registered by default.
    public async Task PrepareAsync(RequestBudget budget)
    {
        using var phase = budget.BeginDependency(options.OperationTimeoutMilliseconds);
        var durableDatabase = database.WithWriteConcern(Durable);
        try { await durableDatabase.CreateCollectionAsync(Grants, cancellationToken: phase.Token); }
        catch (MongoCommandException ex) when (ex.Code == 48) { } // NamespaceExists
        var keys = Builders<BsonDocument>.IndexKeys.Ascending("userId").Ascending("revision");
        await events.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(keys, new() { Name = "eligibility_user_revision", Unique = true }),
            cancellationToken: phase.Token);
        phase.ThrowIfCancellationRequested();
        prepared = true;
    }

    public async Task<GrantRead> ReadAsync(Guid id, RequestBudget budget)
    {
        if (!prepared) return new(GrantReadStatus.Unavailable);
        try
        {
            var row = await FindOneAsync(grants, id, budget);
            return row is null ? new(GrantReadStatus.Missing) : new(GrantReadStatus.Confirmed, Read(row, id));
        }
        catch (Exception ex) when (Unavailable(ex)) { return new(GrantReadStatus.Unavailable); }
    }

    public Task<GrantWriteStatus> InsertAsync(EnrollmentDecision decision, RequestBudget budget)
    {
        if (!prepared || decision.UserId == Guid.Empty || decision.OperationId == Guid.Empty ||
            decision.Now.Kind != DateTimeKind.Utc || !RescuerEnrollmentInput.IsVersion(decision.RulesVersion) ||
            !RescuerEnrollmentInput.IsVersion(decision.PolicyVersion)) return Task.FromResult(GrantWriteStatus.Unavailable);
        var row = RescuerEligibilityDocument.Create(decision);
        return WriteAsync(RescuerEligibilityEventDocument.Create(decision), async (session, token) =>
        {
            await grants.InsertOneAsync(session, row, cancellationToken: token);
            return true;
        }, budget);
    }

    // Storage CAS only. A future caller must authorize the actor independently and check active account for rehabilitation.
    public Task<GrantWriteStatus> TransitionAsync(EligibilityTransition decision, RequestBudget budget)
    {
        if (!prepared || decision.UserId == Guid.Empty || decision.OperationId == Guid.Empty ||
            decision.ExpectedRevision is <= 0 or long.MaxValue || decision.Now.Kind != DateTimeKind.Utc ||
            decision.ExpectedStatus is not (GrantStatus.Enabled or GrantStatus.Revoked) ||
            !RescuerEnrollmentInput.IsVersion(decision.Actor) || !RescuerEnrollmentInput.IsVersion(decision.ReasonCode) ||
            !RescuerEnrollmentInput.IsVersion(decision.PolicyVersion)) return Task.FromResult(GrantWriteStatus.Unavailable);

        var next = decision.ExpectedStatus == GrantStatus.Enabled ? GrantStatus.Revoked : GrantStatus.Enabled;
        return WriteAsync(RescuerEligibilityEventDocument.Create(decision, next), async (session, token) =>
        {
            var filters = Builders<BsonDocument>.Filter;
            var filter = filters.Eq("_id", Uuid(decision.UserId)) &
                filters.Eq("status", Status(decision.ExpectedStatus)) & filters.Eq("revision", decision.ExpectedRevision);
            var current = await grants.Find(session, filter).FirstOrDefaultAsync(token);
            if (current is null) return false;
            if (decision.Now < Read(current, decision.UserId).UpdatedAt) return false;
            var update = Builders<BsonDocument>.Update.Set("status", Status(next))
                .Set("updatedAt", new BsonDateTime(decision.Now)).Inc("revision", 1L);
            return (await grants.UpdateOneAsync(session, filter, update, cancellationToken: token)).ModifiedCount == 1;
        }, budget);
    }

    private async Task<GrantWriteStatus> WriteAsync(BsonDocument audit,
        Func<IClientSessionHandle, CancellationToken, Task<bool>> mutate, RequestBudget budget)
    {
        var commitDispatched = false;
        var fallback = GrantWriteStatus.Unavailable;
        using (var phase = budget.BeginDependency(options.OperationTimeoutMilliseconds))
        {
            try
            {
                phase.ThrowIfCancellationRequested();
                using var session = await database.Client.StartSessionAsync(new ClientSessionOptions { CausalConsistency = false },
                    phase.Token);
                session.StartTransaction(new TransactionOptions(ReadConcern.Snapshot, ReadPreference.Primary, Durable));
                if (await mutate(session, phase.Token))
                {
                    await events.InsertOneAsync(session, audit, cancellationToken: phase.Token);
                    phase.ThrowIfCancellationRequested();
                    commitDispatched = true;
                    await session.CommitTransactionAsync(phase.Token);
                    phase.ThrowIfCancellationRequested();
                    return GrantWriteStatus.Confirmed;
                }
                fallback = GrantWriteStatus.Conflict;
                await session.AbortTransactionAsync(phase.Token);
            }
            catch (MongoWriteException ex) when (!commitDispatched && ex.WriteConcernError is null &&
                ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            { fallback = GrantWriteStatus.Conflict; }
            catch (MongoException ex) when (!commitDispatched && ex.HasErrorLabel("TransientTransactionError"))
            { fallback = GrantWriteStatus.Conflict; } // No commit was sent; read any confirmed winner, never blind retry.
            catch (Exception ex) when (Unavailable(ex)) { }
        }

        if (budget.Cause != BudgetCancellationCause.None) return GrantWriteStatus.Unavailable;
        try
        {
            var confirmed = await FindOneAsync(events, audit["_id"].AsBsonBinaryData.ToGuid(), budget);
            if (confirmed is not null && confirmed.Equals(audit))
                return GrantWriteStatus.Reconciled; // caller must recheck the current grant, not use historical enablement
            return confirmed is null ? fallback : GrantWriteStatus.Unavailable;
        }
        catch (Exception ex) when (Unavailable(ex)) { return GrantWriteStatus.Unavailable; }
    }

    private async Task<BsonDocument?> FindOneAsync(IMongoCollection<BsonDocument> collection, Guid id, RequestBudget budget)
    {
        using var phase = budget.BeginDependency(options.OperationTimeoutMilliseconds);
        phase.ThrowIfCancellationRequested();
        var row = await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", Uuid(id)),
            new FindOptions { MaxTime = TimeSpan.FromMilliseconds(Math.Max(1, Math.Ceiling(phase.Remaining.TotalMilliseconds))) })
            .Limit(1).FirstOrDefaultAsync(phase.Token);
        phase.ThrowIfCancellationRequested();
        return row;
    }

    private static bool Unavailable(Exception ex) =>
        ex is MongoException or BsonException or TimeoutException or OperationCanceledException or FormatException or ArgumentOutOfRangeException;
}
