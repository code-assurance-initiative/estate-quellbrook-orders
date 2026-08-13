using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quellbrook.Orders.Application;
using Quellbrook.Orders.Domain.Orders;
using Quellbrook.Orders.Infrastructure.Outbox;

namespace Quellbrook.Orders.Infrastructure.Persistence;

/// <summary>
/// Loads and stores Order aggregates. Every aggregate loaded or added through one repository instance (one request)
/// is written back by <see cref="SaveChangesAsync"/>, together with the outbox messages for the events it raised, in
/// one transaction (ADR 0003).
/// </summary>
public sealed class EfOrderRepository(OrdersDbContext db) : IOrderRepository
{
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

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

            foreach (var message in OrderContractMapper.ToIntegrationMessages(order))
            {
                db.OutboxMessages.Add(new OutboxMessage
                {
                    Id = message.MessageId,
                    Type = message.EventType,
                    Payload = JsonSerializer.Serialize(message.Payload, message.Payload.GetType(), s_json),
                    OccurredAt = message.OccurredAt,
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        foreach (var (order, _) in _tracked.Values)
        {
            order.ClearDomainEvents();
        }
    }
}
