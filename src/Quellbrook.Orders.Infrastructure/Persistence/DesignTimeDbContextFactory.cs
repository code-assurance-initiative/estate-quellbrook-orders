using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Quellbrook.Orders.Infrastructure.Persistence;

/// <summary>
/// Used by <c>dotnet ef</c> and by the migrations bundle. The connection string comes from the environment, never from
/// the command line, so it does not show up in process listings or CI logs.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Orders")
            ?? "Host=localhost;Database=orders;Username=orders";
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new OrdersDbContext(options);
    }
}
