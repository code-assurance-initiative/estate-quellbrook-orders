using Microsoft.Extensions.DependencyInjection;
using Quellbrook.Orders.Application.CancelOrder;
using Quellbrook.Orders.Application.PlaceOrder;

namespace Quellbrook.Orders.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersApplication(this IServiceCollection services)
    {
        services.AddScoped<PlaceOrderHandler>();
        services.AddScoped<CancelOrderHandler>();
        return services;
    }
}
