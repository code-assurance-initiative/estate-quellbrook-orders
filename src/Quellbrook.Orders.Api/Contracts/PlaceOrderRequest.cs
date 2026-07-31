using Quellbrook.Orders.Application.PlaceOrder;

namespace Quellbrook.Orders.Api.Contracts;

public sealed record PlaceOrderRequest(
    string? CustomerAccountId,
    string? ServiceLevel,
    ConsigneeRequest? Consignee,
    IReadOnlyList<ParcelRequest>? Parcels)
{
    public const int MaxParcels = 20;

    /// <summary>Shape checks; the domain enforces the business rules (limits per service level, weights).</summary>
    public Dictionary<string, string[]> Validate()
    {
        var errors = new RequestErrors();
        errors.Required(CustomerAccountId, "customerAccountId");
        errors.Required(ServiceLevel, "serviceLevel");
        if (Consignee is null)
        {
            errors.Add("consignee", "is required");
        }
        else
        {
            Consignee.Validate(errors);
        }

        if (Parcels is null || Parcels.Count is 0 or > MaxParcels)
        {
            errors.Add("parcels", $"must hold 1 to {MaxParcels} parcels");
        }
        else
        {
            for (var i = 0; i < Parcels.Count; i++)
            {
                Parcels[i].Validate(errors, $"parcels[{i}]");
            }
        }

        return errors.ToDictionary();
    }

    /// <summary>Call only after <see cref="Validate"/> returned no errors.</summary>
    public PlaceOrderCommand ToCommand(string operatorId) =>
        new(
            CustomerAccountId ?? string.Empty,
            ServiceLevel ?? string.Empty,
            Consignee?.ToInput() ?? throw new InvalidOperationException("Validate the request first."),
            [.. (Parcels ?? []).Select(parcel => parcel.ToInput())],
            operatorId);
}

public sealed record ConsigneeRequest(
    string? Name,
    string? Line1,
    string? Line2,
    string? PostalCode,
    string? City,
    string? CountryCode,
    string? Email,
    string? Phone)
{
    internal void Validate(RequestErrors errors)
    {
        errors.Required(Name, "consignee.name");
        errors.Required(Line1, "consignee.line1");
        errors.Required(PostalCode, "consignee.postalCode");
        errors.Required(City, "consignee.city");
        errors.Required(CountryCode, "consignee.countryCode");
    }

    internal ConsigneeInput ToInput() =>
        new(Name ?? string.Empty, Line1 ?? string.Empty, Line2, PostalCode ?? string.Empty, City ?? string.Empty,
            CountryCode ?? string.Empty, Email, Phone);
}

public sealed record ParcelRequest(int WeightGrams, int LengthCm, int WidthCm, int HeightCm)
{
    internal void Validate(RequestErrors errors, string path)
    {
        if (WeightGrams <= 0)
        {
            errors.Add($"{path}.weightGrams", "must be positive");
        }

        if (LengthCm <= 0 || WidthCm <= 0 || HeightCm <= 0)
        {
            errors.Add($"{path}.dimensions", "every side must be positive");
        }
    }

    internal ParcelInput ToInput() => new(WeightGrams, LengthCm, WidthCm, HeightCm);
}
