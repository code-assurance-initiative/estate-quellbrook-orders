using System.Text.Json;
using Quellbrook.Orders.Domain.Orders;

namespace Quellbrook.Orders.Infrastructure.Persistence;

internal sealed record ConsigneeDocument(
    string Name,
    string Line1,
    string? Line2,
    string PostalCode,
    string City,
    string CountryCode,
    string? Email,
    string? Phone);

internal sealed record ParcelDocument(int Number, int WeightGrams, int LengthCm, int WidthCm, int HeightCm);

/// <summary>Maps the aggregate to and from <see cref="OrderRecord"/>.</summary>
internal static class OrderDocuments
{
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    public static void CopyTo(Order order, OrderRecord record)
    {
        var consignee = order.Consignee;
        record.Id = order.Id.Value;
        record.CustomerAccountId = order.Customer.Value;
        record.ServiceLevel = order.ServiceLevel.ToString();
        record.Status = order.Status.ToString();
        record.ConsigneeName = consignee.Name;
        record.DestinationCity = consignee.Address.City;
        record.ParcelCount = order.Parcels.Count;
        record.Consignee = JsonSerializer.Serialize(
            new ConsigneeDocument(
                consignee.Name,
                consignee.Address.Line1,
                consignee.Address.Line2,
                consignee.Address.PostalCode,
                consignee.Address.City,
                consignee.Address.CountryCode,
                consignee.Contact.Email,
                consignee.Contact.Phone),
            s_json);
        record.Parcels = JsonSerializer.Serialize(
            order.Parcels.Select(parcel => new ParcelDocument(
                parcel.Number,
                parcel.WeightGrams,
                parcel.Dimensions.LengthCm,
                parcel.Dimensions.WidthCm,
                parcel.Dimensions.HeightCm)),
            s_json);
        record.PlacedBy = order.PlacedBy;
        record.PlacedAt = order.PlacedAt;
        record.RequestKey = order.RequestKey;
        record.CancelledBy = order.CancelledBy;
        record.CancelledAt = order.CancelledAt;
        record.CancellationReason = order.CancellationReason;
    }

    public static Order ToOrder(OrderRecord record)
    {
        var consignee = Deserialize<ConsigneeDocument>(record.Consignee);
        var parcels = Deserialize<List<ParcelDocument>>(record.Parcels);
        return Order.Restore(
            new OrderId(record.Id),
            CustomerAccountId.Parse(record.CustomerAccountId),
            Consignee.Create(
                consignee.Name,
                Address.Create(consignee.Line1, consignee.Line2, consignee.PostalCode, consignee.City, consignee.CountryCode),
                ContactDetails.Create(consignee.Email, consignee.Phone)),
            Enum.Parse<ServiceLevel>(record.ServiceLevel),
            parcels.Select(parcel => Parcel.Restore(
                parcel.Number,
                parcel.WeightGrams,
                Dimensions.Create(parcel.LengthCm, parcel.WidthCm, parcel.HeightCm))),
            Enum.Parse<OrderStatus>(record.Status),
            record.PlacedBy,
            record.PlacedAt,
            record.RequestKey,
            record.CancelledAt is { } cancelledAt
                ? new Cancellation(record.CancelledBy ?? string.Empty, cancelledAt, record.CancellationReason ?? string.Empty)
                : null);
    }

    private static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, s_json)
        ?? throw new InvalidOperationException($"Stored {typeof(T).Name} is empty.");
}
