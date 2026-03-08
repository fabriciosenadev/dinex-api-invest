namespace DinExApi.Api.Contracts.Statement;

public sealed record RegisterStatementEntryRequest(
    LedgerEntryType Type,
    string Description,
    decimal GrossAmount,
    decimal NetAmount,
    string Currency,
    DateTime? OccurredAtUtc,
    string Source,
    string? AssetSymbol,
    decimal? Quantity,
    decimal? UnitPriceAmount,
    string? ReferenceId,
    string? Metadata);
