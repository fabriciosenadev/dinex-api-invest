namespace DinExApi.Service;

public sealed record GetCorporateEventsQuery(Guid UserId) : IQuery<OperationResult<IReadOnlyCollection<CorporateEventItem>>>;
