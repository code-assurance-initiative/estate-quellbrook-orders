using System.ComponentModel.DataAnnotations;

namespace Quellbrook.Orders.Infrastructure.Messaging;

/// <summary>Where events are published. Credentials come from the environment (a Kubernetes Secret).</summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    /// <summary>amqps://host:port/virtual-host, without credentials.</summary>
    [Required]
    public Uri? Uri { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Exchange { get; set; } = "quellbrook.events";
}
