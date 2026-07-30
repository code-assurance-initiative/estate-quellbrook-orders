namespace Quellbrook.Orders.Application.Abstractions;

/// <summary>Publishes an integration event to the message broker.</summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(Guid messageId, string eventType, T payload, CancellationToken cancellationToken)
        where T : class;
}
