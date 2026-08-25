using System.Net.Mail;
using Quellbrook.Orders.Domain.Common;

namespace Quellbrook.Orders.Domain.Orders;

/// <summary>
/// How the consignee can be told about the delivery. Both parts are optional: a consignee without contact details
/// simply receives no notifications. Phone numbers are stored in E.164 form so the SMS provider accepts them.
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
        new(NormaliseEmail(Text.Optional(email, 254, nameof(email))), NormalisePhone(Text.Optional(phone, 20, nameof(phone))));

    private static string? NormaliseEmail(string? email)
    {
        if (email is null)
        {
            return null;
        }

        return MailAddress.TryCreate(email, out var address) && address.Address == email && address.Host.Contains('.', StringComparison.Ordinal)
            ? email
            : throw new DomainException($"'{email}' is not an e-mail address.");
    }

    private static string? NormalisePhone(string? phone)
    {
        if (phone is null)
        {
            return null;
        }

        var compact = string.Concat(phone.Where(character => character is not (' ' or '-')));
        var digits = compact.AsSpan(1);
        var valid = compact.StartsWith('+') && digits.Length is >= 8 and <= 15 && digits[0] != '0'
            && !digits.ContainsAnyExceptInRange('0', '9');
        return valid ? compact : throw new DomainException("A phone number is written in international form, e.g. +45 20 30 40 50.");
    }
}
