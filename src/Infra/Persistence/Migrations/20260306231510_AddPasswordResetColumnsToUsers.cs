using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinExApi.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetColumnsToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetCode",
                table: "users",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetCodeExpiresAtUtc",
                table: "users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_PasswordResetCode",
                table: "users",
                column: "PasswordResetCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_PasswordResetCode",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PasswordResetCode",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeExpiresAtUtc",
                table: "users");
        }
    }
}
