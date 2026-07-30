using Quellbrook.Orders.Contracts.IntegrationEvents;
using Quellbrook.Orders.Domain.Orders;

namespace Quellbrook.Orders.Application;

/// <summary>Maps the Order aggregate to the published contract; the domain never sees the contract types.</summary>
public static class OrderContractMapper
{
    public static OrderPlacedV1 ToOrderPlaced(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        var consignee = order.Consignee;
        var address = consignee.Address;
        return new OrderPlacedV1(
            order.Id.Value,
            order.Customer.Value,
            ServiceLevelName(order.ServiceLevel),
            new ConsigneeV1(
                consignee.Name,
                new AddressV1(address.Line1, address.Line2, address.PostalCode, address.City, address.CountryCode),
                new ContactV1(consignee.Contact.Email, consignee.Contact.Phone)),
            [.. order.Parcels.Select(parcel => new ParcelV1(
                parcel.Number,
                parcel.WeightGrams,
                parcel.Dimensions.LengthCm,
                parcel.Dimensions.WidthCm,
                parcel.Dimensions.HeightCm))],
            order.PlacedAt);
    }

    public static string ServiceLevelName(ServiceLevel serviceLevel) => serviceLevel switch
    {
        ServiceLevel.Standard => "standard",
        ServiceLevel.Express => "express",
        _ => throw new ArgumentOutOfRangeException(nameof(serviceLevel), serviceLevel, null),
    };
}
