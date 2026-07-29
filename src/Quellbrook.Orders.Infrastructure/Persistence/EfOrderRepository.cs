using Microsoft.EntityFrameworkCore;
using Quellbrook.Orders.Domain.Orders;

namespace Quellbrook.Orders.Infrastructure.Persistence;

/// <summary>
/// Loads and stores Order aggregates. Every aggregate loaded or added through one repository instance (one request)
/// is written back by <see cref="SaveChangesAsync"/>.
/// </summary>
public sealed class EfOrderRepository(OrdersDbContext db) : IOrderRepository
{
    private readonly Dictionary<Guid, (Order Order, OrderRecord Record)> _tracked = [];

    public async Task<Order?> FindAsync(OrderId id, CancellationToken cancellationToken)
    {
        if (_tracked.TryGetValue(id.Value, out var tracked))
        {
            return tracked.Order;
        }

        var record = await db.Orders.SingleOrDefaultAsync(order => order.Id == id.Value, cancellationToken).ConfigureAwait(false);
        if (record is null)
        {
            return null;
        }

        var order = OrderDocuments.ToOrder(record);
        _tracked[id.Value] = (order, record);
        return order;
    }

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        var record = new OrderRecord();
        OrderDocuments.CopyTo(order, record);
        db.Orders.Add(record);
        _tracked[order.Id.Value] = (order, record);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var (order, record) in _tracked.Values)
        {
            OrderDocuments.CopyTo(order, record);
            if (db.Entry(record).State == EntityState.Modified)
            {
                record.Version++;
            }

            order.ClearDomainEvents();
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
