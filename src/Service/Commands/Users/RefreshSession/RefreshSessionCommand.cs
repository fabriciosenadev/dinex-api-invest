namespace DinExApi.Service;

public sealed record RefreshSessionCommand(
    string RefreshToken) : ICommand<OperationResult<AuthenticatedUserResult>>;
