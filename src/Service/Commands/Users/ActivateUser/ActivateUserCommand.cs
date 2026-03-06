namespace DinExApi.Service;

public sealed record ActivateUserCommand(
    string Email,
    string ActivationCode) : ICommand<OperationResult>;
