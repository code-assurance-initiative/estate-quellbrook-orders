using Quellbrook.Orders.Application.PlaceOrder;
using Quellbrook.Orders.Domain.Orders;

namespace Quellbrook.Orders.UnitTests.TestSupport;

internal static class OrderData
{
    public static readonly DateTimeOffset PlacedAt = new(2026, 7, 28, 9, 30, 0, TimeSpan.Zero);

    public static Consignee ConsigneeInAarhus() =>
        Consignee.Create(
            "Halden Bikes ApS",
            Address.Create("Søndergade 12", null, "8000", "Aarhus C", "DK"),
            ContactDetails.Create("orders@halden-bikes.example", "+4520304050"));

    public static ParcelSpecification Parcel(int weightGrams = 2_400) =>
        new(weightGrams, Dimensions.Create(40, 30, 20));

    public static Order Placed(ServiceLevel serviceLevel = ServiceLevel.Standard, int parcels = 2) =>
        Order.Place(
            new OrderId(Guid.Parse("0198f1a2-0000-7000-8000-000000000001")),
            CustomerAccountId.Parse("QB-104233"),
            ConsigneeInAarhus(),
            serviceLevel,
            [.. Enumerable.Range(0, parcels).Select(_ => Parcel())],
            "operator-17",
            PlacedAt);

    public static PlaceOrderCommand Command(string serviceLevel = "standard", int parcels = 1) =>
        new(
            "QB-104233",
            serviceLevel,
            new ConsigneeInput("Halden Bikes ApS", "Søndergade 12", null, "8000", "Aarhus C", "dk", "orders@halden-bikes.example", null),
            [.. Enumerable.Range(0, parcels).Select(_ => new ParcelInput(2_400, 40, 30, 20))],
            "operator-17");
}
