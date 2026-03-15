namespace DinExApi.Service;

public sealed class GetCurrentUserQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetCurrentUserQuery, OperationResult<CurrentUserItem>>
{
    public async Task<OperationResult<CurrentUserItem>> HandleAsync(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<CurrentUserItem>();

        try
        {
            var user = await userRepository.GetByIdAsync(query.UserId, cancellationToken);
            if (user is null)
            {
                result.SetAsNotFound();
                result.AddError("User not found.");
                return result;
            }

            result.SetData(new CurrentUserItem(
                user.Id,
                user.FullName,
                user.Email,
                user.UserStatus,
                user.UserRole,
                user.CreatedAt,
                user.UpdatedAt));
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while getting current user.");
            return result;
        }
    }
}
