using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quellbrook.Orders.Application.Abstractions;
using Quellbrook.Orders.Application.Queries;
using Quellbrook.Orders.Domain.Orders;
using Quellbrook.Orders.Infrastructure.Messaging;
using Quellbrook.Orders.Infrastructure.Persistence;

namespace Quellbrook.Orders.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Orders")));
        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddScoped<IOrderQueries, OrderQueries>();

        services.AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
        services.AddSingleton<IIntegrationEventPublisher, RabbitMqPublisher>();

        services.AddHealthChecks().AddDbContextCheck<OrdersDbContext>("database", tags: ["ready"]);
        return services;
    }
}
