using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTrackerApi.Migrations;

/// <inheritdoc />
public partial class _20261126011320_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Type = table.Column<string>(type: "TEXT", nullable: false),
                Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Transactions", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Transactions");
    }
}
