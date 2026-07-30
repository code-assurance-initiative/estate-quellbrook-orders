namespace Quellbrook.Orders.Contracts.IntegrationEvents;

/// <summary>
/// Published once per order when it is placed (routing key <c>orders.order-placed.v1</c>). Schema:
/// contracts/events/order-placed.v1.schema.json. Fields are only ever added, never renamed or removed, within v1.
/// </summary>
public sealed record OrderPlacedV1(
    Guid OrderId,
    string CustomerAccountId,
    string ServiceLevel,
    ConsigneeV1 Consignee,
    IReadOnlyList<ParcelV1> Parcels,
    DateTimeOffset PlacedAt)
{
    public const string EventType = "orders.order-placed.v1";
}

public sealed record ConsigneeV1(string Name, AddressV1 Address, ContactV1 Contact);

public sealed record AddressV1(string Line1, string? Line2, string PostalCode, string City, string CountryCode);

public sealed record ContactV1(string? Email, string? Phone);

public sealed record ParcelV1(int Number, int WeightGrams, int LengthCm, int WidthCm, int HeightCm);
