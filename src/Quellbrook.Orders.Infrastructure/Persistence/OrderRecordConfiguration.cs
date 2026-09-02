using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Quellbrook.Orders.Infrastructure.Persistence;

internal sealed class OrderRecordConfiguration : IEntityTypeConfiguration<OrderRecord>
{
    public void Configure(EntityTypeBuilder<OrderRecord> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.CustomerAccountId).HasMaxLength(16).IsRequired();
        builder.Property(order => order.ServiceLevel).HasMaxLength(16).IsRequired();
        builder.Property(order => order.Status).HasMaxLength(16).IsRequired();
        builder.Property(order => order.ConsigneeName).HasMaxLength(100).IsRequired();
        builder.Property(order => order.DestinationCity).HasMaxLength(60).IsRequired();
        builder.Property(order => order.Consignee).HasColumnType("jsonb").IsRequired();
        builder.Property(order => order.Parcels).HasColumnType("jsonb").IsRequired();
        builder.Property(order => order.PlacedBy).HasMaxLength(64).IsRequired();
        builder.Property(order => order.RequestKey).HasMaxLength(64);
        builder.HasIndex(order => order.RequestKey).IsUnique();
        builder.Property(order => order.CancelledBy).HasMaxLength(64);
        builder.Property(order => order.CancellationReason).HasMaxLength(200);
        builder.Property(order => order.Version).IsConcurrencyToken();
        builder.HasIndex(order => order.PlacedAt);
        builder.HasIndex(order => new { order.Status, order.PlacedAt });
    }
}
