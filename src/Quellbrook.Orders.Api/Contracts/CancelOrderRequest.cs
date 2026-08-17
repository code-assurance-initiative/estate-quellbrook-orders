namespace Quellbrook.Orders.Api.Contracts;

public sealed record CancelOrderRequest(string? Reason)
{
    public const int MaxReasonLength = 200;

    public Dictionary<string, string[]> Validate()
    {
        var errors = new RequestErrors();
        errors.Required(Reason, "reason");
        if (Reason is { Length: > MaxReasonLength })
        {
            errors.Add("reason", $"must be at most {MaxReasonLength} characters");
        }

        return errors.ToDictionary();
    }
}
