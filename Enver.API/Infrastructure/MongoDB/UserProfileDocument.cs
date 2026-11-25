using Enver.API.Features.Users.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Enver.API.Infrastructure.MongoDB;

[BsonIgnoreExtraElements(false)]
internal sealed class UserProfileDocument
{
    [BsonId, BsonRequired, BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; init; }
    [BsonElement("fullName")]
    public string? FullName { get; init; }
    [BsonElement("phone")]
    public string? Phone { get; init; }
    [BsonElement("activeRole")]
    public string? ActiveRole { get; init; }

    public UserProfile ToProfile() => new(FullName, Phone, ActiveRole switch
    {
        null or "Solicitante" => Features.Users.Core.ActiveRole.Solicitante,
        "Rescatista" => Features.Users.Core.ActiveRole.Rescatista,
        _ => throw new FormatException("Modo persistido incompatible.")
    });
}
