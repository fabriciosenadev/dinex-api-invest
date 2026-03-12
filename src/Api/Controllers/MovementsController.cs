
namespace DinExApi.Api.Controllers;

[Route("api/[controller]")]
public sealed class MovementsController(IApplicationDispatcher dispatcher) : MainController
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RegisterMovement(
        [FromBody] RegisterMovementRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var command = new RegisterMovementCommand(
            userId,
            request.AssetSymbol,
            request.Type,
            request.Quantity,
            request.UnitPrice,
            request.Currency,
            request.OccurredAtUtc ?? DateTime.UtcNow);

        var result = await dispatcher.SendAsync<RegisterMovementCommand, OperationResult<Guid>>(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("portfolio")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PortfolioPositionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetPortfolio(
        [FromQuery] int page = PaginationRequest.DefaultPage,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var result = await dispatcher.QueryAsync<GetPortfolioPositionsQuery, OperationResult<IReadOnlyCollection<PortfolioPositionItem>>>(
            new GetPortfolioPositionsQuery(userId, page, pageSize),
            cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("portfolio/income-tax-summary")]
    [ProducesResponseType(typeof(IReadOnlyCollection<IncomeTaxYearSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetIncomeTaxSummary(CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var result = await dispatcher.QueryAsync<GetIncomeTaxSummaryQuery, OperationResult<IReadOnlyCollection<IncomeTaxYearSummaryItem>>>(
            new GetIncomeTaxSummaryQuery(userId),
            cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<IReadOnlyCollection<IncomeTaxYearSummaryResponse>>();
        mapped.SetData(result.Data
            .Select(year => new IncomeTaxYearSummaryResponse(
                year.Year,
                year.Companies
                    .Select(company => new IncomeTaxCompanySummaryResponse(
                        company.CompanyCode,
                        company.TotalQuantity,
                        company.ConsolidatedAveragePrice,
                        company.TotalCost,
                        company.Currency,
                        company.Assets
                            .Select(asset => new IncomeTaxAssetSummaryResponse(
                                asset.AssetSymbol,
                                asset.Quantity,
                                asset.AveragePrice,
                                asset.TotalCost,
                                asset.Currency))
                            .ToArray()))
                    .ToArray()))
            .ToArray());

        return HandleResult(mapped);
    }

    [HttpPost("portfolio/reconcile")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ReconcilePortfolioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ReconcilePortfolio(
        [FromForm] ReconcilePortfolioRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new ErrorResponse(["A position spreadsheet (.xlsx) file is required."]));
        }

        if (!request.File.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ErrorResponse([$"File {request.File.FileName} is not a valid .xlsx spreadsheet."]));
        }

        await using var stream = request.File.OpenReadStream();
        await using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        var command = new ReconcilePortfolioCommand(
            userId,
            new ReconcilePortfolioSpreadsheetFile(request.File.FileName, memoryStream.ToArray()));
        var result = await dispatcher.SendAsync<ReconcilePortfolioCommand, OperationResult<ReconcilePortfolioResult>>(
            command,
            cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<ReconcilePortfolioResponse>();
        mapped.SetData(new ReconcilePortfolioResponse(
            TotalAssets: result.Data.TotalAssets,
            MatchedAssets: result.Data.MatchedAssets,
            DivergentAssets: result.Data.DivergentAssets,
            Assets: result.Data.Assets
                .Select(x => new ReconcilePortfolioAssetResponse(
                    x.AssetSymbol,
                    x.ExpectedQuantity,
                    x.CurrentQuantity,
                    x.Difference,
                    x.Status,
                    x.Reason))
                .ToArray()));

        return HandleResult(mapped);
    }
}
