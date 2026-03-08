namespace DinExApi.Service;

public sealed record GetStatementEntriesQuery(
    Guid UserId,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IQuery<OperationResult<IReadOnlyCollection<StatementEntryItem>>>;
