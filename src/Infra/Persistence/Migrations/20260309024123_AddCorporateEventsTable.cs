using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinExApi.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorporateEventsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "corporate_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceAssetSymbol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TargetAssetSymbol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Factor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    EffectiveAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AppliedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_corporate_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_corporate_events_SourceAssetSymbol",
                table: "corporate_events",
                column: "SourceAssetSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_corporate_events_UserId_EffectiveAtUtc",
                table: "corporate_events",
                columns: new[] { "UserId", "EffectiveAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "corporate_events");
        }
    }
}
