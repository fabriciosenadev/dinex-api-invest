namespace DinExApi.Service;

public sealed record InviteUserCommand(
    string FullName,
    string Email,
    UserRole UserRole) : ICommand<OperationResult<Guid>>;
