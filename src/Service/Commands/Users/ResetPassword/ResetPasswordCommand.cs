namespace DinExApi.Service;

public sealed record ResetPasswordCommand(
    string Email,
    string Code,
    string NewPassword,
    string ConfirmNewPassword) : ICommand<OperationResult>;
