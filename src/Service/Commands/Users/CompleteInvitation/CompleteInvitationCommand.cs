namespace DinExApi.Service;

public sealed record CompleteInvitationCommand(
    string Email,
    string ActivationCode,
    string Password,
    string ConfirmPassword) : ICommand<OperationResult>;
