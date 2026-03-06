namespace DinExApi.Service;

public sealed record AuthenticateUserCommand(
    string Email,
    string Password) : ICommand<OperationResult<AuthenticatedUserResult>>;
