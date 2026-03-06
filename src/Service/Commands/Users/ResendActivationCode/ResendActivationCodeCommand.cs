namespace DinExApi.Service;

public sealed record ResendActivationCodeCommand(
    string Email) : ICommand<OperationResult>;
