using Enver.API.Features.Users.Application;
using Enver.API.Features.Users.Validation;
using MongoDB.Bson;

namespace Enver.API.Infrastructure.MongoDB;

internal static class RescuerEligibilityDocument
{
    internal static BsonBinaryData Uuid(Guid id) => new(id, GuidRepresentation.Standard);
    internal static string Status(GrantStatus status) => status switch
    {
        GrantStatus.Enabled => "enabled",
        GrantStatus.Revoked => "revoked",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    internal static BsonDocument Create(EnrollmentDecision decision) => new()
    {
        { "_id", Uuid(decision.UserId) }, { "status", "enabled" }, { "revision", 1L },
        { "updatedAt", new BsonDateTime(decision.Now) }, { "acceptedRulesVersion", decision.RulesVersion },
        { "rulesAcceptedAt", new BsonDateTime(decision.Now) }
    };

    internal static RescuerGrant Read(BsonDocument row, Guid id)
    {
        if (row.ElementCount != 6 ||
            !HasType("_id", BsonType.Binary) || row["_id"] != Uuid(id) ||
            !HasType("status", BsonType.String) ||
            !HasType("revision", BsonType.Int64) || row["revision"].AsInt64 <= 0 ||
            !HasType("updatedAt", BsonType.DateTime) || !HasType("rulesAcceptedAt", BsonType.DateTime) ||
            !HasType("acceptedRulesVersion", BsonType.String) || !RescuerEnrollmentInput.IsVersion(row["acceptedRulesVersion"].AsString))
            throw new FormatException("Documento de elegibilidad inválido.");

        var revision = row["revision"].AsInt64;
        var status = row["status"].AsString switch
        {
            "enabled" when revision % 2 == 1 => GrantStatus.Enabled,
            "revoked" when revision % 2 == 0 => GrantStatus.Revoked,
            _ => throw new FormatException("Estado de elegibilidad inválido.")
        };
        var updated = row["updatedAt"].AsBsonDateTime.ToUniversalTime();
        var accepted = row["rulesAcceptedAt"].AsBsonDateTime.ToUniversalTime();
        if (updated < accepted || (revision == 1 && updated != accepted))
            throw new FormatException("Fechas de elegibilidad inválidas.");
        return new(id, status, revision, updated, row["acceptedRulesVersion"].AsString, accepted);

        bool HasType(string name, BsonType type) => row.TryGetValue(name, out var value) && value.BsonType == type;
    }
}
