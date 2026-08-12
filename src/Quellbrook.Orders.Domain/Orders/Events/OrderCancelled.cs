using Quellbrook.Orders.Domain.Common;

namespace Quellbrook.Orders.Domain.Orders.Events;

public sealed record OrderCancelled(OrderId OrderId, string Reason, DateTimeOffset OccurredAt) : IDomainEvent;
