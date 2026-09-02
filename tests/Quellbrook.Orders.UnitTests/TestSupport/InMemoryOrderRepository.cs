using Quellbrook.Orders.Domain.Orders;

namespace Quellbrook.Orders.UnitTests.TestSupport;

internal sealed class InMemoryOrderRepository : IOrderRepository
{
    public Dictionary<OrderId, Order> Stored { get; } = [];

    public int Saves { get; private set; }

    public Task<Order?> FindAsync(OrderId id, CancellationToken cancellationToken) =>
        Task.FromResult(Stored.GetValueOrDefault(id));

    public Task<Order?> FindByRequestKeyAsync(string requestKey, CancellationToken cancellationToken) =>
        Task.FromResult(Stored.Values.FirstOrDefault(order => order.RequestKey == requestKey));

    public void Add(Order order) => Stored.Add(order.Id, order);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saves++;
        return Task.CompletedTask;
    }
}
