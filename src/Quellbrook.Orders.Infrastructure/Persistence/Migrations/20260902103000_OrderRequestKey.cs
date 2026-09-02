using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quellbrook.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrderRequestKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestKey",
                table: "orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_RequestKey",
                table: "orders",
                column: "RequestKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_orders_RequestKey",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "RequestKey",
                table: "orders");
        }
    }
}
