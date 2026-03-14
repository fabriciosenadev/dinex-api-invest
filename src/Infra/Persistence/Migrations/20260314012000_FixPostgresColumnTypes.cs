using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DinExApi.Infra.Persistence.Migrations;

[DbContext(typeof(DinExDbContext))]
[Migration("20260314012000_FixPostgresColumnTypes")]
public sealed class FixPostgresColumnTypes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (!ActiveProvider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        migrationBuilder.Sql("""
            ALTER TABLE users
                ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid,
                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt"::timestamptz,
                ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone USING "UpdatedAt"::timestamptz,
                ALTER COLUMN "DeletedAt" TYPE timestamp with time zone USING "DeletedAt"::timestamptz,
                ALTER COLUMN "RefreshTokenExpiresAtUtc" TYPE timestamp with time zone USING "RefreshTokenExpiresAtUtc"::timestamptz,
                ALTER COLUMN "PasswordResetCodeExpiresAtUtc" TYPE timestamp with time zone USING "PasswordResetCodeExpiresAtUtc"::timestamptz,
                ALTER COLUMN "ActivationCodeExpiresAtUtc" TYPE timestamp with time zone USING "ActivationCodeExpiresAtUtc"::timestamptz,
                ALTER COLUMN "LoginLockedUntilUtc" TYPE timestamp with time zone USING "LoginLockedUntilUtc"::timestamptz;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE investment_operations
                ALTER COLUMN "UserId" DROP DEFAULT;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE investment_operations
                ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid,
                ALTER COLUMN "UserId" TYPE uuid USING "UserId"::uuid,
                ALTER COLUMN "Quantity" TYPE numeric(18,6) USING "Quantity"::numeric(18,6),
                ALTER COLUMN "UnitPriceAmount" TYPE numeric(18,6) USING "UnitPriceAmount"::numeric(18,6),
                ALTER COLUMN "OccurredAtUtc" TYPE timestamp with time zone USING "OccurredAtUtc"::timestamptz;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE ledger_entries
                ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid,
                ALTER COLUMN "UserId" TYPE uuid USING "UserId"::uuid,
                ALTER COLUMN "Quantity" TYPE numeric(18,6) USING "Quantity"::numeric(18,6),
                ALTER COLUMN "UnitPriceAmount" TYPE numeric(18,6) USING "UnitPriceAmount"::numeric(18,6),
                ALTER COLUMN "GrossAmount" TYPE numeric(18,6) USING "GrossAmount"::numeric(18,6),
                ALTER COLUMN "NetAmount" TYPE numeric(18,6) USING "NetAmount"::numeric(18,6),
                ALTER COLUMN "OccurredAtUtc" TYPE timestamp with time zone USING "OccurredAtUtc"::timestamptz,
                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt"::timestamptz,
                ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone USING "UpdatedAt"::timestamptz,
                ALTER COLUMN "DeletedAt" TYPE timestamp with time zone USING "DeletedAt"::timestamptz;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE corporate_events
                ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid,
                ALTER COLUMN "UserId" TYPE uuid USING "UserId"::uuid,
                ALTER COLUMN "Factor" TYPE numeric(18,6) USING "Factor"::numeric(18,6),
                ALTER COLUMN "EffectiveAtUtc" TYPE timestamp with time zone USING "EffectiveAtUtc"::timestamptz,
                ALTER COLUMN "AppliedAtUtc" TYPE timestamp with time zone USING "AppliedAtUtc"::timestamptz,
                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt"::timestamptz,
                ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone USING "UpdatedAt"::timestamptz,
                ALTER COLUMN "DeletedAt" TYPE timestamp with time zone USING "DeletedAt"::timestamptz;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE asset_definitions
                ALTER COLUMN "Id" TYPE uuid USING "Id"::uuid,
                ALTER COLUMN "UserId" TYPE uuid USING "UserId"::uuid,
                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone USING "CreatedAt"::timestamptz,
                ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone USING "UpdatedAt"::timestamptz,
                ALTER COLUMN "DeletedAt" TYPE timestamp with time zone USING "DeletedAt"::timestamptz;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (!ActiveProvider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        migrationBuilder.Sql("""
            ALTER TABLE users
                ALTER COLUMN "Id" TYPE text USING "Id"::text,
                ALTER COLUMN "CreatedAt" TYPE text USING "CreatedAt"::text,
                ALTER COLUMN "UpdatedAt" TYPE text USING "UpdatedAt"::text,
                ALTER COLUMN "DeletedAt" TYPE text USING "DeletedAt"::text,
                ALTER COLUMN "RefreshTokenExpiresAtUtc" TYPE text USING "RefreshTokenExpiresAtUtc"::text,
                ALTER COLUMN "PasswordResetCodeExpiresAtUtc" TYPE text USING "PasswordResetCodeExpiresAtUtc"::text,
                ALTER COLUMN "ActivationCodeExpiresAtUtc" TYPE text USING "ActivationCodeExpiresAtUtc"::text,
                ALTER COLUMN "LoginLockedUntilUtc" TYPE text USING "LoginLockedUntilUtc"::text;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE investment_operations
                ALTER COLUMN "Id" TYPE text USING "Id"::text,
                ALTER COLUMN "UserId" TYPE text USING "UserId"::text,
                ALTER COLUMN "Quantity" TYPE text USING "Quantity"::text,
                ALTER COLUMN "UnitPriceAmount" TYPE text USING "UnitPriceAmount"::text,
                ALTER COLUMN "OccurredAtUtc" TYPE text USING "OccurredAtUtc"::text;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE ledger_entries
                ALTER COLUMN "Id" TYPE text USING "Id"::text,
                ALTER COLUMN "UserId" TYPE text USING "UserId"::text,
                ALTER COLUMN "Quantity" TYPE text USING "Quantity"::text,
                ALTER COLUMN "UnitPriceAmount" TYPE text USING "UnitPriceAmount"::text,
                ALTER COLUMN "GrossAmount" TYPE text USING "GrossAmount"::text,
                ALTER COLUMN "NetAmount" TYPE text USING "NetAmount"::text,
                ALTER COLUMN "OccurredAtUtc" TYPE text USING "OccurredAtUtc"::text,
                ALTER COLUMN "CreatedAt" TYPE text USING "CreatedAt"::text,
                ALTER COLUMN "UpdatedAt" TYPE text USING "UpdatedAt"::text,
                ALTER COLUMN "DeletedAt" TYPE text USING "DeletedAt"::text;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE corporate_events
                ALTER COLUMN "Id" TYPE text USING "Id"::text,
                ALTER COLUMN "UserId" TYPE text USING "UserId"::text,
                ALTER COLUMN "Factor" TYPE text USING "Factor"::text,
                ALTER COLUMN "EffectiveAtUtc" TYPE text USING "EffectiveAtUtc"::text,
                ALTER COLUMN "AppliedAtUtc" TYPE text USING "AppliedAtUtc"::text,
                ALTER COLUMN "CreatedAt" TYPE text USING "CreatedAt"::text,
                ALTER COLUMN "UpdatedAt" TYPE text USING "UpdatedAt"::text,
                ALTER COLUMN "DeletedAt" TYPE text USING "DeletedAt"::text;
            """);

        migrationBuilder.Sql("""
            ALTER TABLE asset_definitions
                ALTER COLUMN "Id" TYPE text USING "Id"::text,
                ALTER COLUMN "UserId" TYPE text USING "UserId"::text,
                ALTER COLUMN "CreatedAt" TYPE text USING "CreatedAt"::text,
                ALTER COLUMN "UpdatedAt" TYPE text USING "UpdatedAt"::text,
                ALTER COLUMN "DeletedAt" TYPE text USING "DeletedAt"::text;
            """);
    }
}
