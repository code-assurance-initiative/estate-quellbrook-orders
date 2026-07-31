using Quellbrook.Orders.Api.Contracts;
using Quellbrook.Orders.Api.Security;
using Quellbrook.Orders.Application;
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
        return app;
    }

    private static async Task<IResult> PlaceAsync(
        PlaceOrderRequest request,
        HttpRequest http,
        PlaceOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var operatorId = OperatorHeader.From(http);
        if (operatorId is null)
        {
            return EndpointResults.MissingOperator();
        }

        var errors = request.Validate();
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var result = await handler.HandleAsync(request.ToCommand(operatorId), cancellationToken).ConfigureAwait(false);
        return result.Status == OperationStatus.Succeeded
            ? Results.Created($"/orders/{result.Value}", new { id = result.Value.Value })
            : EndpointResults.Problem(result);
    }

    private static async Task<IResult> GetAsync(Guid id, IOrderRepository orders, CancellationToken cancellationToken)
    {
        var order = await orders.FindAsync(new OrderId(id), cancellationToken).ConfigureAwait(false);
        return order is null ? Results.NotFound() : Results.Ok(OrderResponse.From(order));
    }

    private static async Task<IResult> ListAsync(
        IOrderQueries queries,
        int page = 1,
        int pageSize = 25)
    {
        if (page < 1 || pageSize is < 1 or > MaxPageSize)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["paging"] = [$"page must be 1 or more and pageSize between 1 and {MaxPageSize}"],
            });
        }

        return Results.Ok(await queries.ListAsync(page, pageSize).ConfigureAwait(false));
    }
}
