
namespace DinExApi.Api.Controllers;

[Route("api/[controller]")]
public sealed class MovementsController(IApplicationDispatcher dispatcher) : MainController
{
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RegisterMovement(
        [FromBody] RegisterMovementRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterMovementCommand(
            request.AssetSymbol,
            request.Type,
            request.Quantity,
            request.UnitPrice,
            request.Currency,
            request.OccurredAtUtc ?? DateTime.UtcNow);

        var result = await dispatcher.SendAsync<RegisterMovementCommand, OperationResult<Guid>>(command, cancellationToken);
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpGet("portfolio")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PortfolioPositionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetPortfolio(CancellationToken cancellationToken)
    {
        var result = await dispatcher.QueryAsync<GetPortfolioPositionsQuery, OperationResult<IReadOnlyCollection<PortfolioPositionItem>>>(
            new GetPortfolioPositionsQuery(),
            cancellationToken);

        return HandleResult(result);
    }
}
