using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Quellbrook.Orders.Application;
using Quellbrook.Orders.Application.PlaceOrder;
using Quellbrook.Orders.Contracts.IntegrationEvents;
using Quellbrook.Orders.Domain.Orders.Events;
using Quellbrook.Orders.UnitTests.TestSupport;

namespace Quellbrook.Orders.UnitTests.Application;

public sealed class PlaceOrderHandlerTests
{
    private readonly InMemoryOrderRepository _orders = new();
    private readonly FakeTimeProvider _time = new(OrderData.PlacedAt);

    private PlaceOrderHandler Handler() => new(_orders, _time, NullLogger<PlaceOrderHandler>.Instance);

    [Fact]
    public async Task AValidOrderIsStoredWithItsOrderPlacedEvent()
    {
        var result = await Handler().HandleAsync(OrderData.Command(parcels: 2), TestContext.Current.CancellationToken);

        Assert.Equal(OperationStatus.Succeeded, result.Status);
        var order = Assert.Single(_orders.Stored.Values);
        Assert.Equal(1, _orders.Saves);
        Assert.IsType<OrderPlaced>(Assert.Single(order.DomainEvents));
        var message = Assert.Single(OrderContractMapper.ToIntegrationMessages(order));
        Assert.Equal(OrderPlacedV1.EventType, message.EventType);
        var placed = Assert.IsType<OrderPlacedV1>(message.Payload);
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
    }

    [Fact]
    public async Task PlacingAgainWithTheSameIdempotencyKeyReturnsTheFirstOrder()
    {
        var command = OrderData.Command() with { IdempotencyKey = "form-7f3a2c" };

        var first = await Handler().HandleAsync(command, TestContext.Current.CancellationToken);
        var second = await Handler().HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.Equal(first.Value, second.Value);
        Assert.Single(_orders.Stored);
    }

    [Fact]
    public async Task TooManyParcelsForExpressIsInvalid()
    {
        var result = await Handler().HandleAsync(OrderData.Command(serviceLevel: "express", parcels: 6), TestContext.Current.CancellationToken);

        Assert.Equal(OperationStatus.Invalid, result.Status);
    }
}
