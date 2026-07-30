using System.Text.Json;
using Microsoft.Extensions.Options;
using NSubstitute;
using Quellbrook.Orders.Infrastructure.Messaging;
using RabbitMQ.Client;

namespace Quellbrook.Orders.UnitTests.Infrastructure;

public sealed class RabbitMqPublisherTests
{
    private readonly IChannel _channel = Substitute.For<IChannel>();
    private readonly IRabbitMqConnectionProvider _connections = Substitute.For<IRabbitMqConnectionProvider>();

    public RabbitMqPublisherTests()
    {
        var connection = Substitute.For<IConnection>();
        connection.CreateChannelAsync(Arg.Any<CreateChannelOptions?>(), Arg.Any<CancellationToken>()).Returns(_channel);
        _connections.GetConnectionAsync(Arg.Any<CancellationToken>()).Returns(connection);
    }

    [Fact]
    public async Task EventsArePublishedPersistentlyToTheTopicExchangeUnderTheirType()
    {
        var publisher = new RabbitMqPublisher(_connections, Options.Create(new RabbitMqOptions { Exchange = "quellbrook.events" }));
        var messageId = Guid.Parse("0198f1a2-0000-7000-8000-0000000000aa");
        ReadOnlyMemory<byte> body = default;
        BasicProperties? properties = null;
        await _channel.BasicPublishAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(),
            Arg.Do<BasicProperties>(value => properties = value),
            Arg.Do<ReadOnlyMemory<byte>>(value => body = value),
            Arg.Any<CancellationToken>());

        await publisher.PublishAsync(messageId, "orders.order-placed.v1", new { OrderId = 42 }, TestContext.Current.CancellationToken);

        await _channel.Received(1).ExchangeDeclareAsync(
            "quellbrook.events", ExchangeType.Topic, true, Arg.Is(false), Arg.Any<IDictionary<string, object?>?>(),
            Arg.Is(false), Arg.Is(false), Arg.Any<CancellationToken>());
        await _channel.Received(1).BasicPublishAsync(
            "quellbrook.events", "orders.order-placed.v1", true, Arg.Any<BasicProperties>(),
            Arg.Any<ReadOnlyMemory<byte>>(), Arg.Any<CancellationToken>());
        Assert.NotNull(properties);
        Assert.Equal(messageId.ToString(), properties.MessageId);
        Assert.Equal(DeliveryModes.Persistent, properties.DeliveryMode);
        Assert.Equal("application/json", properties.ContentType);
        using var json = JsonDocument.Parse(body);
        Assert.Equal(42, json.RootElement.GetProperty("orderId").GetInt32());
    }
}
