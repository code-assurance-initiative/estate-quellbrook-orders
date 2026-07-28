using Quellbrook.Orders.Domain.Common;

namespace Quellbrook.Orders.Domain.Orders;

/// <summary>A delivery address in one of the countries Quellbrook delivers to.</summary>
public sealed record Address
{
    private static readonly HashSet<string> s_countries = new(StringComparer.Ordinal) { "DK", "SE", "NO", "DE", "NL" };

    private Address(string line1, string? line2, string postalCode, string city, string countryCode)
    {
        Line1 = line1;
        Line2 = line2;
        PostalCode = postalCode;
        City = city;
        CountryCode = countryCode;
    }

    public string Line1 { get; }

    public string? Line2 { get; }

    public string PostalCode { get; }

    public string City { get; }

    /// <summary>ISO 3166-1 alpha-2.</summary>
    public string CountryCode { get; }

    public static Address Create(string line1, string? line2, string postalCode, string city, string countryCode)
    {
        var country = Text.Required(countryCode, 2, nameof(countryCode)).ToUpperInvariant();
        if (!s_countries.Contains(country))
        {
            throw new DomainException($"Quellbrook does not deliver to '{country}'.");
        }

        return new Address(
            Text.Required(line1, 100, nameof(line1)),
            Text.Optional(line2, 100, nameof(line2)),
            Text.Required(postalCode, 10, nameof(postalCode)).ToUpperInvariant(),
            Text.Required(city, 60, nameof(city)),
            country);
    }
}
