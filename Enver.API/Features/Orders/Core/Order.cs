namespace Enver.API.Features.Orders.Core;

// Los códigos persistidos son estables; no reordenar ni reutilizar valores.
public enum OrderStatus { Published = 1, Accepted = 2, OnTheWay = 3, Arrived = 4, AwaitingConfirmation = 5, Completed = 6, Cancelled = 7 }
public enum OrderCategory { Stationery = 1, Food = 2, Technology = 3, Other = 4 }
public enum DeliveryPreference { Now = 1, InOneHour = 2, LaterToday = 3 }

public sealed record OrderLocation(string? Detail, double? Lat, double? Lng, string? AddressLabel, string? HabitualPlaceLabel);
public sealed record OrderDraft(OrderCategory Category, string Title, string? SpecialInstructions, decimal Amount,
    OrderLocation Location, DeliveryPreference DeliveryTime, Guid? MediaId);

public sealed record Order(Guid Id, Guid OwnerId, Guid? RescuerId, string RequesterName, OrderDraft Draft,
    string Currency, OrderStatus Status, DateTime CreatedAt, DateTime UpdatedAt, DateTime? CancelledAt)
{
    public const string V1Currency = "MXN";
    public bool IsParticipant(Guid viewer) => viewer == OwnerId || viewer == RescuerId;
    public bool IsVisibleTo(Guid viewer) => Status == OrderStatus.Published || IsParticipant(viewer);
}
