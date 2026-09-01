namespace Quellbrook.Orders.Domain.Common;

/// <summary>A business rule was broken; the message is safe to show to the caller.</summary>
public sealed class DomainException(string message) : Exception(message);
