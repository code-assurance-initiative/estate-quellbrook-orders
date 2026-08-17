namespace Quellbrook.Orders.Contracts.IntegrationEvents;

/// <summary>
/// Published when an order is cancelled (routing key <c>orders.order-cancelled.v1</c>). Schema:
/// contracts/events/order-cancelled.v1.schema.json.
/// </summary>
public sealed record OrderCancelledV1(Guid OrderId, string Reason, DateTimeOffset CancelledAt)
{
    public const string EventType = "orders.order-cancelled.v1";
}
