using Quellbrook.Orders.Api.Contracts;
using Quellbrook.Orders.Api.Security;
using Quellbrook.Orders.Application;
using Quellbrook.Orders.Application.CancelOrder;
using Quellbrook.Orders.Application.PlaceOrder;
using Quellbrook.Orders.Application.Queries;
using Quellbrook.Orders.Domain.Orders;

namespace Quellbrook.Orders.Api.Endpoints;

public static class OrderEndpoints
{
    public const int MaxPageSize = 100;

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/orders");
        orders.MapPost("/", PlaceAsync).RequireAuthorization(AuthorizationPolicies.WriteOrders);
        orders.MapGet("/", ListAsync).RequireAuthorization(AuthorizationPolicies.ReadOrders);
        orders.MapGet("/{id:guid}", GetAsync).RequireAuthorization(AuthorizationPolicies.ReadOrders);
        orders.MapPost("/{id:guid}/cancellation", CancelAsync).RequireAuthorization(AuthorizationPolicies.WriteOrders);
        return app;
    }

    private static Task<IResult> PlaceAsync(
        PlaceOrderRequest request,
        HttpRequest http,
        PlaceOrderHandler handler,
        CancellationToken cancellationToken) =>
        EndpointResults.ForOperatorAsync(http, request.Validate(), async operatorId =>
        {
            var command = request.ToCommand(operatorId, IdempotencyKey.From(http));
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return result.Status == OperationStatus.Succeeded
                ? Results.Created($"/orders/{result.Value}", new { id = result.Value.Value })
                : EndpointResults.Problem(result);
        });

    private static Task<IResult> CancelAsync(
        Guid id,
        CancelOrderRequest request,
        HttpRequest http,
        CancelOrderHandler handler,
        CancellationToken cancellationToken) =>
        EndpointResults.ForOperatorAsync(http, request.Validate(), async operatorId =>
        {
            var command = new CancelOrderCommand(id, request.Reason ?? string.Empty, operatorId);
            var result = await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return result.Status == OperationStatus.Succeeded ? Results.NoContent() : EndpointResults.Problem(result);
        });

    private static async Task<IResult> GetAsync(Guid id, IOrderRepository orders, CancellationToken cancellationToken)
    {
        var order = await orders.FindAsync(new OrderId(id), cancellationToken).ConfigureAwait(false);
        return order is null ? Results.NotFound() : Results.Ok(OrderResponse.From(order));
    }

    private static async Task<IResult> ListAsync(
        IOrderQueries queries,
        int page = 1,
        int pageSize = 25,
        string? status = null)
    {
        if (page < 1 || pageSize is < 1 or > MaxPageSize)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["paging"] = [$"page must be 1 or more and pageSize between 1 and {MaxPageSize}"],
            });
        }

        if (!TryParseStatus(status, out var wanted))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["status"] = ["must be placed or cancelled"] });
        }

        return Results.Ok(await queries.ListAsync(page, pageSize, wanted).ConfigureAwait(false));
    }

    private static bool TryParseStatus(string? value, out OrderStatus? status)
    {
        status = value switch
        {
            "placed" => OrderStatus.Placed,
            "cancelled" => OrderStatus.Cancelled,
            _ => null,
        };
        return status is not null || string.IsNullOrEmpty(value);
    }
}
