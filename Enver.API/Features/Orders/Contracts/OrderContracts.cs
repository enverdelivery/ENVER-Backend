using System.Globalization;
using Enver.API.Features.Orders.Core;

namespace Enver.API.Features.Orders.Contracts;

public static class OrderLabels
{
    public static string Category(OrderCategory value) => value switch
    {
        OrderCategory.Stationery => "Papelería", OrderCategory.Food => "Comida",
        OrderCategory.Technology => "Tecnología", OrderCategory.Other => "Otros",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static string Delivery(DeliveryPreference value) => value switch
    {
        DeliveryPreference.Now => "Ahora", DeliveryPreference.InOneHour => "En 1 hora", DeliveryPreference.LaterToday => "Hoy más tarde",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static string Status(OrderStatus value) => value switch
    {
        OrderStatus.Published => "Publicado", OrderStatus.Accepted => "Aceptado", OrderStatus.OnTheWay => "En camino",
        OrderStatus.Arrived => "Llegué", OrderStatus.AwaitingConfirmation => "Esperando confirmación",
        OrderStatus.Completed => "Completado", OrderStatus.Cancelled => "Cancelado",
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static string Cost(decimal amount) => amount.ToString("F2", CultureInfo.InvariantCulture);
}

public record OrderFeedItem(Guid Id, string Category, string Title, string Cost, bool Is100PercentYours,
    string RequesterName, decimal? Rating, string? Faculty, string? ImageUrl)
{
    // Compatibilidad Android V1: false no tiene significado de negocio.
    public static OrderFeedItem From(Order order) => new(order.Id, OrderLabels.Category(order.Draft.Category),
        order.Draft.Title, OrderLabels.Cost(order.Draft.Amount), false, order.RequesterName, null, null, null);
}

public record OrderSummary(string Status, string DeliveryTime, DateTime CreatedAt, DateTime UpdatedAt,
    Guid Id, string Category, string Title, string Cost, bool Is100PercentYours, string RequesterName,
    decimal? Rating, string? Faculty, string? ImageUrl)
    : OrderFeedItem(Id, Category, Title, Cost, Is100PercentYours, RequesterName, Rating, Faculty, ImageUrl)
{
    public static new OrderSummary From(Order order) => new(OrderLabels.Status(order.Status), OrderLabels.Delivery(order.Draft.DeliveryTime),
        order.CreatedAt, order.UpdatedAt, order.Id, OrderLabels.Category(order.Draft.Category), order.Draft.Title,
        OrderLabels.Cost(order.Draft.Amount), false, order.RequesterName, null, null, null);
}

public sealed record OrderDetail(string? SpecialInstructions, OrderLocation? Location, Guid? MediaId,
    string Status, string DeliveryTime, DateTime CreatedAt, DateTime UpdatedAt, Guid Id, string Category,
    string Title, string Cost, bool Is100PercentYours, string RequesterName, decimal? Rating, string? Faculty, string? ImageUrl)
    : OrderSummary(Status, DeliveryTime, CreatedAt, UpdatedAt, Id, Category, Title, Cost, Is100PercentYours, RequesterName, Rating, Faculty, ImageUrl)
{
    public static OrderDetail From(Order order, Guid viewer)
    {
        var summary = OrderSummary.From(order);
        var participant = order.IsParticipant(viewer);
        return new(participant ? order.Draft.SpecialInstructions : null, participant ? order.Draft.Location : null,
            participant ? order.Draft.MediaId : null, summary.Status, summary.DeliveryTime, summary.CreatedAt, summary.UpdatedAt,
            summary.Id, summary.Category, summary.Title, summary.Cost, summary.Is100PercentYours, summary.RequesterName,
            summary.Rating, summary.Faculty, summary.ImageUrl);
    }
}

public sealed record OrderPage<T>(IReadOnlyList<T> Items, string? NextCursor);
