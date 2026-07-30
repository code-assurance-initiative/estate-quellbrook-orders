namespace Quellbrook.Orders.Application.Queries;

/// <summary>The read side: lists orders without loading aggregates.</summary>
public interface IOrderQueries
{
    Task<OrderPage> ListAsync(int page, int pageSize);
}
