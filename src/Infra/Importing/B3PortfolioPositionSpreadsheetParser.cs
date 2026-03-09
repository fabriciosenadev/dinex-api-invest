namespace DinExApi.Infra;

internal sealed class B3PortfolioPositionSpreadsheetParser : IPortfolioPositionSpreadsheetParser
{
    public Task<IReadOnlyCollection<ImportedPortfolioPositionRow>> ParseAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook(stream);
        if (workbook.Worksheets.Count == 0)
        {
            throw new InvalidDataException($"File {fileName}: no worksheet found.");
        }

        var rows = new List<ImportedPortfolioPositionRow>();
        foreach (var worksheet in workbook.Worksheets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var worksheetRows = ParseWorksheet(worksheet, cancellationToken);
            if (worksheetRows.Count > 0)
            {
                rows.AddRange(worksheetRows);
            }
        }

        if (rows.Count == 0)
        {
            return Task.FromResult<IReadOnlyCollection<ImportedPortfolioPositionRow>>([]);
        }

        var consolidated = rows
            .GroupBy(x => x.AssetSymbol, StringComparer.OrdinalIgnoreCase)
            .Select(x => new ImportedPortfolioPositionRow(x.Key, x.Sum(y => y.Quantity), "BRL"))
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<ImportedPortfolioPositionRow>>(consolidated);
    }

    private static IReadOnlyCollection<ImportedPortfolioPositionRow> ParseWorksheet(
        IXLWorksheet worksheet,
        CancellationToken cancellationToken)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange is null)
        {
            return [];
        }

        var headerRow = FindHeaderRow(worksheet, usedRange);
        var headerMap = ReadHeaderMap(worksheet, headerRow, usedRange.RangeAddress.LastAddress.ColumnNumber);
        var assetColumn = FindColumn(headerMap, ["codigo de negociacao", "codigo", "ticker"]);
        var productColumn = FindColumn(headerMap, ["produto", "ativo", "descricao"]);
        var quantityColumn = FindColumn(headerMap, ["quantidade"]);

        if (!quantityColumn.HasValue || (!assetColumn.HasValue && !productColumn.HasValue))
        {
            return [];
        }

        var rows = new List<ImportedPortfolioPositionRow>();
        for (var rowNumber = headerRow + 1; rowNumber <= usedRange.RangeAddress.LastAddress.RowNumber; rowNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = worksheet.Row(rowNumber);
            var quantity = ParseDecimal(row.Cell(quantityColumn!.Value));
            if (!quantity.HasValue || quantity.Value <= 0)
            {
                continue;
            }

            var codeAsset = assetColumn.HasValue
                ? row.Cell(assetColumn.Value).GetString()
                : string.Empty;
            var productAsset = productColumn.HasValue
                ? row.Cell(productColumn.Value).GetString()
                : string.Empty;

            var asset = codeAsset;

            if (string.IsNullOrWhiteSpace(asset))
            {
                asset = productAsset;
            }

            // Tesouro rows usually expose ISIN (BRSTN...) in code and the investor-facing title in product.
            if (asset.StartsWith("BRSTN", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(productAsset)
                && productAsset.Contains("TESOURO", StringComparison.OrdinalIgnoreCase))
            {
                asset = productAsset;
            }

            var normalizedAsset = NormalizeAsset(asset);
            if (string.IsNullOrWhiteSpace(normalizedAsset))
            {
                continue;
            }

            rows.Add(new ImportedPortfolioPositionRow(normalizedAsset, quantity.Value, "BRL"));
        }

        return rows;
    }

    private static int FindHeaderRow(IXLWorksheet worksheet, IXLRange usedRange)
    {
        var maxRow = Math.Min(usedRange.RangeAddress.LastAddress.RowNumber, 20);
        var maxColumn = Math.Min(usedRange.RangeAddress.LastAddress.ColumnNumber, 20);
        var bestRow = usedRange.RangeAddress.FirstAddress.RowNumber;
        var bestScore = int.MinValue;

        for (var rowNumber = usedRange.RangeAddress.FirstAddress.RowNumber; rowNumber <= maxRow; rowNumber++)
        {
            var score = 0;
            for (var columnNumber = 1; columnNumber <= maxColumn; columnNumber++)
            {
                var value = Normalize(worksheet.Cell(rowNumber, columnNumber).GetString());
                if (value.Contains("quantidade", StringComparison.Ordinal)) score += 1;
                if (value.Contains("codigo", StringComparison.Ordinal)) score += 1;
                if (value.Contains("produto", StringComparison.Ordinal)) score += 1;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestRow = rowNumber;
            }
        }

        return bestRow;
    }

    private static IReadOnlyDictionary<int, string> ReadHeaderMap(IXLWorksheet worksheet, int headerRow, int lastColumn)
    {
        var map = new Dictionary<int, string>();
        for (var column = 1; column <= lastColumn; column++)
        {
            map[column] = Normalize(worksheet.Cell(headerRow, column).GetString());
        }

        return map;
    }

    private static int? FindColumn(IReadOnlyDictionary<int, string> headers, IReadOnlyCollection<string> hints)
    {
        foreach (var pair in headers)
        {
            foreach (var hint in hints)
            {
                if (pair.Value.Contains(hint, StringComparison.Ordinal))
                {
                    return pair.Key;
                }
            }
        }

        return null;
    }

    private static decimal? ParseDecimal(IXLCell cell)
    {
        if (cell.DataType == XLDataType.Number)
        {
            return Convert.ToDecimal(cell.GetDouble());
        }

        var text = cell.GetString().Trim();
        if (string.IsNullOrWhiteSpace(text) || text == "-")
        {
            return null;
        }

        var sanitized = text.Replace(".", string.Empty, StringComparison.Ordinal)
            .Replace(",", ".", StringComparison.Ordinal);

        return decimal.TryParse(sanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static string NormalizeAsset(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim().ToUpperInvariant();
        if (trimmed.Contains(" - ", StringComparison.Ordinal))
        {
            trimmed = trimmed.Split(" - ", 2, StringSplitOptions.TrimEntries)[0];
        }

        return Regex.Replace(trimmed, @"\s+", string.Empty);
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
}
