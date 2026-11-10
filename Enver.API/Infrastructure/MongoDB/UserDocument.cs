using Enver.API.Features.Authentication.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Enver.API.Infrastructure.MongoDB;

/// <summary>Representación BSON cerrada. Sin convenciones globales ni campos auxiliares persistidos.</summary>
[BsonIgnoreExtraElements(false)]
public sealed class UserDocument
{
    [BsonConstructor]
    public UserDocument(Guid id, string fullName, string normalizedEmail, string passwordHash,
        UserStatus status, DateTime createdAt, DateTime updatedAt)
    {
        var user = new User(id, fullName, normalizedEmail, passwordHash, status, createdAt, updatedAt);
        Id = user.Id;
        FullName = user.FullName;
        NormalizedEmail = user.NormalizedEmail;
        PasswordHash = user.PasswordHash;
        Status = user.Status;
        CreatedAt = user.CreatedAt;
        UpdatedAt = user.UpdatedAt;
    }

    [BsonId, BsonRequired, BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; }

    [BsonElement("fullName"), BsonRequired]
    public string FullName { get; }

    [BsonElement("normalizedEmail"), BsonRequired]
    public string NormalizedEmail { get; }

    [BsonElement("passwordHash"), BsonRequired]
    public string PasswordHash { get; }

    [BsonElement("status"), BsonRequired, BsonSerializer(typeof(StatusStringSerializer))]
    public UserStatus Status { get; }

    [BsonElement("createdAt"), BsonRequired, BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; }

    [BsonElement("updatedAt"), BsonRequired, BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; }

    public static UserDocument FromUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new(user.Id, user.FullName, user.NormalizedEmail, user.PasswordHash,
            user.Status, user.CreatedAt, user.UpdatedAt);
    }

    public User ToUser() => new(Id, FullName, NormalizedEmail, PasswordHash, Status, CreatedAt, UpdatedAt);

    // EnumSerializer permite enteros incluso con representación String: usar un serializador cerrado.
    public sealed class StatusStringSerializer : SerializerBase<UserStatus>
    {
        public override UserStatus Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            if (context.Reader.GetCurrentBsonType() != BsonType.String)
            {
                throw new FormatException("El estado BSON debe ser String.");
            }

            return context.Reader.ReadString() switch
            {
                "Active" => UserStatus.Active,
                "Disabled" => UserStatus.Disabled,
                _ => throw new FormatException("Estado BSON incompatible.")
            };
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, UserStatus value)
        {
            context.Writer.WriteString(value switch
            {
                UserStatus.Active => "Active",
                UserStatus.Disabled => "Disabled",
                _ => throw new BsonSerializationException("Estado BSON incompatible.")
            });
        }
    }
}
