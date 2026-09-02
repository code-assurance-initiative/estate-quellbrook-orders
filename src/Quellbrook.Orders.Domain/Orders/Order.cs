using Quellbrook.Orders.Domain.Common;
using Quellbrook.Orders.Domain.Orders.Events;

namespace Quellbrook.Orders.Domain.Orders;

/// <summary>A shipper's order to collect and deliver one or more parcels to one consignee.</summary>
public sealed class Order : AggregateRoot
{
    public const int MaxParcels = 20;
    public const int MaxExpressParcels = 5;
    public const int MaxParcelWeightGrams = 31_500;

    private readonly List<Parcel> _parcels;

    private Order(
        OrderId id,
        CustomerAccountId customer,
        Consignee consignee,
        ServiceLevel serviceLevel,
        List<Parcel> parcels,
        string placedBy,
        DateTimeOffset placedAt,
        string? requestKey)
    {
        Id = id;
        Customer = customer;
        Consignee = consignee;
        ServiceLevel = serviceLevel;
        _parcels = parcels;
        PlacedBy = placedBy;
        PlacedAt = placedAt;
        RequestKey = requestKey;
        Status = OrderStatus.Placed;
    }

    public OrderId Id { get; }

    public CustomerAccountId Customer { get; }

    public Consignee Consignee { get; }

    public ServiceLevel ServiceLevel { get; }

    public IReadOnlyList<Parcel> Parcels => _parcels;

    public OrderStatus Status { get; private set; }

    /// <summary>The operator who placed the order on the shipper's behalf.</summary>
    public string PlacedBy { get; }

    public DateTimeOffset PlacedAt { get; }

    /// <summary>
    /// The caller's idempotency key for the request that placed the order, if it sent one: placing again with the
    /// same key returns this order instead of placing a second one.
    /// </summary>
    public string? RequestKey { get; }

    public string? CancelledBy { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public string? CancellationReason { get; private set; }

    public int TotalWeightGrams => _parcels.Sum(parcel => parcel.WeightGrams);

    public static Order Place(
        OrderId id,
        CustomerAccountId customer,
        Consignee consignee,
        ServiceLevel serviceLevel,
        IReadOnlyList<ParcelSpecification> parcels,
        string placedBy,
        DateTimeOffset placedAt,
        string? requestKey = null)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(consignee);
        ArgumentNullException.ThrowIfNull(parcels);
        ArgumentException.ThrowIfNullOrWhiteSpace(placedBy);
        EnsureParcelsAllowed(serviceLevel, parcels);

        var numbered = parcels.Select((specification, index) => Parcel.Create(index + 1, specification)).ToList();
        var order = new Order(id, customer, consignee, serviceLevel, numbered, placedBy, placedAt, requestKey);
        order.Raise(new OrderPlaced(id, placedAt));
        return order;
    }

    /// <summary>Rebuilds an order from storage; raises no events.</summary>
    public static Order Restore(
        OrderId id,
        CustomerAccountId customer,
        Consignee consignee,
        ServiceLevel serviceLevel,
        IEnumerable<Parcel> parcels,
        OrderStatus status,
        string placedBy,
        DateTimeOffset placedAt,
        string? requestKey,
        Cancellation? cancellation) =>
        new(id, customer, consignee, serviceLevel, [.. parcels], placedBy, placedAt, requestKey)
        {
            Status = status,
            CancelledBy = cancellation?.By,
            CancelledAt = cancellation?.At,
            CancellationReason = cancellation?.Reason,
        };

    /// <summary>
    /// Cancels a placed order. Dispatch drops the consignment when it hears of it; a parcel already on a route is
    /// returned to the depot by the driver.
    /// </summary>
    public void Cancel(string reason, string cancelledBy, DateTimeOffset cancelledAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cancelledBy);
        if (Status == OrderStatus.Cancelled)
        {
            throw new DomainException($"Order {Id} is already cancelled.");
        }

        var trimmedReason = Text.Required(reason, 200, nameof(reason));
        Status = OrderStatus.Cancelled;
        CancelledBy = cancelledBy;
        CancelledAt = cancelledAt;
        CancellationReason = trimmedReason;
        Raise(new OrderCancelled(Id, trimmedReason, cancelledAt));
    }

    private static void EnsureParcelsAllowed(ServiceLevel serviceLevel, IReadOnlyList<ParcelSpecification> parcels)
    {
        var limit = serviceLevel == ServiceLevel.Express ? MaxExpressParcels : MaxParcels;
        if (parcels.Count == 0 || parcels.Count > limit)
        {
            throw new DomainException($"A {serviceLevel.ToString().ToLowerInvariant()} order has 1 to {limit} parcels.");
        }

        if (parcels.Any(parcel => parcel.WeightGrams is < 1 or > MaxParcelWeightGrams))
        {
            throw new DomainException($"A parcel weighs between 1 g and {MaxParcelWeightGrams} g.");
        }
    }
}
