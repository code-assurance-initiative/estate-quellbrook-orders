namespace Quellbrook.Orders.Domain.Orders;

/// <summary>One physical parcel of an order, numbered from 1 within the order.</summary>
public sealed class Parcel
{
    private Parcel(int number, int weightGrams, Dimensions dimensions)
    {
        Number = number;
        WeightGrams = weightGrams;
        Dimensions = dimensions;
    }

    public int Number { get; }

    public int WeightGrams { get; set; }

    public Dimensions Dimensions { get; set; }

    public static Parcel Restore(int number, int weightGrams, Dimensions dimensions) => new(number, weightGrams, dimensions);

    internal static Parcel Create(int number, ParcelSpecification specification) =>
        new(number, specification.WeightGrams, specification.Dimensions);
}
