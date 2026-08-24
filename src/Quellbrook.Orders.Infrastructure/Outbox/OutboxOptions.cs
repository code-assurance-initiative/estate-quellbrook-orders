using System.ComponentModel.DataAnnotations;

namespace Quellbrook.Orders.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    /// <summary>How often the relay looks for undispatched messages.</summary>
    [Range(typeof(TimeSpan), "00:00:00.100", "00:01:00")]
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(2);

    [Range(1, 500)]
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// How long a dispatched message is kept for replay and investigation before it is deleted. The payloads carry
    /// consignee contact details (docs/privacy.md), so this is also their retention in the outbox.
    /// </summary>
    [Range(typeof(TimeSpan), "1.00:00:00", "30.00:00:00")]
    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>How often expired messages are deleted.</summary>
    [Range(typeof(TimeSpan), "00:01:00", "1.00:00:00")]
    public TimeSpan PurgeInterval { get; set; } = TimeSpan.FromHours(1);
}
