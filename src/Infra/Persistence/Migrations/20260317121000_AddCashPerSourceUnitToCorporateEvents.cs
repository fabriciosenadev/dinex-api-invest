using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DinExApi.Infra.Persistence.Migrations;

[DbContext(typeof(DinExDbContext))]
[Migration("20260317121000_AddCashPerSourceUnitToCorporateEvents")]
public sealed class AddCashPerSourceUnitToCorporateEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "CashPerSourceUnit",
            table: "corporate_events",
            precision: 18,
            scale: 6,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CashPerSourceUnit",
            table: "corporate_events");
    }
}
