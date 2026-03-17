namespace DinExApi.Service;

public sealed record CorporateEventItem(
    Guid Id,
    CorporateEventType Type,
    string SourceAssetSymbol,
    string? TargetAssetSymbol,
    decimal Factor,
    decimal? CashPerSourceUnit,
    DateTime EffectiveAtUtc,
    string? Notes,
    DateTime AppliedAtUtc);
