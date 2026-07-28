namespace Quellbrook.Orders.Domain.Common;

/// <summary>Trimming and length rules shared by the value objects.</summary>
internal static class Text
{
    public static string Required(string? value, int maxLength, string field)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw new DomainException($"{field} is required.");
        }

        return trimmed.Length <= maxLength
            ? trimmed
            : throw new DomainException($"{field} must be at most {maxLength} characters.");
    }

    public static string? Optional(string? value, int maxLength, string field) =>
        string.IsNullOrWhiteSpace(value) ? null : Required(value, maxLength, field);
}
