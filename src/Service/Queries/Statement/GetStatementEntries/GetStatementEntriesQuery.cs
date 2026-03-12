namespace DinExApi.Service;

public sealed record GetStatementEntriesQuery(
    Guid UserId,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    int Page = PaginationRequest.DefaultPage,
    int PageSize = PaginationRequest.DefaultPageSize) : IQuery<OperationResult<IReadOnlyCollection<StatementEntryItem>>>;
