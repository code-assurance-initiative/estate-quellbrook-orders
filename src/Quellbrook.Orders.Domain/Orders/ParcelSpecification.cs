namespace Quellbrook.Orders.Domain.Orders;

/// <summary>What the shipper declares about a parcel when placing an order.</summary>
public sealed record ParcelSpecification(int WeightGrams, Dimensions Dimensions);
