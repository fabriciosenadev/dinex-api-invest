namespace DinExApi.Service;

public sealed record RegisterUserCommand(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword) : ICommand<OperationResult<Guid>>;
