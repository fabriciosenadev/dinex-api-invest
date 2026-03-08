using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinExApi.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateLedgerEntriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ledger_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    AssetSymbol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true),
                    UnitPriceAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: true),
                    GrossAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    NetAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    ReferenceId = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_AssetSymbol",
                table: "ledger_entries",
                column: "AssetSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_UserId_OccurredAtUtc",
                table: "ledger_entries",
                columns: new[] { "UserId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_UserId_Source_ReferenceId",
                table: "ledger_entries",
                columns: new[] { "UserId", "Source", "ReferenceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ledger_entries");
        }
    }
}
