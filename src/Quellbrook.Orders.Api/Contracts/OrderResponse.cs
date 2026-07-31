using Quellbrook.Orders.Application;
using Quellbrook.Orders.Domain.Orders;

namespace Quellbrook.Orders.Api.Contracts;

public sealed record OrderResponse(
    Guid Id,
    string CustomerAccountId,
    string ServiceLevel,
    string Status,
    ConsigneeResponse Consignee,
    IReadOnlyList<ParcelResponse> Parcels,
    int TotalWeightGrams,
    string PlacedBy,
    DateTimeOffset PlacedAt)
{
    public static OrderResponse From(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        var consignee = order.Consignee;
        var address = consignee.Address;
        return new OrderResponse(
            order.Id.Value,
            order.Customer.Value,
            OrderContractMapper.ServiceLevelName(order.ServiceLevel),
            order.Status.ToString().ToLowerInvariant(),
            new ConsigneeResponse(
                consignee.Name,
                address.Line1,
                address.Line2,
                address.PostalCode,
                address.City,
                address.CountryCode,
                consignee.Contact.Email,
                consignee.Contact.Phone),
            [.. order.Parcels.Select(parcel => new ParcelResponse(
                parcel.Number,
                parcel.WeightGrams,
                parcel.Dimensions.LengthCm,
                parcel.Dimensions.WidthCm,
                parcel.Dimensions.HeightCm))],
            order.TotalWeightGrams,
            order.PlacedBy,
            order.PlacedAt);
    }
}

public sealed record ConsigneeResponse(
    string Name,
    string Line1,
    string? Line2,
    string PostalCode,
    string City,
    string CountryCode,
    string? Email,
    string? Phone);

public sealed record ParcelResponse(int Number, int WeightGrams, int LengthCm, int WidthCm, int HeightCm);
