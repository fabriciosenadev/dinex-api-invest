namespace DinExApi.Api.Controllers;

[Route("api/admin/users")]
[Authorize(Roles = nameof(UserRole.Admin))]
public sealed class AdminUsersController(IApplicationDispatcher dispatcher) : MainController
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<AdminUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> List(CancellationToken cancellationToken)
    {
        var result = await dispatcher.QueryAsync<GetAdminUsersQuery, OperationResult<IReadOnlyCollection<AdminUserItem>>>(
            new GetAdminUsersQuery(),
            cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<IReadOnlyCollection<AdminUserResponse>>();
        mapped.SetData(result.Data
            .Select(user => new AdminUserResponse(
                user.UserId,
                user.FullName,
                user.Email,
                user.UserStatus.ToString(),
                user.UserRole.ToString(),
                user.CreatedAtUtc,
                user.UpdatedAtUtc))
            .ToArray());

        return HandleResult(mapped);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Invite(
        [FromBody] InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserRole>(request.UserRole, true, out var userRole))
        {
            return BadRequest(new ErrorResponse(["UserRole must be User or Admin."]));
        }

        var command = new InviteUserCommand(
            request.FullName,
            request.Email,
            userRole);

        var result = await dispatcher.SendAsync<InviteUserCommand, OperationResult<Guid>>(command, cancellationToken);
        return HandleResult(result);
    }
}
