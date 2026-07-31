using Quellbrook.Orders.Api.Security;
using Quellbrook.Orders.Application;

namespace Quellbrook.Orders.Api.Endpoints;

internal static class EndpointResults
{
    public static IResult Problem<T>(OperationResult<T> result) => result.Status switch
    {
        OperationStatus.Invalid => Results.Problem(result.Error, statusCode: StatusCodes.Status422UnprocessableEntity),
        OperationStatus.NotFound => Results.Problem(result.Error, statusCode: StatusCodes.Status404NotFound),
        OperationStatus.Conflict => Results.Problem(result.Error, statusCode: StatusCodes.Status409Conflict),
        _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "Not a failure."),
    };

    public static IResult MissingOperator() =>
        Results.Problem(
            $"The {OperatorHeader.Name} header must name the operator (1 to {OperatorHeader.MaxLength} characters).",
            statusCode: StatusCodes.Status400BadRequest);
}
