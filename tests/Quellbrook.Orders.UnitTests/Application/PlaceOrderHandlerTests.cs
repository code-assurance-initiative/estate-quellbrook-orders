using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Quellbrook.Orders.Application;
using Quellbrook.Orders.Application.PlaceOrder;
using Quellbrook.Orders.Contracts.IntegrationEvents;
using Quellbrook.Orders.UnitTests.TestSupport;

namespace Quellbrook.Orders.UnitTests.Application;

public sealed class PlaceOrderHandlerTests
{
    private readonly InMemoryOrderRepository _orders = new();
    private readonly RecordingPublisher _publisher = new();
    private readonly FakeTimeProvider _time = new(OrderData.PlacedAt);

    private PlaceOrderHandler Handler() => new(_orders, _publisher, _time, NullLogger<PlaceOrderHandler>.Instance);

    [Fact]
    public async Task AValidOrderIsStoredAndAnnounced()
    {
        var result = await Handler().HandleAsync(OrderData.Command(parcels: 2), TestContext.Current.CancellationToken);

        Assert.Equal(OperationStatus.Succeeded, result.Status);
        var order = Assert.Single(_orders.Stored.Values);
        Assert.Equal(1, _orders.Saves);
        var (_, eventType, payload) = Assert.Single(_publisher.Published);
        Assert.Equal(OrderPlacedV1.EventType, eventType);
        var placed = Assert.IsType<OrderPlacedV1>(payload);
        Assert.Equal(order.Id.Value, placed.OrderId);
        Assert.Equal("standard", placed.ServiceLevel);
        Assert.Equal("DK", placed.Consignee.Address.CountryCode);
        Assert.Equal(2, placed.Parcels.Count);
    }

    [Fact]
    public async Task AnUnknownServiceLevelIsInvalidAndNothingIsStored()
    {
        var result = await Handler().HandleAsync(OrderData.Command(serviceLevel: "overnight"), TestContext.Current.CancellationToken);

        Assert.Equal(OperationStatus.Invalid, result.Status);
        Assert.Contains("overnight", result.Error, StringComparison.Ordinal);
        Assert.Empty(_orders.Stored);
        Assert.Empty(_publisher.Published);
    }

    [Fact]
    public async Task TooManyParcelsForExpressIsInvalid()
    {
        var result = await Handler().HandleAsync(OrderData.Command(serviceLevel: "express", parcels: 6), TestContext.Current.CancellationToken);

        Assert.Equal(OperationStatus.Invalid, result.Status);
    }
}
