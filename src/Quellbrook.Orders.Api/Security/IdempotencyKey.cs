namespace Quellbrook.Orders.Api.Security;

/// <summary>
/// The optional <c>Idempotency-Key</c> request header: the gateway sends one per submitted order form, so a retried
/// submission returns the order the first one placed.
/// </summary>
public static class IdempotencyKey
{
    public const string HeaderName = "Idempotency-Key";
    public const int MaxLength = 64;

    public static string? From(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var value = request.Headers[HeaderName].ToString().Trim();
        return value.Length is > 0 and <= MaxLength ? value : null;
    }
}
