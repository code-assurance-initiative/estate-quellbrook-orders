using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Quellbrook.Orders.Infrastructure.Persistence;

namespace Quellbrook.Orders.UnitTests.TestSupport;

/// <summary>A throw-away in-memory SQLite database with the service's schema.</summary>
internal sealed class SqliteOrdersDb : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteOrdersDb()
    {
        _connection.Open();
        using var db = CreateContext();
        db.Database.EnsureCreated();
    }

    public OrdersDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<OrdersDbContext>()
            .UseSqlite(_connection)
            .ReplaceService<IModelCustomizer, SqliteModelCustomizer>()
            .Options);

    public void Dispose() => _connection.Dispose();
}

/// <summary>SQLite cannot compare or order DateTimeOffset values; store them as ticks for the test database only.</summary>
internal sealed class SqliteModelCustomizer(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
        {
            if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
            {
                property.SetValueConverter(new DateTimeOffsetToBinaryConverter());
            }
        }
    }
}
