namespace DinExApi.Api.Contracts.CorporateEvents;

public sealed record RegisterCorporateEventRequest(
    string Type,
    string SourceAssetSymbol,
    string? TargetAssetSymbol,
    decimal Factor,
    DateTime EffectiveAtUtc,
    string? Notes);
