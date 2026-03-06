namespace DinExApi.Service;

public sealed record ForgotPasswordCommand(
    string Email) : ICommand<OperationResult>;
