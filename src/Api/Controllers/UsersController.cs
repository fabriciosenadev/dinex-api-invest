namespace DinExApi.Api.Controllers;

[Route("api/[controller]")]
public sealed class UsersController(IApplicationDispatcher dispatcher) : MainController
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var result = await dispatcher.QueryAsync<GetCurrentUserQuery, OperationResult<CurrentUserItem>>(
            new GetCurrentUserQuery(userId),
            cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<CurrentUserResponse>();
        mapped.SetData(new CurrentUserResponse(
            result.Data.UserId,
            result.Data.FullName,
            result.Data.Email,
            result.Data.UserStatus.ToString(),
            result.Data.CreatedAtUtc,
            result.Data.UpdatedAtUtc));

        return HandleResult(mapped);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.FullName,
            request.Email,
            request.Password,
            request.ConfirmPassword);

        var result = await dispatcher.SendAsync<RegisterUserCommand, OperationResult<Guid>>(command, cancellationToken);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    [ProducesResponseType(typeof(AuthenticateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Authenticate(
        [FromBody] AuthenticateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AuthenticateUserCommand(request.Email, request.Password);
        var result = await dispatcher.SendAsync<AuthenticateUserCommand, OperationResult<AuthenticatedUserResult>>(command, cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<AuthenticateUserResponse>();
        mapped.SetData(new AuthenticateUserResponse(
            result.Data.UserId,
            result.Data.FullName,
            result.Data.Email,
            result.Data.AccessToken,
            result.Data.ExpiresAtUtc,
            result.Data.RefreshToken,
            result.Data.RefreshTokenExpiresAtUtc));

        return HandleResult(mapped);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthenticateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RefreshToken(
        [FromBody] RefreshSessionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshSessionCommand(request.RefreshToken);
        var result = await dispatcher.SendAsync<RefreshSessionCommand, OperationResult<AuthenticatedUserResult>>(command, cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<AuthenticateUserResponse>();
        mapped.SetData(new AuthenticateUserResponse(
            result.Data.UserId,
            result.Data.FullName,
            result.Data.Email,
            result.Data.AccessToken,
            result.Data.ExpiresAtUtc,
            result.Data.RefreshToken,
            result.Data.RefreshTokenExpiresAtUtc));

        return HandleResult(mapped);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var command = new LogoutUserCommand(userId);
        var result = await dispatcher.SendAsync<LogoutUserCommand, OperationResult>(command, cancellationToken);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Activate(
        [FromBody] ActivateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ActivateUserCommand(request.Email, request.ActivationCode);
        var result = await dispatcher.SendAsync<ActivateUserCommand, OperationResult>(command, cancellationToken);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("resend-activation-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ResendActivationCode(
        [FromBody] ResendActivationCodeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResendActivationCodeCommand(request.Email);
        var result = await dispatcher.SendAsync<ResendActivationCodeCommand, OperationResult>(command, cancellationToken);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await dispatcher.SendAsync<ForgotPasswordCommand, OperationResult>(command, cancellationToken);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(
            request.Email,
            request.Code,
            request.NewPassword,
            request.ConfirmNewPassword);

        var result = await dispatcher.SendAsync<ResetPasswordCommand, OperationResult>(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var command = new ChangePasswordCommand(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword);

        var result = await dispatcher.SendAsync<ChangePasswordCommand, OperationResult>(command, cancellationToken);
        return HandleResult(result);
    }
}
