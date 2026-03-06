namespace DinExApi.Service;

public sealed class RefreshSessionCommandHandler(
    IUserRepository userRepository,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork) : ICommandHandler<RefreshSessionCommand, OperationResult<AuthenticatedUserResult>>
{
    public async Task<OperationResult<AuthenticatedUserResult>> HandleAsync(
        RefreshSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<AuthenticatedUserResult>();

        try
        {
            if (string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                result.AddError("Refresh token is required.");
                return result;
            }

            var providedTokenHash = refreshTokenService.ComputeHash(command.RefreshToken);
            var user = await userRepository.GetByRefreshTokenHashAsync(providedTokenHash, cancellationToken);
            if (user is null)
            {
                result.AddError("Invalid refresh token.");
                return result;
            }

            if (user.UserStatus != UserStatus.Active)
            {
                result.AddError("Account is not active.");
                return result;
            }

            if (!user.HasValidRefreshToken(providedTokenHash, DateTime.UtcNow))
            {
                result.AddError("Refresh token expired or invalid.");
                return result;
            }

            var newRefreshToken = refreshTokenService.Generate();
            user.AssignRefreshToken(newRefreshToken.TokenHash, newRefreshToken.ExpiresAtUtc);
            if (!user.IsValid)
            {
                result.AddNotifications(user.Notifications);
                return result;
            }

            await userRepository.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var accessToken = accessTokenService.Generate(user);
            result.SetData(new AuthenticatedUserResult(
                UserId: user.Id,
                FullName: user.FullName,
                Email: user.Email,
                AccessToken: accessToken.AccessToken,
                ExpiresAtUtc: accessToken.ExpiresAtUtc,
                RefreshToken: newRefreshToken.Token,
                RefreshTokenExpiresAtUtc: newRefreshToken.ExpiresAtUtc));

            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while refreshing session.");
            return result;
        }
    }
}
