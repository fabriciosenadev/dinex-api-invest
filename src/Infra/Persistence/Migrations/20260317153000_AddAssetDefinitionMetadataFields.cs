using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinExApi.Infra.Persistence.Migrations
{
    [DbContext(typeof(DinExDbContext))]
    [Migration("20260317153000_AddAssetDefinitionMetadataFields")]
    public partial class AddAssetDefinitionMetadataFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Administrator",
                table: "asset_definitions",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "asset_definitions",
                type: "TEXT",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "asset_definitions",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvmCode",
                table: "asset_definitions",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Document",
                table: "asset_definitions",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FiiCategory",
                table: "asset_definitions",
                type: "TEXT",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manager",
                table: "asset_definitions",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "asset_definitions",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "asset_definitions",
                type: "TEXT",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Segment",
                table: "asset_definitions",
                type: "TEXT",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShareClass",
                table: "asset_definitions",
                type: "TEXT",
                maxLength: 40,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Administrator", table: "asset_definitions");
            migrationBuilder.DropColumn(name: "Country", table: "asset_definitions");
            migrationBuilder.DropColumn(name: "Currency", table: "asset_definitions");
            migrationBuilder.DropColumn(name: "CvmCode", table: "asset_definitions");
            migrationBuilder.DropColumn(name: "Document", table: "asset_definitions");
            migrationBuilder.DropColumn(name: "FiiCategory", table: "asset_definitions");
            migrationBuilder.DropColumn(name: "Manager", table: "asset_definitions");
            migrationBuilder.DropColumn(name: "Name", table: "asset_definitions");
            migrationBuilder.DropColumn(name: "Sector", table: "asset_definitions");
            migrationBuilder.DropColumn(name: "Segment", table: "asset_definitions");
            migrationBuilder.DropColumn(name: "ShareClass", table: "asset_definitions");
        }
    }
}
