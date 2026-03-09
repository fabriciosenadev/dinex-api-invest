namespace DinExApi.Api.Contracts.CorporateEvents;

public sealed record CorporateEventResponse(
    Guid Id,
    string Type,
    string SourceAssetSymbol,
    string? TargetAssetSymbol,
    decimal Factor,
    DateTime EffectiveAtUtc,
    string? Notes,
    DateTime AppliedAtUtc);
