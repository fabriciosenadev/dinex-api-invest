using Microsoft.EntityFrameworkCore.Migrations;

namespace DinExApi.Infra.Persistence.Migrations;

public sealed partial class AddUserRoleToUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "UserRole",
            table: "users",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "UserRole",
            table: "users");
    }
}
