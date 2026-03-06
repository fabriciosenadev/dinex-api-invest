namespace DinExApi.Api.Contracts.Users;

public sealed record ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword,
    string ConfirmNewPassword);
