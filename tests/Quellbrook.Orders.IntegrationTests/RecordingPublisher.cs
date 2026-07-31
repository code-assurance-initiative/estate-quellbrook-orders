using System.Collections.Concurrent;
using Quellbrook.Orders.Application.Abstractions;

namespace Quellbrook.Orders.IntegrationTests;

public sealed class RecordingPublisher : IIntegrationEventPublisher
{
    public ConcurrentQueue<(Guid MessageId, string EventType, object Payload)> Published { get; } = new();

    public Task PublishAsync<T>(Guid messageId, string eventType, T payload, CancellationToken cancellationToken)
        where T : class
    {
        Published.Enqueue((messageId, eventType, payload));
        return Task.CompletedTask;
    }
}
