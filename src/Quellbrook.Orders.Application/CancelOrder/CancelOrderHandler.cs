using Microsoft.Extensions.Logging;
using Quellbrook.Orders.Domain.Common;
using Quellbrook.Orders.Domain.Orders;

namespace Quellbrook.Orders.Application.CancelOrder;

public sealed partial class CancelOrderHandler(IOrderRepository orders, TimeProvider time, ILogger<CancelOrderHandler> logger)
{
    public async Task<OperationResult<OrderId>> HandleAsync(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var order = await orders.FindAsync(new OrderId(command.OrderId), cancellationToken).ConfigureAwait(false);
        if (order is null)
        {
            return OperationResult.NotFound<OrderId>($"Order {command.OrderId} does not exist.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return OperationResult.Conflict<OrderId>($"Order {order.Id} is already cancelled.");
        }

        try
        {
            order.Cancel(command.Reason, command.Operator, time.GetUtcNow());
        }
        catch (DomainException exception)
        {
            return OperationResult.Invalid<OrderId>(exception.Message);
        }

        await orders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        LogOrderCancelled(order.Id.Value);
        return OperationResult.Succeeded(order.Id);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} cancelled")]
    private partial void LogOrderCancelled(Guid orderId);
}
