using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quellbrook.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrdersStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_orders_Status_PlacedAt",
                table: "orders",
                columns: new[] { "Status", "PlacedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_orders_Status_PlacedAt",
                table: "orders");
        }
    }
}
