using System.Text;
using Quellbrook.Orders.Infrastructure.Outbox;

namespace Quellbrook.Orders.UnitTests.TestSupport;

internal sealed class RecordingOutboxPublisher : IOutboxPublisher
{
    public List<(Guid MessageId, string EventType, string Body)> Published { get; } = [];

    /// <summary>Publishing the message with this event type throws, as a broker outage would.</summary>
    public string? FailOn { get; set; }

    public Task PublishAsync(Guid messageId, string eventType, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        if (eventType == FailOn)
        {
            throw new InvalidOperationException("broker unavailable");
        }

        Published.Add((messageId, eventType, Encoding.UTF8.GetString(body.Span)));
        return Task.CompletedTask;
    }
}
