using System.Security.Cryptography;
using System.Text.Json;
using Enver.API.Features.Orders.Application;
using Enver.API.Features.Orders.Contracts;
using Enver.API.Features.Orders.Core;

namespace Enver.API.UnitTests;

public sealed class OrderContractTests
{
    [Theory]
    [InlineData("Papelería", "Ahora", "30", "30.00")]
    [InlineData("Comida", "En 1 hora", "0.01", "0.01")]
    [InlineData("Tecnología", "Hoy más tarde", "040.1", "40.10")]
    [InlineData("Otros", "Ahora", "40.00", "40.00")]
    public void Android_labels_round_trip_with_exact_money(string category, string delivery, string amount, string cost)
    {
        using var json = JsonSerializer.SerializeToDocument(new { selectedCategory = category, orderTitle = " Título ",
            tarifa = amount, selectedAddressLabel = " Edificio ", deliveryTime = delivery });
        var draft = Assert.IsType<OrderDraft>(OrderInput.Parse(json.RootElement));
        Assert.Equal(category, OrderLabels.Category(draft.Category));
        Assert.Equal(delivery, OrderLabels.Delivery(draft.DeliveryTime));
        Assert.Equal(cost, OrderLabels.Cost(draft.Amount));
        Assert.Equal("Título", draft.Title);
        Assert.Equal("Edificio", draft.Location.AddressLabel);
    }

    [Theory]
    [InlineData("0")][InlineData("-1")][InlineData("1,50")][InlineData("1.001")][InlineData("1e1")]
    [InlineData("79228162514264337593543950335.01")]
    public void Rejects_invalid_or_inexact_amount(string amount)
    {
        using var json = JsonSerializer.SerializeToDocument(new { selectedCategory = "Comida", orderTitle = "Título",
            tarifa = amount, locationDetail = "Edificio", deliveryTime = "Ahora" });
        Assert.Null(OrderInput.Parse(json.RootElement));
    }

    [Fact]
    public void Cursor_is_bound_to_the_listing_viewer_and_position()
    {
        var root = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var first = new OrderCursor(root);
        var second = new OrderCursor(root);
        var viewer = Guid.NewGuid();
        var position = new OrderPosition(DateTime.UtcNow, Guid.NewGuid());
        var token = first.Encode(OrderListing.Mine, viewer, position);
        Assert.True(second.TryDecode(token, OrderListing.Mine, viewer, out var decoded));
        Assert.Equal(position, decoded);
        Assert.False(second.TryDecode(token, OrderListing.Mine, Guid.NewGuid(), out _));
        Assert.False(second.TryDecode(token, OrderListing.Feed, viewer, out _));
        Assert.False(second.TryDecode((token[0] == 'A' ? 'B' : 'A') + token[1..], OrderListing.Mine, viewer, out _));
    }

    [Fact]
    public void Shared_states_and_projection_preserve_visibility_and_compatibility()
    {
        Assert.Equal(new[] { "Publicado", "Aceptado", "En camino", "Llegué", "Esperando confirmación", "Completado", "Cancelado" },
            Enum.GetValues<OrderStatus>().Select(OrderLabels.Status));
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Público",
            new(OrderCategory.Food, "Título", "Privado", 30.01m, new("Privado", 20, -99, "Edificio", null),
                DeliveryPreference.Now, null), Order.V1Currency, OrderStatus.Published, DateTime.UtcNow, DateTime.UtcNow, null);
        var stranger = Guid.NewGuid();
        Assert.True(order.IsVisibleTo(stranger));
        var reduced = OrderDetail.From(order, stranger);
        Assert.Null(reduced.Location);
        Assert.Null(reduced.SpecialInstructions);
        Assert.Null(reduced.ImageUrl);
        Assert.False(reduced.Is100PercentYours);
        Assert.False(OrderDetail.From(order, order.OwnerId).Is100PercentYours);
        var accepted = order with { Status = OrderStatus.Accepted };
        Assert.False(accepted.IsVisibleTo(stranger));
        Assert.True(accepted.IsVisibleTo(accepted.RescuerId!.Value));
        Assert.NotNull(OrderDetail.From(accepted, accepted.RescuerId.Value).Location);
    }
}
