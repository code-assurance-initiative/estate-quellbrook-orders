using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quellbrook.Orders.Infrastructure.Persistence;

namespace Quellbrook.Orders.Infrastructure.Outbox;

/// <summary>
/// Publishes stored messages in the order they occurred and marks each one dispatched (ADR 0003). A failure stops the
/// batch so that order is kept; the message is retried on the next poll.
/// </summary>
public sealed partial class OutboxRelay(
    IServiceScopeFactory scopes,
    IOutboxPublisher publisher,
    TimeProvider time,
    IOptions<OutboxOptions> options,
    ILogger<OutboxRelay> logger) : BackgroundService
{
    public async Task<int> DispatchPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        var pending = await db.OutboxMessages
            .Where(message => message.DispatchedAt == null)
            .OrderBy(message => message.OccurredAt)
            .ThenBy(message => message.Id)
            .Take(options.Value.BatchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dispatched = 0;
        foreach (var message in pending)
        {
            try
            {
                await publisher.PublishAsync(message.Id, message.Type, Encoding.UTF8.GetBytes(message.Payload), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                message.Attempts++;
                await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                LogPublishFailed(exception, message.Id, message.Type, message.Attempts);
                break;
            }

            message.DispatchedAt = time.GetUtcNow();
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            dispatched++;
        }

        return dispatched;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.PollInterval, time);
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await DispatchPendingAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (DbUpdateException exception)
            {
                LogStoreFailed(exception);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Publishing outbox message {MessageId} ({EventType}) failed, attempt {Attempts}; retrying on the next poll")]
    private partial void LogPublishFailed(Exception exception, Guid messageId, string eventType, int attempts);

    [LoggerMessage(Level = LogLevel.Error, Message = "The outbox could not be updated; retrying on the next poll")]
    private partial void LogStoreFailed(Exception exception);
}
