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
        DateTimeOffset placedAt)
    {
        Id = id;
        Customer = customer;
        Consignee = consignee;
        ServiceLevel = serviceLevel;
        _parcels = parcels;
        PlacedBy = placedBy;
        PlacedAt = placedAt;
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

    public int TotalWeightGrams => _parcels.Sum(parcel => parcel.WeightGrams);

    public static Order Place(
        OrderId id,
        CustomerAccountId customer,
        Consignee consignee,
        ServiceLevel serviceLevel,
        IReadOnlyList<ParcelSpecification> parcels,
        string placedBy,
        DateTimeOffset placedAt)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(consignee);
        ArgumentNullException.ThrowIfNull(parcels);
        ArgumentException.ThrowIfNullOrWhiteSpace(placedBy);
        EnsureParcelsAllowed(serviceLevel, parcels);

        var numbered = parcels.Select((specification, index) => Parcel.Create(index + 1, specification)).ToList();
        var order = new Order(id, customer, consignee, serviceLevel, numbered, placedBy, placedAt);
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
        DateTimeOffset placedAt) =>
        new(id, customer, consignee, serviceLevel, [.. parcels], placedBy, placedAt) { Status = status };

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
