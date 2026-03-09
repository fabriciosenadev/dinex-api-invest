namespace DinExApi.Core;

public sealed record ImportedSpreadsheetRow(
    int RowNumber,
    DateTime OccurredAtUtc,
    string EventDescription,
    string? MovementDetail,
    string? Direction,
    string? AssetSymbol,
    decimal? Quantity,
    decimal? UnitPriceAmount,
    decimal? GrossAmount,
    decimal? NetAmount,
    string Currency,
    string FileName);
