namespace DinExApi.Service;

public sealed record UpdateCorporateEventCommand(
    Guid UserId,
    Guid EventId,
    CorporateEventType Type,
    string SourceAssetSymbol,
    string? TargetAssetSymbol,
    decimal Factor,
    DateTime EffectiveAtUtc,
    string? Notes) : ICommand<OperationResult<RegisterCorporateEventResult>>;
