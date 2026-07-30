namespace Quellbrook.Orders.Application.PlaceOrder;

public sealed record PlaceOrderCommand(
    string CustomerAccountId,
    string ServiceLevel,
    ConsigneeInput Consignee,
    IReadOnlyList<ParcelInput> Parcels,
    string Operator);

public sealed record ConsigneeInput(
    string Name,
    string Line1,
    string? Line2,
    string PostalCode,
    string City,
    string CountryCode,
    string? Email,
    string? Phone);

public sealed record ParcelInput(int WeightGrams, int LengthCm, int WidthCm, int HeightCm);
