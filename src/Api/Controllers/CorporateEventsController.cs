namespace DinExApi.Api.Controllers;

[Route("api/[controller]")]
public sealed class CorporateEventsController(IApplicationDispatcher dispatcher) : MainController
{
    [HttpPost]
    [ProducesResponseType(typeof(RegisterCorporateEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RegisterEvent(
        [FromBody] RegisterCorporateEventRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        if (!Enum.TryParse<CorporateEventType>(request.Type, true, out var eventType))
        {
            return BadRequest(new ErrorResponse(["Corporate event type is invalid."]));
        }

        var command = new RegisterCorporateEventCommand(
            UserId: userId,
            Type: eventType,
            SourceAssetSymbol: request.SourceAssetSymbol,
            TargetAssetSymbol: request.TargetAssetSymbol,
            Factor: request.Factor,
            CashPerSourceUnit: request.CashPerSourceUnit,
            EffectiveAtUtc: request.EffectiveAtUtc,
            Notes: request.Notes);

        var result = await dispatcher.SendAsync<RegisterCorporateEventCommand, OperationResult<RegisterCorporateEventResult>>(
            command,
            cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<RegisterCorporateEventResponse>();
        mapped.SetData(new RegisterCorporateEventResponse(result.Data.EventId, result.Data.AffectedOperations));
        return HandleResult(mapped);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RegisterCorporateEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateEvent(
        [FromRoute] Guid id,
        [FromBody] RegisterCorporateEventRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        if (!Enum.TryParse<CorporateEventType>(request.Type, true, out var eventType))
        {
            return BadRequest(new ErrorResponse(["Corporate event type is invalid."]));
        }

        var command = new UpdateCorporateEventCommand(
            UserId: userId,
            EventId: id,
            Type: eventType,
            SourceAssetSymbol: request.SourceAssetSymbol,
            TargetAssetSymbol: request.TargetAssetSymbol,
            Factor: request.Factor,
            CashPerSourceUnit: request.CashPerSourceUnit,
            EffectiveAtUtc: request.EffectiveAtUtc,
            Notes: request.Notes);

        var result = await dispatcher.SendAsync<UpdateCorporateEventCommand, OperationResult<RegisterCorporateEventResult>>(
            command,
            cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<RegisterCorporateEventResponse>();
        mapped.SetData(new RegisterCorporateEventResponse(result.Data.EventId, result.Data.AffectedOperations));
        return HandleResult(mapped);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(RegisterCorporateEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteEvent([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var command = new DeleteCorporateEventCommand(userId, id);
        var result = await dispatcher.SendAsync<DeleteCorporateEventCommand, OperationResult<RegisterCorporateEventResult>>(
            command,
            cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<RegisterCorporateEventResponse>();
        mapped.SetData(new RegisterCorporateEventResponse(result.Data.EventId, result.Data.AffectedOperations));
        return HandleResult(mapped);
    }

    [HttpDelete("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ClearEvents(CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var command = new ClearCorporateEventsCommand(userId);
        var result = await dispatcher.SendAsync<ClearCorporateEventsCommand, OperationResult>(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<CorporateEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetEvents(
        [FromQuery] int page = PaginationRequest.DefaultPage,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var query = new GetCorporateEventsQuery(userId, page, pageSize);
        var result = await dispatcher.QueryAsync<GetCorporateEventsQuery, OperationResult<IReadOnlyCollection<CorporateEventItem>>>(
            query,
            cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<IReadOnlyCollection<CorporateEventResponse>>();
        mapped.SetData(result.Data
            .Select(x => new CorporateEventResponse(
                x.Id,
                x.Type.ToString(),
                x.SourceAssetSymbol,
                x.TargetAssetSymbol,
                x.Factor,
                x.CashPerSourceUnit,
                x.EffectiveAtUtc,
                x.Notes,
                x.AppliedAtUtc))
            .ToArray());
        return HandleResult(mapped);
    }
}
