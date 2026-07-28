namespace Quellbrook.Orders.Domain.Orders;

public enum ServiceLevel
{
    /// <summary>Delivered on a planned route, normally the next working day.</summary>
    Standard = 0,

    /// <summary>Delivered the same day when placed before the dispatch cut-off.</summary>
    Express = 1,
}
