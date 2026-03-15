namespace DinExApi.Api.Contracts.Admin.Users;

public sealed record InviteUserRequest(
    string FullName,
    string Email,
    string UserRole);
