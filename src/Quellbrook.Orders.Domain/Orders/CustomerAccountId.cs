using Quellbrook.Orders.Domain.Common;

namespace Quellbrook.Orders.Domain.Orders;

/// <summary>The shipper's customer account number: <c>QB-</c> followed by four to twelve digits.</summary>
public sealed record CustomerAccountId
{
    private const string Prefix = "QB-";

    private CustomerAccountId(string value) => Value = value;

    public string Value { get; }

    public static CustomerAccountId Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var digits = value.StartsWith(Prefix, StringComparison.Ordinal) ? value.AsSpan(Prefix.Length) : [];
        if (digits.Length is < 4 or > 12 || digits.ContainsAnyExceptInRange('0', '9'))
        {
            throw new DomainException($"'{value}' is not a customer account number.");
        }

        return new CustomerAccountId(value);
    }

    public override string ToString() => Value;
}
