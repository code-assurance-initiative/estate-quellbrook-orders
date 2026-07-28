using Quellbrook.Orders.Domain.Common;

namespace Quellbrook.Orders.Domain.Orders;

/// <summary>Outer dimensions of a parcel in whole centimetres.</summary>
public sealed record Dimensions
{
    public const int MaxSideCm = 175;

    private Dimensions(int lengthCm, int widthCm, int heightCm)
    {
        LengthCm = lengthCm;
        WidthCm = widthCm;
        HeightCm = heightCm;
    }

    public int LengthCm { get; }

    public int WidthCm { get; }

    public int HeightCm { get; }

    public static Dimensions Create(int lengthCm, int widthCm, int heightCm)
    {
        foreach (var side in (ReadOnlySpan<int>)[lengthCm, widthCm, heightCm])
        {
            if (side is < 1 or > MaxSideCm)
            {
                throw new DomainException($"Every side must be between 1 and {MaxSideCm} cm.");
            }
        }

        return new Dimensions(lengthCm, widthCm, heightCm);
    }
}
