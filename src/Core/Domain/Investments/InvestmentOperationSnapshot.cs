namespace DinExApi.Core;

public sealed record InvestmentOperationSnapshot(
    string AssetSymbol,
    OperationType Type,
    decimal Quantity,
    decimal UnitPriceAmount,
    string Currency,
    DateTime OccurredAtUtc);
