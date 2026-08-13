namespace Quellbrook.Orders.Application;

/// <summary>An integration event ready for the outbox: its message id, routing key and contract payload.</summary>
public sealed record IntegrationMessage(Guid MessageId, string EventType, object Payload, DateTimeOffset OccurredAt);
