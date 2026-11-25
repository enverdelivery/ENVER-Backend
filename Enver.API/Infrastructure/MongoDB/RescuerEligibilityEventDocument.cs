using Enver.API.Features.Users.Application;
using MongoDB.Bson;
using static Enver.API.Infrastructure.MongoDB.RescuerEligibilityDocument;

namespace Enver.API.Infrastructure.MongoDB;

// Comando interno de persistencia; no constituye autorización ni está conectado a un canal.
public sealed record EligibilityTransition(Guid OperationId, Guid UserId, GrantStatus ExpectedStatus,
    long ExpectedRevision, string Actor, string PolicyVersion, string ReasonCode, DateTime Now);

internal static class RescuerEligibilityEventDocument
{
    internal static BsonDocument Create(EnrollmentDecision decision)
    {
        var row = Create(decision.OperationId, decision.UserId, 1, BsonNull.Value, GrantStatus.Enabled,
            Uuid(decision.UserId), "server_policy", decision.PolicyVersion, decision.Now);
        row.Add("rulesVersion", decision.RulesVersion);
        row.Add("checkedRequirements", new BsonArray { "active_account", "institutional_email", "profile_phone",
            "accepted_rules", "no_revocation" });
        return row;
    }

    internal static BsonDocument Create(EligibilityTransition decision, GrantStatus next)
    {
        var row = Create(decision.OperationId, decision.UserId, decision.ExpectedRevision + 1,
            Status(decision.ExpectedStatus), next, decision.Actor, "authorized_decision", decision.PolicyVersion, decision.Now);
        row.Add("reasonCode", decision.ReasonCode);
        return row;
    }

    private static BsonDocument Create(Guid operation, Guid user, long revision, BsonValue previous,
        GrantStatus next, BsonValue initiatedBy, string authority, string policyVersion, DateTime now) => new()
    {
        { "_id", Uuid(operation) }, { "userId", Uuid(user) }, { "revision", revision },
        { "previousStatus", previous }, { "status", Status(next) }, { "outcome", "confirmed" },
        { "decidedAt", new BsonDateTime(now) }, { "initiatedBy", initiatedBy },
        { "authority", authority }, { "policyVersion", policyVersion }
    };
}
