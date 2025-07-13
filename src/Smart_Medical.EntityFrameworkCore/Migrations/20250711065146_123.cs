using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Medical.Migrations
{
    /// <inheritdoc />
    public partial class _123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrmName",
                table: "AppDrugInStocks");

            migrationBuilder.AddColumn<Guid>(
                name: "PharmaceuticalCompanyId",
                table: "AppDrugInStocks",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PharmaceuticalCompanyId",
                table: "AppDrugInStocks");

            migrationBuilder.AddColumn<string>(
                name: "OrmName",
                table: "AppDrugInStocks",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
