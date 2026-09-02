using Microsoft.Extensions.Logging;
using Quellbrook.Orders.Domain.Common;
using Quellbrook.Orders.Domain.Orders;

namespace Quellbrook.Orders.Application.PlaceOrder;

public sealed partial class PlaceOrderHandler(
    IOrderRepository orders,
    TimeProvider time,
    ILogger<PlaceOrderHandler> logger)
{
    public async Task<OperationResult<OrderId>> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.IdempotencyKey is { } key
            && await orders.FindByRequestKeyAsync(key, cancellationToken).ConfigureAwait(false) is { } placedBefore)
        {
            return OperationResult.Succeeded(placedBefore.Id);
        }

        Order order;
        try
        {
            order = Create(command, time.GetUtcNow());
        }
        catch (DomainException exception)
        {
            return OperationResult.Invalid<OrderId>(exception.Message);
        }

        orders.Add(order);
        await orders.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        LogOrderPlaced(order.Id.Value, order.Customer.Value);
        return OperationResult.Succeeded(order.Id);
    }

    private static Order Create(PlaceOrderCommand command, DateTimeOffset now)
    {
        var input = command.Consignee;
        var consignee = Consignee.Create(
            input.Name,
            Address.Create(input.Line1, input.Line2, input.PostalCode, input.City, input.CountryCode),
            ContactDetails.Create(input.Email, input.Phone));
        var parcels = command.Parcels
            .Select(parcel => new ParcelSpecification(
                parcel.WeightGrams,
                Dimensions.Create(parcel.LengthCm, parcel.WidthCm, parcel.HeightCm)))
            .ToList();
        return Order.Place(
            new OrderId(Guid.CreateVersion7(now)),
            CustomerAccountId.Parse(command.CustomerAccountId),
            consignee,
            ParseServiceLevel(command.ServiceLevel),
            parcels,
            command.Operator,
            now,
            command.IdempotencyKey);
    }

    private static ServiceLevel ParseServiceLevel(string value) => value switch
    {
        "standard" => ServiceLevel.Standard,
        "express" => ServiceLevel.Express,
        _ => throw new DomainException($"'{value}' is not a service level (standard, express)."),
    };

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} placed for {CustomerAccountId}")]
    private partial void LogOrderPlaced(Guid orderId, string customerAccountId);
}
