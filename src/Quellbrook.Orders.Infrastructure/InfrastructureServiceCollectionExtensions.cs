using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quellbrook.Orders.Application.Queries;
using Quellbrook.Orders.Domain.Orders;
using Quellbrook.Orders.Infrastructure.Messaging;
using Quellbrook.Orders.Infrastructure.Outbox;
using Quellbrook.Orders.Infrastructure.Persistence;
using RabbitMQ.Client;

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
        services.AddSingleton<IConnectionFactory>(provider =>
            RabbitMqConnectionProvider.CreateFactory(provider.GetRequiredService<IOptions<RabbitMqOptions>>()));
        services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
        services.AddSingleton<IOutboxPublisher, RabbitMqPublisher>();
        services.AddOptions<OutboxOptions>()
            .BindConfiguration(OutboxOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddHostedService<OutboxRelay>();

        services.AddHealthChecks().AddDbContextCheck<OrdersDbContext>("database", tags: ["ready"]);
        return services;
    }
}
