namespace DinExApi.Service;

public sealed record GetAdminUsersQuery : IQuery<OperationResult<IReadOnlyCollection<AdminUserItem>>>;
