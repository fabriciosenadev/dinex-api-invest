namespace DinExApi.Service;

public sealed record GetCorporateEventsQuery(
    Guid UserId,
    int Page = PaginationRequest.DefaultPage,
    int PageSize = PaginationRequest.DefaultPageSize) : IQuery<OperationResult<IReadOnlyCollection<CorporateEventItem>>>;
