namespace DinExApi.Service;

public sealed record GetCurrentUserQuery(Guid UserId) : IQuery<OperationResult<CurrentUserItem>>;
