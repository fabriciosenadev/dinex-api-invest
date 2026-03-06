namespace DinExApi.Service;

public sealed record LogoutUserCommand(
    Guid UserId) : ICommand<OperationResult>;
