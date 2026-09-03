using Microsoft.Extensions.Options;
using NSubstitute;
using Quellbrook.Orders.Infrastructure.Messaging;
using RabbitMQ.Client;

namespace Quellbrook.Orders.UnitTests.Infrastructure;

public sealed class RabbitMqConnectionProviderTests
{
    private readonly IConnectionFactory _factory = Substitute.For<IConnectionFactory>();

    [Fact]
    public async Task AnOpenConnectionIsReused()
    {
        var connection = Connection(open: true);
        _factory.CreateConnectionAsync(Arg.Any<CancellationToken>()).Returns(connection);
        await using var provider = new RabbitMqConnectionProvider(_factory);

        var first = await provider.GetConnectionAsync(TestContext.Current.CancellationToken);
        var second = await provider.GetConnectionAsync(TestContext.Current.CancellationToken);

        Assert.Same(first, second);
        await _factory.Received(1).CreateConnectionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AClosedConnectionIsDisposedAndReplaced()
    {
        var closed = Connection(open: false);
        var fresh = Connection(open: true);
        _factory.CreateConnectionAsync(Arg.Any<CancellationToken>()).Returns(closed, fresh);
        await using var provider = new RabbitMqConnectionProvider(_factory);

        await provider.GetConnectionAsync(TestContext.Current.CancellationToken);
        var second = await provider.GetConnectionAsync(TestContext.Current.CancellationToken);

        Assert.Same(fresh, second);
        await closed.Received(1).DisposeAsync();
    }

    [Fact]
    public void TheFactoryUsesTheConfiguredBrokerAndCredentials()
    {
        var factory = RabbitMqConnectionProvider.CreateFactory(Options.Create(new RabbitMqOptions
        {
            Uri = new Uri("amqps://rabbitmq.messaging.svc.cluster.local:5671/quellbrook"),
            UserName = "orders",
        }));

        Assert.Equal("rabbitmq.messaging.svc.cluster.local", factory.HostName);
        Assert.Equal("quellbrook", factory.VirtualHost);
        Assert.Equal("orders", factory.UserName);
        Assert.True(factory.AutomaticRecoveryEnabled);
    }

    [Fact]
    public void AMissingBrokerAddressIsAConfigurationError() =>
        Assert.Throws<InvalidOperationException>(() => RabbitMqConnectionProvider.CreateFactory(Options.Create(new RabbitMqOptions())));

    private static IConnection Connection(bool open)
    {
        var connection = Substitute.For<IConnection>();
        connection.IsOpen.Returns(open);
        return connection;
    }
}
