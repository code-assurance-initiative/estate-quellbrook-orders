using Microsoft.EntityFrameworkCore;
using Quellbrook.Orders.Infrastructure.Persistence;

namespace Quellbrook.Orders.UnitTests.Infrastructure;

public sealed class DesignTimeDbContextFactoryTests
{
    [Fact]
    public void TheDesignTimeContextTargetsPostgreSQL()
    {
        using var context = new DesignTimeDbContextFactory().CreateDbContext([]);

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", context.Database.ProviderName);
    }
}
