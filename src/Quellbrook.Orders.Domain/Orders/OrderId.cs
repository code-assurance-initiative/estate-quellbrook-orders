namespace Quellbrook.Orders.Domain.Orders;

public readonly record struct OrderId(Guid Value)
{
    public override string ToString() => Value.ToString();
}
