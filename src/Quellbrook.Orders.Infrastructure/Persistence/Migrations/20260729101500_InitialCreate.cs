using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quellbrook.Orders.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerAccountId = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ServiceLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ConsigneeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DestinationCity = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    ParcelCount = table.Column<int>(type: "integer", nullable: false),
                    Consignee = table.Column<string>(type: "jsonb", nullable: false),
                    Parcels = table.Column<string>(type: "jsonb", nullable: false),
                    PlacedBy = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PlacedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_orders_PlacedAt",
                table: "orders",
                column: "PlacedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orders");
        }
    }
}
