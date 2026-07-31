namespace Quellbrook.Orders.Api.Security;

/// <summary>
/// The gateway authenticates the operator and forwards their id in this header; the service records it on the
/// orders the operator changes. Only the gateway's token carries the write scope, so only the gateway can set it.
/// </summary>
public static class OperatorHeader
{
    public const string Name = "X-Quellbrook-Operator";
    public const int MaxLength = 64;

    public static string? From(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var value = request.Headers[Name].ToString().Trim();
        return value.Length is > 0 and <= MaxLength ? value : null;
    }
}
