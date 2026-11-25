using Enver.API.Features.Orders.Application;
using Enver.API.Features.Orders.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Enver.API.Infrastructure.MongoDB;

[BsonIgnoreExtraElements(false)]
internal sealed class OrderDocument
{
    [BsonId, BsonGuidRepresentation(GuidRepresentation.Standard)] public Guid Id { get; init; }
    [BsonElement("ownerId"), BsonGuidRepresentation(GuidRepresentation.Standard)] public Guid OwnerId { get; init; }
    [BsonElement("rescuerId"), BsonGuidRepresentation(GuidRepresentation.Standard)] public Guid? RescuerId { get; init; }
    [BsonElement("requesterName")] public required string RequesterName { get; init; }
    [BsonElement("category")] public OrderCategory Category { get; init; }
    [BsonElement("title")] public required string Title { get; init; }
    [BsonElement("specialInstructions")] public string? SpecialInstructions { get; init; }
    [BsonElement("amount"), BsonRepresentation(BsonType.Decimal128)] public decimal Amount { get; init; }
    [BsonElement("currency")] public required string Currency { get; init; }
    [BsonElement("location")] public required OrderLocationDocument Location { get; init; }
    [BsonElement("deliveryTime")] public DeliveryPreference DeliveryTime { get; init; }
    [BsonElement("mediaId"), BsonGuidRepresentation(GuidRepresentation.Standard)] public Guid? MediaId { get; init; }
    [BsonElement("status")] public OrderStatus Status { get; init; }
    [BsonElement("createdAt")] public DateTime CreatedAt { get; init; }
    [BsonElement("updatedAt")] public DateTime UpdatedAt { get; init; }
    [BsonElement("cancelledAt")] public DateTime? CancelledAt { get; init; }
    [BsonElement("idempotencyKey"), BsonGuidRepresentation(GuidRepresentation.Standard)] public Guid IdempotencyKey { get; init; }
    [BsonElement("fingerprint")] public required string Fingerprint { get; init; }

    public Order ToOrder() => new(Id, OwnerId, RescuerId, RequesterName,
        new(Category, Title, SpecialInstructions, Amount, Location.ToLocation(), DeliveryTime, MediaId),
        Currency, Status, CreatedAt, UpdatedAt, CancelledAt);

    public static OrderDocument From(OrderCreation creation)
    {
        var order = creation.Order;
        var draft = order.Draft;
        return new()
        {
            Id = order.Id, OwnerId = order.OwnerId, RescuerId = order.RescuerId, RequesterName = order.RequesterName,
            Category = draft.Category, Title = draft.Title, SpecialInstructions = draft.SpecialInstructions,
            Amount = draft.Amount, Currency = order.Currency, Location = OrderLocationDocument.From(draft.Location),
            DeliveryTime = draft.DeliveryTime, MediaId = draft.MediaId, Status = order.Status, CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt, CancelledAt = order.CancelledAt, IdempotencyKey = creation.IdempotencyKey,
            Fingerprint = creation.Fingerprint
        };
    }
}

internal sealed class OrderLocationDocument
{
    [BsonElement("detail")] public string? Detail { get; init; }
    [BsonElement("lat")] public double? Lat { get; init; }
    [BsonElement("lng")] public double? Lng { get; init; }
    [BsonElement("addressLabel")] public string? AddressLabel { get; init; }
    [BsonElement("habitualPlaceLabel")] public string? HabitualPlaceLabel { get; init; }

    public OrderLocation ToLocation() => new(Detail, Lat, Lng, AddressLabel, HabitualPlaceLabel);
    public static OrderLocationDocument From(OrderLocation location) => new()
    {
        Detail = location.Detail, Lat = location.Lat, Lng = location.Lng, AddressLabel = location.AddressLabel,
        HabitualPlaceLabel = location.HabitualPlaceLabel
    };
}
