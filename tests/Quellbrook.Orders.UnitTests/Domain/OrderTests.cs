using Quellbrook.Orders.Domain.Common;
using Quellbrook.Orders.Domain.Orders;
using Quellbrook.Orders.Domain.Orders.Events;
using Quellbrook.Orders.UnitTests.TestSupport;

namespace Quellbrook.Orders.UnitTests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void PlacingAnOrderNumbersItsParcelsAndRaisesOrderPlaced()
    {
        var order = OrderData.Placed(parcels: 3);

        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Equal([1, 2, 3], order.Parcels.Select(parcel => parcel.Number));
        Assert.Equal(7_200, order.TotalWeightGrams);
        var placed = Assert.IsType<OrderPlaced>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, placed.OrderId);
        Assert.Equal(OrderData.PlacedAt, placed.OccurredAt);
    }

    [Fact]
    public void AnOrderNeedsAtLeastOneParcel()
    {
        var exception = Assert.Throws<DomainException>(() => Order.Place(
            new OrderId(Guid.NewGuid()), CustomerAccountId.Parse("QB-104233"), OrderData.ConsigneeInAarhus(),
            ServiceLevel.Standard, [], "operator-17", OrderData.PlacedAt));

        Assert.Contains("1 to 20 parcels", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnExpressOrderTakesAtMostFiveParcels()
    {
        Assert.Throws<DomainException>(() => OrderData.Placed(ServiceLevel.Express, parcels: 6));
        Assert.Equal(5, OrderData.Placed(ServiceLevel.Express, parcels: 5).Parcels.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31_501)]
    public void AParcelWeightOutsideTheLimitsIsRefused(int weightGrams)
    {
        Assert.Throws<DomainException>(() => Order.Place(
            new OrderId(Guid.NewGuid()), CustomerAccountId.Parse("QB-104233"), OrderData.ConsigneeInAarhus(),
            ServiceLevel.Standard, [OrderData.Parcel(weightGrams)], "operator-17", OrderData.PlacedAt));
    }

    [Fact]
    public void ARestoredOrderRaisesNoEvents()
    {
        var placed = OrderData.Placed();

        var restored = Order.Restore(placed.Id, placed.Customer, placed.Consignee, placed.ServiceLevel, placed.Parcels,
            placed.Status, placed.PlacedBy, placed.PlacedAt);

        Assert.Empty(restored.DomainEvents);
        Assert.Equal(placed.TotalWeightGrams, restored.TotalWeightGrams);
    }
}
