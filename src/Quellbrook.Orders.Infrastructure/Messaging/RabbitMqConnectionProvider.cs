using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Quellbrook.Orders.Infrastructure.Messaging;

public interface IRabbitMqConnectionProvider
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken);
}

/// <summary>One long-lived connection per process, opened on first use; the client recovers it automatically.</summary>
public sealed class RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options) : IRabbitMqConnectionProvider, IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true } open)
        {
            return open;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_connection is { IsOpen: true } current)
            {
                return current;
            }

            var settings = options.Value;
            var factory = new ConnectionFactory
            {
                Uri = settings.Uri ?? throw new InvalidOperationException("RabbitMq:Uri is not configured."),
                UserName = settings.UserName,
                Password = settings.Password,
                ClientProvidedName = "quellbrook-orders",
                AutomaticRecoveryEnabled = true,
            };
            _connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
            return _connection;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        _gate.Dispose();
    }
}
