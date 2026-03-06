namespace DinExApi.Service;

public sealed class AuthenticateUserCommandHandler(
    IUserRepository userRepository,
    IUserPasswordHasher passwordHasher,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork) : ICommandHandler<AuthenticateUserCommand, OperationResult<AuthenticatedUserResult>>
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LoginLockoutDuration = TimeSpan.FromMinutes(15);

    public async Task<OperationResult<AuthenticatedUserResult>> HandleAsync(
        AuthenticateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<AuthenticatedUserResult>();

        try
        {
            var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (user is null)
            {
                result.AddError("Invalid credentials.");
                return result;
            }

            if (user.UserStatus != UserStatus.Active)
            {
                result.AddError("Account is not active.");
                return result;
            }

            if (user.IsLoginLocked(DateTime.UtcNow))
            {
                result.AddError("Too many failed attempts. Try again later.");
                return result;
            }

            var isPasswordValid = passwordHasher.VerifyPassword(user.Password, command.Password);
            if (!isPasswordValid)
            {
                user.RegisterFailedLogin(MaxFailedLoginAttempts, LoginLockoutDuration, DateTime.UtcNow);
                await userRepository.UpdateAsync(user, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                result.AddError("Invalid credentials.");
                return result;
            }

            user.RegisterSuccessfulLogin(DateTime.UtcNow);

            var refreshToken = refreshTokenService.Generate();
            user.AssignRefreshToken(refreshToken.TokenHash, refreshToken.ExpiresAtUtc);
            if (!user.IsValid)
            {
                result.AddNotifications(user.Notifications);
                return result;
            }

            await userRepository.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var tokenResult = accessTokenService.Generate(user);
            result.SetData(new AuthenticatedUserResult(
                UserId: user.Id,
                FullName: user.FullName,
                Email: user.Email,
                AccessToken: tokenResult.AccessToken,
                ExpiresAtUtc: tokenResult.ExpiresAtUtc,
                RefreshToken: refreshToken.Token,
                RefreshTokenExpiresAtUtc: refreshToken.ExpiresAtUtc));

            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while authenticating user.");
            return result;
        }
    }
}
