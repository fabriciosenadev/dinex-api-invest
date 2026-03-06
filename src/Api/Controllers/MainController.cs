
namespace DinExApi.Api.Controllers;

[ApiController]
[Authorize]
public abstract class MainController : ControllerBase
{
    protected ActionResult HandleResult<T>(OperationResult<T> result)
    {
        if (result.HasErrors())
        {
            var errorResponse = new ErrorResponse(result.Errors.ToArray());
            if (result.IsNotFound)
            {
                return NotFound(errorResponse);
            }

            if (result.InternalServerError)
            {
                var internalError = result.Errors.FirstOrDefault() ?? "Internal server error.";
                return Problem(internalError, statusCode: StatusCodes.Status500InternalServerError);
            }

            return BadRequest(errorResponse);
        }

        return Ok(result.Data);
    }

    protected ActionResult HandleResult(OperationResult result)
    {
        if (result.HasErrors())
        {
            var errorResponse = new ErrorResponse(result.Errors.ToArray());
            if (result.IsNotFound)
            {
                return NotFound(errorResponse);
            }

            if (result.InternalServerError)
            {
                var internalError = result.Errors.FirstOrDefault() ?? "Internal server error.";
                return Problem(internalError, statusCode: StatusCodes.Status500InternalServerError);
            }

            return BadRequest(errorResponse);
        }

        return Ok();
    }

    protected static Guid GetUserId(HttpContext context)
    {
        if (context.Items["UserId"] is Guid userId)
        {
            return userId;
        }

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        if (Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            return parsedUserId;
        }

        return Guid.Empty;
    }
}
