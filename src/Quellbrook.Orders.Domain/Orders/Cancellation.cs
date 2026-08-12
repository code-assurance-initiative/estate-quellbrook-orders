namespace Quellbrook.Orders.Domain.Orders;

/// <summary>Who cancelled an order, when and why — as stored.</summary>
public sealed record Cancellation(string By, DateTimeOffset At, string Reason);
