namespace DinExApi.Api.Contracts.Statement;

public sealed record StatementEntryResponse(
    Guid Id,
    string Type,
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
