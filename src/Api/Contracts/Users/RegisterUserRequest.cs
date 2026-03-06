namespace DinExApi.Api.Contracts.Users;

public sealed record RegisterUserRequest(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword);
