namespace DinExApi.Service;

public sealed record CorporateEventItem(
    Guid Id,
    CorporateEventType Type,
    string SourceAssetSymbol,
    string? TargetAssetSymbol,
    decimal Factor,
    DateTime EffectiveAtUtc,
    string? Notes,
    DateTime AppliedAtUtc);
