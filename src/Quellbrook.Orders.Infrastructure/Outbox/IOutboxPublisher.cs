namespace Quellbrook.Orders.Infrastructure.Outbox;

/// <summary>Sends one stored message to the broker; returns once the broker has confirmed it.</summary>
public interface IOutboxPublisher
{
    Task PublishAsync(Guid messageId, string eventType, ReadOnlyMemory<byte> body, CancellationToken cancellationToken);
}
