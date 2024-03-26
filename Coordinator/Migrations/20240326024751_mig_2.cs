using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Coordinator.Migrations
{
    /// <inheritdoc />
    public partial class mig_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("068b7243-0417-4f67-819b-c4c74d11f3c5"), "Payment.API" },
                    { new Guid("be19ecf4-d661-4a13-9b15-4396789bb983"), "Order.API" },
                    { new Guid("c583ceac-bcef-4937-8e07-71ebc45729b1"), "Stock.API" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("068b7243-0417-4f67-819b-c4c74d11f3c5"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("be19ecf4-d661-4a13-9b15-4396789bb983"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("c583ceac-bcef-4937-8e07-71ebc45729b1"));
        }
    }
}
