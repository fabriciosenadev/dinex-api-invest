using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinExApi.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenColumnsToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiresAtUtc",
                table: "users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenHash",
                table: "users",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_RefreshTokenHash",
                table: "users",
                column: "RefreshTokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_RefreshTokenHash",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiresAtUtc",
                table: "users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenHash",
                table: "users");
        }
    }
}
