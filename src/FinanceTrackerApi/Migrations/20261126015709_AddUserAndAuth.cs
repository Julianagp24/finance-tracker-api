using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTrackerApi.Migrations;

/// <inheritdoc />
public partial class _20261126015709_AddUserAndAuth : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "UserId",
            table: "Transactions",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_UserId",
            table: "Transactions",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Transactions_Users_UserId",
            table: "Transactions",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Transactions_Users_UserId",
            table: "Transactions");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Transactions_UserId",
            table: "Transactions");

        migrationBuilder.DropColumn(
            name: "UserId",
            table: "Transactions");
    }
}
