namespace Quellbrook.Orders.Infrastructure.Outbox;

/// <summary>An integration event stored with the change that raised it, waiting to be published (ADR 0003).</summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset? DispatchedAt { get; set; }

    public int Attempts { get; set; }
}
