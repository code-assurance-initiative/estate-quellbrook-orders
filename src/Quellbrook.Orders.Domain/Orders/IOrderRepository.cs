namespace Quellbrook.Orders.Domain.Orders;

public interface IOrderRepository
{
    Task<Order?> FindAsync(OrderId id, CancellationToken cancellationToken);

    /// <summary>The order placed by the request with this idempotency key, if any.</summary>
    Task<Order?> FindByRequestKeyAsync(string requestKey, CancellationToken cancellationToken);

    void Add(Order order);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
