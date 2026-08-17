namespace Quellbrook.Orders.Application.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, string Reason, string Operator);
