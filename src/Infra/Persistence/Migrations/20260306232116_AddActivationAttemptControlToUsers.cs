using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DinExApi.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActivationAttemptControlToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationCodeExpiresAtUtc",
                table: "users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActivationCodeFailedAttempts",
                table: "users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivationCodeExpiresAtUtc",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ActivationCodeFailedAttempts",
                table: "users");
        }
    }
}
