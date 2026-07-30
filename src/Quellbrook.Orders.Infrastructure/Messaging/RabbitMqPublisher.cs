using System.Text.Json;
using Microsoft.Extensions.Options;
using Quellbrook.Orders.Application.Abstractions;
using RabbitMQ.Client;

namespace Quellbrook.Orders.Infrastructure.Messaging;

/// <summary>
/// Publishes JSON events to the topic exchange with the event type as routing key, as persistent messages, and waits
/// for the broker's publisher confirmation.
/// </summary>
public sealed class RabbitMqPublisher(IRabbitMqConnectionProvider connections, IOptions<RabbitMqOptions> options)
    : IIntegrationEventPublisher
{
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    public async Task PublishAsync<T>(Guid messageId, string eventType, T payload, CancellationToken cancellationToken)
        where T : class
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(payload, s_json);
        await PublishRawAsync(messageId, eventType, body, cancellationToken).ConfigureAwait(false);
    }

    public async Task PublishRawAsync(Guid messageId, string eventType, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        var exchange = options.Value.Exchange;
        var connection = await connections.GetConnectionAsync(cancellationToken).ConfigureAwait(false);
        var channelOptions = new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true);
        var channel = await connection.CreateChannelAsync(channelOptions, cancellationToken).ConfigureAwait(false);
        await using (channel.ConfigureAwait(false))
        {
            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var properties = new BasicProperties
            {
                MessageId = messageId.ToString(),
                Type = eventType,
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
            };
            await channel.BasicPublishAsync(exchange, eventType, mandatory: true, properties, body, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
