using Quellbrook.Orders.Contracts.IntegrationEvents;
using Quellbrook.Orders.Domain.Orders;
using Quellbrook.Orders.Domain.Orders.Events;

namespace Quellbrook.Orders.Application;

/// <summary>Maps the Order aggregate to the published contract; the domain never sees the contract types.</summary>
public static class OrderContractMapper
{
    /// <summary>The integration messages for the domain events an order raised in this unit of work.</summary>
    public static IReadOnlyList<IntegrationMessage> ToIntegrationMessages(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return [.. order.DomainEvents.Select(domainEvent => domainEvent switch
        {
            OrderPlaced placed => new IntegrationMessage(
                Guid.CreateVersion7(placed.OccurredAt), OrderPlacedV1.EventType, ToOrderPlaced(order), placed.OccurredAt),
            OrderCancelled cancelled => new IntegrationMessage(
                Guid.CreateVersion7(cancelled.OccurredAt),
                OrderCancelledV1.EventType,
                new OrderCancelledV1(cancelled.OrderId.Value, cancelled.Reason, cancelled.OccurredAt),
                cancelled.OccurredAt),
            _ => throw new InvalidOperationException($"No integration event for {domainEvent.GetType().Name}."),
        })];
    }

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
