namespace Quellbrook.Orders.Application.Queries;

public sealed record OrderSummary(
    Guid OrderId,
    string CustomerAccountId,
    string ServiceLevel,
    string Status,
    string ConsigneeName,
    string DestinationCity,
    int ParcelCount,
    DateTimeOffset PlacedAt);

public sealed record OrderPage(IReadOnlyList<OrderSummary> Items, int Page, int PageSize, int TotalCount);
