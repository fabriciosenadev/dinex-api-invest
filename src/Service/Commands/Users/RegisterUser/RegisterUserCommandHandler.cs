namespace DinExApi.Service;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IUserPasswordHasher passwordHasher,
    IUserActivationEmailSender activationEmailSender,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterUserCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<Guid>();

        try
        {
            var existingUser = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (existingUser is not null)
            {
                result.AddError("A user with this email already exists.");
                return result;
            }

            var user = User.CreateUser(command.FullName, command.Email, command.Password, command.ConfirmPassword);
            if (!user.IsValid)
            {
                result.AddNotifications(user.Notifications);
                return result;
            }

            user.SetPasswordHash(passwordHasher.HashPassword(command.Password));
            if (!user.IsValid)
            {
                result.AddNotifications(user.Notifications);
                return result;
            }

            user.AssignActivationCode(Guid.NewGuid().ToString("N")[..8].ToUpperInvariant());

            await userRepository.AddAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await activationEmailSender.SendActivationCodeAsync(
                user.FullName,
                user.Email,
                user.ActivationCode ?? string.Empty,
                cancellationToken);

            result.SetData(user.Id);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while registering user.");
            return result;
        }
    }
}
