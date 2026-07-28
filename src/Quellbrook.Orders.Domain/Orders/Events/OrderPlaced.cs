using Quellbrook.Orders.Domain.Common;

namespace Quellbrook.Orders.Domain.Orders.Events;

public sealed record OrderPlaced(OrderId OrderId, DateTimeOffset OccurredAt) : IDomainEvent;
