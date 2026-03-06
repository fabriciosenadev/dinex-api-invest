namespace DinExApi.Service;

public sealed class LogoutUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<LogoutUserCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(LogoutUserCommand command, CancellationToken cancellationToken = default)
    {
        var result = new OperationResult();

        try
        {
            var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user is null)
            {
                result.SetAsNotFound();
                result.AddError("User not found.");
                return result;
            }

            user.RevokeRefreshToken();
            await userRepository.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while logging out user.");
            return result;
        }
    }
}
