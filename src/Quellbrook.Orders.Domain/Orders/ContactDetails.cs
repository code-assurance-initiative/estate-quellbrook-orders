using Quellbrook.Orders.Domain.Common;

namespace Quellbrook.Orders.Domain.Orders;

/// <summary>
/// How the consignee can be told about the delivery. Both parts are optional: a consignee without contact details
/// simply receives no notifications.
/// </summary>
public sealed record ContactDetails
{
    public static readonly ContactDetails None = new(null, null);

    private ContactDetails(string? email, string? phone)
    {
        Email = email;
        Phone = phone;
    }

    public string? Email { get; }

    public string? Phone { get; }

    public static ContactDetails Create(string? email, string? phone) =>
        new(Text.Optional(email, 254, nameof(email)), Text.Optional(phone, 20, nameof(phone)));
}
