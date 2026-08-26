using Microsoft.EntityFrameworkCore;
using Quellbrook.Orders.Application.Queries;
using Quellbrook.Orders.Domain.Orders;

namespace Quellbrook.Orders.Infrastructure.Persistence;

public sealed class OrderQueries(OrdersDbContext db) : IOrderQueries
{
    public async Task<OrderPage> ListAsync(int page, int pageSize, OrderStatus? status)
    {
        var orders = db.Orders.AsNoTracking();
        if (status is { } wanted)
        {
            var stored = wanted.ToString();
            orders = orders.Where(order => order.Status == stored);
        }

        var total = await orders.CountAsync().ConfigureAwait(false);
        var items = await orders
            .OrderByDescending(order => order.PlacedAt)
            .ThenBy(order => order.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new OrderSummary(
                order.Id,
                order.CustomerAccountId,
                order.ServiceLevel,
                order.Status,
                order.ConsigneeName,
                order.DestinationCity,
                order.ParcelCount,
                order.PlacedAt))
            .ToListAsync()
            .ConfigureAwait(false);
        return new OrderPage(items, page, pageSize, total);
    }
}
