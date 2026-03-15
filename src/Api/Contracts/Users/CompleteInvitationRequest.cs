namespace DinExApi.Api.Contracts.Users;

public sealed record CompleteInvitationRequest(
    string Email,
    string ActivationCode,
    string Password,
    string ConfirmPassword);
