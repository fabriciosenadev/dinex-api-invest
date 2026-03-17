namespace DinExApi.Service;

public sealed record RegisterCorporateEventCommand(
    Guid UserId,
    CorporateEventType Type,
    string SourceAssetSymbol,
    string? TargetAssetSymbol,
    decimal Factor,
    decimal? CashPerSourceUnit,
    DateTime EffectiveAtUtc,
    string? Notes) : ICommand<OperationResult<RegisterCorporateEventResult>>;
