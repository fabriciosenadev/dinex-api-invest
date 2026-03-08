namespace DinExApi.Api.Controllers;

[Route("api/[controller]")]
public sealed class StatementController(IApplicationDispatcher dispatcher) : MainController
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RegisterEntry(
        [FromBody] RegisterStatementEntryRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var command = new RegisterStatementEntryCommand(
            UserId: userId,
            Type: request.Type,
            Description: request.Description,
            GrossAmount: request.GrossAmount,
            NetAmount: request.NetAmount,
            Currency: request.Currency,
            OccurredAtUtc: request.OccurredAtUtc ?? DateTime.UtcNow,
            Source: request.Source,
            AssetSymbol: request.AssetSymbol,
            Quantity: request.Quantity,
            UnitPriceAmount: request.UnitPriceAmount,
            ReferenceId: request.ReferenceId,
            Metadata: request.Metadata);

        var result = await dispatcher.SendAsync<RegisterStatementEntryCommand, OperationResult<Guid>>(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<StatementEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetEntries(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var query = new GetStatementEntriesQuery(userId, fromUtc, toUtc);
        var result = await dispatcher.QueryAsync<GetStatementEntriesQuery, OperationResult<IReadOnlyCollection<StatementEntryItem>>>(
            query,
            cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<IReadOnlyCollection<StatementEntryResponse>>();
        mapped.SetData(result.Data
            .Select(x => new StatementEntryResponse(
                x.Id,
                x.Type.ToString(),
                x.Description,
                x.AssetSymbol,
                x.Quantity,
                x.UnitPriceAmount,
                x.GrossAmount,
                x.NetAmount,
                x.Currency,
                x.OccurredAtUtc,
                x.Source,
                x.ReferenceId,
                x.Metadata))
            .ToArray());

        return HandleResult(mapped);
    }
}
