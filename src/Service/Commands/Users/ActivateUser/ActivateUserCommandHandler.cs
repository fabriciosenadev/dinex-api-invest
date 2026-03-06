namespace DinExApi.Service;

public sealed class ActivateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ActivateUserCommand, OperationResult>
{
    private const int MaxActivationAttempts = 5;

    public async Task<OperationResult> HandleAsync(ActivateUserCommand command, CancellationToken cancellationToken = default)
    {
        var result = new OperationResult();

        try
        {
            var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (user is null)
            {
                result.SetAsNotFound();
                result.AddError("User not found.");
                return result;
            }

            if (user.UserStatus == UserStatus.Active)
            {
                result.AddError("Account is already active.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(user.ActivationCode))
            {
                result.AddError("Activation code is not available.");
                return result;
            }

            if (user.HasExceededActivationAttempts(MaxActivationAttempts))
            {
                result.AddError("Maximum activation attempts reached. Request a new activation code.");
                return result;
            }

            var providedCode = command.ActivationCode.Trim().ToUpperInvariant();
            if (!string.Equals(user.ActivationCode, providedCode, StringComparison.Ordinal))
            {
                var reachedMaxAttempts = user.RegisterActivationFailure(MaxActivationAttempts);
                await userRepository.UpdateAsync(user, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                if (reachedMaxAttempts)
                {
                    result.AddError("Maximum activation attempts reached. Request a new activation code.");
                    return result;
                }

                result.AddError("Invalid activation code.");
                return result;
            }

            if (!user.HasValidActivationCode())
            {
                result.AddError("Activation code expired. Request a new code.");
                return result;
            }

            user.Activate();
            await userRepository.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while activating user.");
            return result;
        }
    }
}
