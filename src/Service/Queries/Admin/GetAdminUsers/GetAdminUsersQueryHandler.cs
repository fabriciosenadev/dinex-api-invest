namespace DinExApi.Service;

public sealed class GetAdminUsersQueryHandler(
    IUserRepository userRepository) : IQueryHandler<GetAdminUsersQuery, OperationResult<IReadOnlyCollection<AdminUserItem>>>
{
    public async Task<OperationResult<IReadOnlyCollection<AdminUserItem>>> HandleAsync(
        GetAdminUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<IReadOnlyCollection<AdminUserItem>>();

        try
        {
            var users = await userRepository.ListAsync(cancellationToken);
            var items = users
                .Select(user => new AdminUserItem(
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.UserStatus,
                    user.UserRole,
                    user.CreatedAt,
                    user.UpdatedAt))
                .ToArray();

            result.SetData(items);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while retrieving admin users.");
            return result;
        }
    }
}
