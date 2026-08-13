namespace Quellbrook.Orders.Infrastructure.Persistence;

/// <summary>
/// The stored shape of an order. The aggregate is kept free of persistence concerns; EfOrderRepository maps between
/// the two. Consignee and parcels are stored as JSON documents, the fields the order list filters and sorts on as
/// columns.
/// </summary>
public sealed class OrderRecord
{
    public Guid Id { get; set; }

    public string CustomerAccountId { get; set; } = string.Empty;

    public string ServiceLevel { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ConsigneeName { get; set; } = string.Empty;

    public string DestinationCity { get; set; } = string.Empty;

    public int ParcelCount { get; set; }

    public string Consignee { get; set; } = string.Empty;

    public string Parcels { get; set; } = string.Empty;

    public string PlacedBy { get; set; } = string.Empty;

    public DateTimeOffset PlacedAt { get; set; }

    public string? CancelledBy { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public string? CancellationReason { get; set; }

    public int Version { get; set; }
}
