using Quellbrook.Orders.Application.Abstractions;

namespace Quellbrook.Orders.UnitTests.TestSupport;

internal sealed class RecordingPublisher : IIntegrationEventPublisher
{
    public List<(Guid MessageId, string EventType, object Payload)> Published { get; } = [];

    public Task PublishAsync<T>(Guid messageId, string eventType, T payload, CancellationToken cancellationToken)
        where T : class
    {
        Published.Add((messageId, eventType, payload));
        return Task.CompletedTask;
    }
}
