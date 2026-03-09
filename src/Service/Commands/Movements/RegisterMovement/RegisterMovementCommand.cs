
namespace DinExApi.Service;

public sealed record RegisterMovementCommand(
    Guid UserId,
    string AssetSymbol,
    OperationType Type,
    decimal Quantity,
    decimal UnitPrice,
    string Currency,
    DateTime OccurredAtUtc) : ICommand<OperationResult<Guid>>;
