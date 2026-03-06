namespace DinExApi.Api.Contracts.Users;

public sealed record AuthenticateUserRequest(
    string Email,
    string Password);
