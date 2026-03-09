namespace DinExApi.Api.Controllers;

[Route("api/[controller]")]
public sealed class StatementController(IApplicationDispatcher dispatcher) : MainController
{
    [HttpDelete("all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ClearAllEntries(CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        var command = new ClearAllEntriesCommand(userId);
        var result = await dispatcher.SendAsync<ClearAllEntriesCommand, OperationResult>(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportInvestmentsSpreadsheetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ImportSpreadsheet(
        [FromForm] ImportInvestmentsSpreadsheetRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized(new ErrorResponse(["Authenticated user id was not found in the token."]));
        }

        if (request.Files.Count == 0)
        {
            return BadRequest(new ErrorResponse(["At least one .xlsx file is required."]));
        }

        var files = new List<ImportInvestmentsSpreadsheetFile>();
        foreach (var file in request.Files)
        {
            if (file.Length == 0)
            {
                continue;
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ErrorResponse([$"File {file.FileName} is not a valid .xlsx spreadsheet."]));
            }

            await using var stream = file.OpenReadStream();
            await using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            files.Add(new ImportInvestmentsSpreadsheetFile(file.FileName, memoryStream.ToArray()));
        }

        var command = new ImportInvestmentsSpreadsheetCommand(userId, files);
        var result = await dispatcher.SendAsync<ImportInvestmentsSpreadsheetCommand, OperationResult<ImportInvestmentsSpreadsheetResult>>(
            command,
            cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            return HandleResult(result);
        }

        var mapped = new OperationResult<ImportInvestmentsSpreadsheetResponse>();
        mapped.SetData(new ImportInvestmentsSpreadsheetResponse(
            ProcessedFiles: result.Data.ProcessedFiles,
            TotalRowsRead: result.Data.TotalRowsRead,
            ImportedMovements: result.Data.ImportedMovements,
            ImportedStatementEntries: result.Data.ImportedStatementEntries,
            SkippedRows: result.Data.SkippedRows,
            Warnings: result.Data.Warnings));

        return HandleResult(mapped);
    }

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
