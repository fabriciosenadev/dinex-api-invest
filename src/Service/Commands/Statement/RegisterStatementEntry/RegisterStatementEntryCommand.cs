namespace DinExApi.Service;

public sealed record RegisterStatementEntryCommand(
    Guid UserId,
    LedgerEntryType Type,
    string? Description,
    decimal GrossAmount,
    decimal NetAmount,
    string Currency,
    DateTime OccurredAtUtc,
    string Source,
    string? AssetSymbol = null,
    decimal? Quantity = null,
    decimal? UnitPriceAmount = null,
    string? ReferenceId = null,
    string? Metadata = null) : ICommand<OperationResult<Guid>>;
