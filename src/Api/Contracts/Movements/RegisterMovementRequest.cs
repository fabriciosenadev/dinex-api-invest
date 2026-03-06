
namespace DinExApi.Api.Contracts.Movements;

public sealed record RegisterMovementRequest(
    string AssetSymbol,
    OperationType Type,
    decimal Quantity,
    decimal UnitPrice,
    string Currency,
    DateTime? OccurredAtUtc);
