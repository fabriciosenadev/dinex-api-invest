namespace DinExApi.Api.Contracts.Users;

public sealed record ActivateUserRequest(
    string Email,
    string ActivationCode);
