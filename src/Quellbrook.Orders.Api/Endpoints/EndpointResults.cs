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

    /// <summary>
    /// The common path of every command an operator authors: the operator header must be present and the request
    /// valid before <paramref name="execute"/> runs with the operator's id.
    /// </summary>
    public static async Task<IResult> ForOperatorAsync(
        HttpRequest http,
        Dictionary<string, string[]> validationErrors,
        Func<string, Task<IResult>> execute)
    {
        var operatorId = OperatorHeader.From(http);
        if (operatorId is null)
        {
            return MissingOperator();
        }

        return validationErrors.Count > 0
            ? Results.ValidationProblem(validationErrors)
            : await execute(operatorId).ConfigureAwait(false);
    }

    public static IResult MissingOperator() =>
        Results.Problem(
            $"The {OperatorHeader.Name} header must name the operator (1 to {OperatorHeader.MaxLength} characters).",
            statusCode: StatusCodes.Status400BadRequest);
}
