using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Quellbrook.Orders.Application;
using Quellbrook.Orders.Application.CancelOrder;
using Quellbrook.Orders.Contracts.IntegrationEvents;
using Quellbrook.Orders.Domain.Orders;
using Quellbrook.Orders.UnitTests.TestSupport;

namespace Quellbrook.Orders.UnitTests.Application;

public sealed class CancelOrderHandlerTests
{
    private readonly InMemoryOrderRepository _orders = new();
    private readonly FakeTimeProvider _time = new(OrderData.PlacedAt.AddHours(1));

    private CancelOrderHandler Handler() => new(_orders, _time, NullLogger<CancelOrderHandler>.Instance);

    [Fact]
    public async Task APlacedOrderIsCancelledAndAnnounced()
    {
        var order = OrderData.Placed();
        order.ClearDomainEvents();
        _orders.Add(order);

        var result = await Handler().HandleAsync(new CancelOrderCommand(order.Id.Value, "Shipper withdrew the order", "operator-4"), TestContext.Current.CancellationToken);

        Assert.Equal(OperationStatus.Succeeded, result.Status);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal("operator-4", order.CancelledBy);
        var message = Assert.Single(OrderContractMapper.ToIntegrationMessages(order));
        var cancelled = Assert.IsType<OrderCancelledV1>(message.Payload);
        Assert.Equal("Shipper withdrew the order", cancelled.Reason);
        Assert.Equal(_time.GetUtcNow(), cancelled.CancelledAt);
    }

    [Fact]
    public async Task AnUnknownOrderIsNotFound()
    {
        var result = await Handler().HandleAsync(new CancelOrderCommand(Guid.NewGuid(), "reason", "operator-4"), TestContext.Current.CancellationToken);

        Assert.Equal(OperationStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CancellingTwiceIsAConflict()
    {
        var order = OrderData.Placed();
        order.Cancel("first", "operator-4", OrderData.PlacedAt);
        _orders.Add(order);

        var result = await Handler().HandleAsync(new CancelOrderCommand(order.Id.Value, "again", "operator-4"), TestContext.Current.CancellationToken);

        Assert.Equal(OperationStatus.Conflict, result.Status);
        Assert.Equal(0, _orders.Saves);
    }

    [Fact]
    public async Task ABlankReasonIsInvalid()
    {
        var order = OrderData.Placed();
        _orders.Add(order);

        var result = await Handler().HandleAsync(new CancelOrderCommand(order.Id.Value, " ", "operator-4"), TestContext.Current.CancellationToken);

        Assert.Equal(OperationStatus.Invalid, result.Status);
        Assert.Equal(OrderStatus.Placed, order.Status);
    }
}
