namespace DinExApi.Service;

public sealed class ImportInvestmentsSpreadsheetCommandHandler(
    IInvestmentSpreadsheetParser parser,
    IInvestmentMovementClassifier movementClassifier,
    IInvestmentOperationRepository investmentOperationRepository,
    ILedgerEntryRepository ledgerEntryRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ImportInvestmentsSpreadsheetCommand, OperationResult<ImportInvestmentsSpreadsheetResult>>
{
    public async Task<OperationResult<ImportInvestmentsSpreadsheetResult>> HandleAsync(
        ImportInvestmentsSpreadsheetCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<ImportInvestmentsSpreadsheetResult>();
        var warnings = new List<string>();
        var allRows = new List<ImportedSpreadsheetRow>();

        try
        {
            if (command.Files.Count == 0)
            {
                result.AddError("At least one spreadsheet file is required.");
                return result;
            }

            foreach (var file in command.Files)
            {
                if (file.Content.Length == 0)
                {
                    warnings.Add($"File {file.FileName}: ignored because it is empty.");
                    continue;
                }

                await using var stream = new MemoryStream(file.Content);
                var parsedRows = await parser.ParseAsync(stream, file.FileName, cancellationToken);
                allRows.AddRange(parsedRows);
            }

            if (allRows.Count == 0)
            {
                result.AddError("No rows were found in the uploaded spreadsheets.");
                return result;
            }

            var importedMovements = 0;
            var importedStatementEntries = 0;
            var skippedRows = 0;

            foreach (var row in allRows
                         .OrderBy(x => x.OccurredAtUtc)
                         .ThenBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(x => x.RowNumber))
            {
                var parseOutcome = TryBuildEntries(command.UserId, row, movementClassifier);
                if (!parseOutcome.Success || parseOutcome.StatementEntry is null)
                {
                    skippedRows += 1;
                    warnings.Add(parseOutcome.ErrorMessage ?? $"File {row.FileName} row {row.RowNumber}: ignored.");
                    continue;
                }

                if (parseOutcome.Movement is not null)
                {
                    if (!parseOutcome.Movement.IsValid)
                    {
                        skippedRows += 1;
                        warnings.Add($"File {row.FileName} row {row.RowNumber}: invalid movement data.");
                        continue;
                    }

                    await investmentOperationRepository.AddAsync(parseOutcome.Movement, cancellationToken);
                    importedMovements += 1;
                }

                if (!parseOutcome.StatementEntry.IsValid)
                {
                    skippedRows += 1;
                    warnings.Add($"File {row.FileName} row {row.RowNumber}: invalid statement data.");
                    continue;
                }

                await ledgerEntryRepository.AddAsync(parseOutcome.StatementEntry, cancellationToken);
                importedStatementEntries += 1;
            }

            if (importedMovements == 0 && importedStatementEntries == 0)
            {
                result.AddError("No valid rows were imported.");
                return result;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            result.SetData(new ImportInvestmentsSpreadsheetResult(
                ProcessedFiles: command.Files.Count,
                TotalRowsRead: allRows.Count,
                ImportedMovements: importedMovements,
                ImportedStatementEntries: importedStatementEntries,
                SkippedRows: skippedRows,
                Warnings: warnings));

            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while importing spreadsheets.");
            return result;
        }
    }

    private static ImportParseOutcome TryBuildEntries(
        Guid userId,
        ImportedSpreadsheetRow row,
        IInvestmentMovementClassifier movementClassifier)
    {
        var normalizedDescription = row.EventDescription.Trim();
        var normalizedDetail = row.MovementDetail?.Trim();
        var movementClassification = movementClassifier.Classify(row);
        var positionAssetSymbol = movementClassification.NormalizedAssetSymbol;
        var referenceId = $"{row.FileName}:{row.RowNumber}";
        var currency = string.IsNullOrWhiteSpace(row.Currency) ? "BRL" : row.Currency.Trim().ToUpperInvariant();
        var rawAmount = row.GrossAmount ?? row.NetAmount ?? CalculateAmount(row.Quantity, row.UnitPriceAmount);
        var grossAmount = Math.Abs(rawAmount);
        var netAmount = Math.Abs(row.NetAmount ?? row.GrossAmount ?? rawAmount);

        if (movementClassification.CreatesMovement)
        {
            if (string.IsNullOrWhiteSpace(positionAssetSymbol))
            {
                return ImportParseOutcome.Fail($"File {row.FileName} row {row.RowNumber}: movement without asset symbol.");
            }

            if (!row.Quantity.HasValue || row.Quantity.Value <= 0)
            {
                return ImportParseOutcome.Fail($"File {row.FileName} row {row.RowNumber}: movement without quantity.");
            }

            var unitPriceAmount = row.UnitPriceAmount.HasValue ? Math.Abs(row.UnitPriceAmount.Value) : 0m;
            if (unitPriceAmount == 0m && movementClassification.OperationType == OperationType.Sell)
            {
                // Sell rows like maturity/redeem can come without unit price in B3 exports.
                unitPriceAmount = 0m;
            }

            var movement = new InvestmentOperation(
                userId: userId,
                assetSymbol: positionAssetSymbol,
                type: movementClassification.OperationType!.Value,
                quantity: Math.Abs(row.Quantity.Value),
                unitPrice: new Money(unitPriceAmount, currency),
                occurredAtUtc: row.OccurredAtUtc);

            var ledgerType = movementClassification.OperationType == OperationType.Buy ? LedgerEntryType.Buy : LedgerEntryType.Sell;
            var statement = new LedgerEntry(
                userId: userId,
                type: ledgerType,
                description: normalizedDescription,
                grossAmount: grossAmount,
                netAmount: netAmount,
                currency: currency,
                occurredAtUtc: row.OccurredAtUtc,
                source: "import-b3",
                assetSymbol: positionAssetSymbol,
                quantity: Math.Abs(row.Quantity.Value),
                unitPriceAmount: unitPriceAmount,
                referenceId: referenceId,
                metadata: row.FileName);

            return ImportParseOutcome.Ok(movement, statement);
        }

        var entryType = MapStatementType(normalizedDescription);
        var statementEntry = new LedgerEntry(
            userId: userId,
            type: entryType,
            description: normalizedDescription,
            grossAmount: grossAmount,
            netAmount: netAmount,
            currency: currency,
            occurredAtUtc: row.OccurredAtUtc,
            source: "import-b3",
            assetSymbol: positionAssetSymbol ?? row.AssetSymbol,
            quantity: row.Quantity.HasValue ? Math.Abs(row.Quantity.Value) : null,
            unitPriceAmount: row.UnitPriceAmount.HasValue ? Math.Abs(row.UnitPriceAmount.Value) : null,
            referenceId: referenceId,
            metadata: row.FileName);

        return ImportParseOutcome.Ok(null, statementEntry);
    }

    private static decimal CalculateAmount(decimal? quantity, decimal? unitPriceAmount)
    {
        if (quantity.HasValue && unitPriceAmount.HasValue)
        {
            return quantity.Value * unitPriceAmount.Value;
        }

        return 0m;
    }

    private static LedgerEntryType MapStatementType(string description)
    {
        var normalized = Normalize(description);

        if (normalized.Contains("dividendo", StringComparison.Ordinal)
            || normalized.Contains("jcp", StringComparison.Ordinal)
            || normalized.Contains("rendimento", StringComparison.Ordinal)
            || normalized.Contains("provento", StringComparison.Ordinal))
        {
            return LedgerEntryType.Income;
        }

        if (normalized.Contains("taxa", StringComparison.Ordinal)
            || normalized.Contains("corretagem", StringComparison.Ordinal)
            || normalized.Contains("emolumento", StringComparison.Ordinal)
            || normalized.Contains("custodia", StringComparison.Ordinal))
        {
            return LedgerEntryType.Fee;
        }

        if (normalized.Contains("ir", StringComparison.Ordinal)
            || normalized.Contains("imposto", StringComparison.Ordinal)
            || normalized.Contains("dedo", StringComparison.Ordinal))
        {
            return LedgerEntryType.Tax;
        }

        if (normalized.Contains("bonificacao", StringComparison.Ordinal)
            || normalized.Contains("desdobramento", StringComparison.Ordinal)
            || normalized.Contains("grupamento", StringComparison.Ordinal)
            || normalized.Contains("subscricao", StringComparison.Ordinal)
            || normalized.Contains("fracao", StringComparison.Ordinal)
            || normalized.Contains("leilao", StringComparison.Ordinal)
            || normalized.Contains("cisao", StringComparison.Ordinal)
            || normalized.Contains("incorporacao", StringComparison.Ordinal))
        {
            return LedgerEntryType.CorporateAction;
        }

        return LedgerEntryType.Adjustment;
    }

    private static string Normalize(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized
            .Replace("á", "a", StringComparison.Ordinal)
            .Replace("à", "a", StringComparison.Ordinal)
            .Replace("â", "a", StringComparison.Ordinal)
            .Replace("ã", "a", StringComparison.Ordinal)
            .Replace("é", "e", StringComparison.Ordinal)
            .Replace("ê", "e", StringComparison.Ordinal)
            .Replace("í", "i", StringComparison.Ordinal)
            .Replace("ó", "o", StringComparison.Ordinal)
            .Replace("ô", "o", StringComparison.Ordinal)
            .Replace("õ", "o", StringComparison.Ordinal)
            .Replace("ú", "u", StringComparison.Ordinal)
            .Replace("ç", "c", StringComparison.Ordinal);
    }

    private sealed record ImportParseOutcome(
        bool Success,
        InvestmentOperation? Movement,
        LedgerEntry? StatementEntry,
        string? ErrorMessage)
    {
        public static ImportParseOutcome Ok(InvestmentOperation? movement, LedgerEntry statementEntry)
            => new(true, movement, statementEntry, null);

        public static ImportParseOutcome Fail(string message)
            => new(false, null, null, message);
    }
}
