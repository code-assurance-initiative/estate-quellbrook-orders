using Quellbrook.Orders.Domain.Orders;
using Quellbrook.Orders.Infrastructure.Persistence;
using Quellbrook.Orders.UnitTests.TestSupport;

namespace Quellbrook.Orders.UnitTests.Infrastructure;

public sealed class OrderQueriesTests : IDisposable
{
    private readonly SqliteOrdersDb _db = new();

    [Fact]
    public async Task OrdersAreListedNewestFirstOnePageAtATime()
    {
        await StoreAsync(5);
        using var context = _db.CreateContext();

        var page = await new OrderQueries(context).ListAsync(2, 2, status: null);

        Assert.Equal(5, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.True(page.Items[0].PlacedAt > page.Items[1].PlacedAt);
        Assert.Equal("Aarhus C", page.Items[0].DestinationCity);
        Assert.Equal(2, page.Items[0].ParcelCount);
    }

    [Fact]
    public async Task OrdersCanBeFilteredByStatus()
    {
        await StoreAsync(3);
        using (var context = _db.CreateContext())
        {
            var repository = new EfOrderRepository(context);
            var first = context.Orders.OrderBy(order => order.Id).First().Id;
            var order = await repository.FindAsync(new OrderId(first), TestContext.Current.CancellationToken);
            Assert.NotNull(order);
            order.Cancel("Shipper withdrew the order", "operator-4", OrderData.PlacedAt.AddHours(1));
            await repository.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var reading = _db.CreateContext();
        var cancelled = await new OrderQueries(reading).ListAsync(1, 10, OrderStatus.Cancelled);
        var placed = await new OrderQueries(reading).ListAsync(1, 10, OrderStatus.Placed);

        Assert.Equal(1, cancelled.TotalCount);
        Assert.Equal("Cancelled", Assert.Single(cancelled.Items).Status);
        Assert.Equal(2, placed.TotalCount);
    }

    private async Task StoreAsync(int count)
    {
        using var context = _db.CreateContext();
        var repository = new EfOrderRepository(context);
        for (var i = 0; i < count; i++)
        {
            repository.Add(Order.Place(
                new OrderId(Guid.NewGuid()), CustomerAccountId.Parse("QB-104233"), OrderData.ConsigneeInAarhus(),
                ServiceLevel.Standard, [OrderData.Parcel(), OrderData.Parcel()], "operator-17",
                OrderData.PlacedAt.AddMinutes(i)));
        }

        await repository.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    public void Dispose() => _db.Dispose();
}
