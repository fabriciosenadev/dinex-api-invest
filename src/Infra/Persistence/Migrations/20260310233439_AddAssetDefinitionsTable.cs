using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinExApi.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetDefinitionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asset_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_definitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asset_definitions_UserId_Symbol",
                table: "asset_definitions",
                columns: new[] { "UserId", "Symbol" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_definitions");
        }
    }
}
