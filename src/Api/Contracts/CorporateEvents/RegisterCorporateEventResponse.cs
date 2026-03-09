namespace DinExApi.Api.Contracts.CorporateEvents;

public sealed record RegisterCorporateEventResponse(Guid EventId, int AffectedOperations);
