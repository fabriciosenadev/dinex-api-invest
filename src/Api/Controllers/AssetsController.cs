namespace DinExApi.Api.Controllers;

[Route("api/assets")]
public sealed class AssetsController(IApplicationDispatcher dispatcher) : MainController
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<AssetDefinitionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetAssets(
        [FromQuery] int page = PaginationRequest.DefaultPage,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var result = await dispatcher.QueryAsync<GetAssetDefinitionsQuery, OperationResult<IReadOnlyCollection<AssetDefinitionItem>>>(
            new GetAssetDefinitionsQuery(userId, page, pageSize),
            cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<IReadOnlyCollection<AssetDefinitionResponse>>();
        mapped.SetData(result.Data
            .Select(x => new AssetDefinitionResponse(
                x.Id,
                x.Symbol,
                x.Type.ToString(),
                x.Name,
                x.Document,
                x.Country,
                x.Currency,
                x.Sector,
                x.Segment,
                x.ShareClass,
                x.CvmCode,
                x.FiiCategory,
                x.Administrator,
                x.Manager,
                x.Notes,
                x.CreatedAt,
                x.UpdatedAt))
            .ToArray());

        return HandleResult(mapped);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpsertAsset(
        [FromBody] UpsertAssetDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        if (!Enum.TryParse<AssetType>(request.Type, true, out var type))
        {
            return BadRequest(new ErrorResponse(["Asset type is invalid."]));
        }

        var command = new UpsertAssetDefinitionCommand(
            userId,
            request.Symbol,
            type,
            request.Name,
            request.Document,
            request.Country,
            request.Currency,
            request.Sector,
            request.Segment,
            request.ShareClass,
            request.CvmCode,
            request.FiiCategory,
            request.Administrator,
            request.Manager,
            request.Notes);
        var result = await dispatcher.SendAsync<UpsertAssetDefinitionCommand, OperationResult<Guid>>(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateAsset(
        [FromRoute] Guid id,
        [FromBody] UpsertAssetDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        if (!Enum.TryParse<AssetType>(request.Type, true, out var type))
        {
            return BadRequest(new ErrorResponse(["Asset type is invalid."]));
        }

        var command = new UpdateAssetDefinitionCommand(
            userId,
            id,
            request.Symbol,
            type,
            request.Name,
            request.Document,
            request.Country,
            request.Currency,
            request.Sector,
            request.Segment,
            request.ShareClass,
            request.CvmCode,
            request.FiiCategory,
            request.Administrator,
            request.Manager,
            request.Notes);
        var result = await dispatcher.SendAsync<UpdateAssetDefinitionCommand, OperationResult<Guid>>(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteAsset([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var command = new DeleteAssetDefinitionCommand(userId, id);
        var result = await dispatcher.SendAsync<DeleteAssetDefinitionCommand, OperationResult>(command, cancellationToken);
        return HandleResult(result);
    }
}
