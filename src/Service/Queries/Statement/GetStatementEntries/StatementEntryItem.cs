namespace DinExApi.Service;

public sealed record StatementEntryItem(
    Guid Id,
    LedgerEntryType Type,
    string Description,
    string? AssetSymbol,
    decimal? Quantity,
    decimal? UnitPriceAmount,
    decimal GrossAmount,
    decimal NetAmount,
    string Currency,
    DateTime OccurredAtUtc,
    string Source,
    string? ReferenceId,
    string? Metadata);
