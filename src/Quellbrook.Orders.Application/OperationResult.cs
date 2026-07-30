namespace Quellbrook.Orders.Application;

public enum OperationStatus
{
    Succeeded,
    Invalid,
    NotFound,
    Conflict,
}

/// <summary>The outcome of a command: a value, or why there is none. Business-rule failures are results, not exceptions.</summary>
public sealed record OperationResult<T>(OperationStatus Status, T? Value, string? Error);

public static class OperationResult
{
    public static OperationResult<T> Succeeded<T>(T value) => new(OperationStatus.Succeeded, value, null);

    public static OperationResult<T> Invalid<T>(string error) => new(OperationStatus.Invalid, default, error);

    public static OperationResult<T> NotFound<T>(string error) => new(OperationStatus.NotFound, default, error);

    public static OperationResult<T> Conflict<T>(string error) => new(OperationStatus.Conflict, default, error);
}
