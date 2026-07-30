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

        var page = await new OrderQueries(context).ListAsync(2, 2);

        Assert.Equal(5, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.True(page.Items[0].PlacedAt > page.Items[1].PlacedAt);
        Assert.Equal("Aarhus C", page.Items[0].DestinationCity);
        Assert.Equal(2, page.Items[0].ParcelCount);
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
