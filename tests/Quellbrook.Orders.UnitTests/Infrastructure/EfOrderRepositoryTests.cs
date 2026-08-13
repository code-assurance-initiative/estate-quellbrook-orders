using Quellbrook.Orders.Domain.Orders;
using Quellbrook.Orders.Infrastructure.Persistence;
using Quellbrook.Orders.UnitTests.TestSupport;

namespace Quellbrook.Orders.UnitTests.Infrastructure;

public sealed class EfOrderRepositoryTests : IDisposable
{
    private readonly SqliteOrdersDb _db = new();

    [Fact]
    public async Task AnAddedOrderRoundTripsWithItsConsigneeAndParcels()
    {
        var order = OrderData.Placed(parcels: 2);
        using (var context = _db.CreateContext())
        {
            var repository = new EfOrderRepository(context);
            repository.Add(order);
            await repository.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var reading = _db.CreateContext();
        var loaded = await new EfOrderRepository(reading).FindAsync(order.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(order.Consignee, loaded.Consignee);
        Assert.Equal(order.Customer, loaded.Customer);
        Assert.Equal(ServiceLevel.Standard, loaded.ServiceLevel);
        Assert.Equal([1, 2], loaded.Parcels.Select(parcel => parcel.Number));
        Assert.Equal(order.Parcels[0].Dimensions, loaded.Parcels[0].Dimensions);
        Assert.Empty(loaded.DomainEvents);
    }

    [Fact]
    public async Task SavingClearsTheRaisedEvents()
    {
        var order = OrderData.Placed();
        using var context = _db.CreateContext();
        var repository = new EfOrderRepository(context);
        repository.Add(order);

        await repository.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Empty(order.DomainEvents);
    }

    [Fact]
    public async Task SavingWritesOneOutboxMessagePerEventInTheSameTransaction()
    {
        var order = OrderData.Placed();
        using (var context = _db.CreateContext())
        {
            var repository = new EfOrderRepository(context);
            repository.Add(order);
            await repository.SaveChangesAsync(TestContext.Current.CancellationToken);
            var loaded = await repository.FindAsync(order.Id, TestContext.Current.CancellationToken);
            Assert.NotNull(loaded);
            loaded.Cancel("Shipper withdrew the order", "operator-4", OrderData.PlacedAt.AddHours(1));
            await repository.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var reading = _db.CreateContext();
        var messages = reading.OutboxMessages.OrderBy(message => message.OccurredAt).ToList();
        Assert.Equal(["orders.order-placed.v1", "orders.order-cancelled.v1"], messages.Select(message => message.Type));
        Assert.All(messages, message => Assert.Null(message.DispatchedAt));
        Assert.Contains("\"reason\":\"Shipper withdrew the order\"", messages[1].Payload, StringComparison.Ordinal);
        var stored = await new EfOrderRepository(reading).FindAsync(order.Id, TestContext.Current.CancellationToken);
        Assert.Equal(OrderStatus.Cancelled, stored?.Status);
        Assert.Equal("operator-4", stored?.CancelledBy);
    }

    [Fact]
    public async Task AnUnknownOrderIsNotFound()
    {
        using var context = _db.CreateContext();

        var loaded = await new EfOrderRepository(context).FindAsync(new OrderId(Guid.NewGuid()), TestContext.Current.CancellationToken);

        Assert.Null(loaded);
    }

    [Fact]
    public async Task FindingTheSameOrderTwiceReturnsTheTrackedAggregate()
    {
        var order = OrderData.Placed();
        using var context = _db.CreateContext();
        var repository = new EfOrderRepository(context);
        repository.Add(order);
        await repository.SaveChangesAsync(TestContext.Current.CancellationToken);

        var first = await repository.FindAsync(order.Id, TestContext.Current.CancellationToken);

        Assert.Same(order, first);
    }

    public void Dispose() => _db.Dispose();
}
