namespace DinExApi.Infra;

internal sealed class B3InvestmentSpreadsheetParser : IInvestmentSpreadsheetParser
{
    private static readonly string[] DateHeaderHints = ["data", "pregao", "movimentacao"];
    private static readonly string[] DescriptionHeaderHints = ["movimentacao", "historico", "descricao", "evento", "tipo"];
    private static readonly string[] DetailHeaderHints = ["tipo de movimentacao", "movimentacao"];
    private static readonly string[] DirectionHeaderHints = ["entrada/saida", "entrada", "saida", "credito/debito", "credito", "debito"];
    private static readonly string[] AssetHeaderHints = ["produto", "ativo", "codigo", "ticker", "papel"];
    private static readonly string[] QuantityHeaderHints = ["quantidade", "qtd"];
    private static readonly string[] UnitPriceHeaderHints = ["preco unitario", "preco", "cotacao", "valor unitario"];
    private static readonly string[] GrossHeaderHints = ["valor da operacao", "valor operacao", "valor financeiro", "valor total", "valor bruto", "valor"];
    private static readonly string[] NetHeaderHints = ["valor liquido", "liquido"];
    private static readonly string[] CurrencyHeaderHints = ["moeda", "currency"];

    public Task<IReadOnlyCollection<ImportedSpreadsheetRow>> ParseAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidDataException($"File {fileName}: no worksheet found.");

        var usedRange = worksheet.RangeUsed();
        if (usedRange is null)
        {
            return Task.FromResult<IReadOnlyCollection<ImportedSpreadsheetRow>>([]);
        }

        var headerRow = ResolveHeaderRow(worksheet, usedRange);
        var headers = ReadHeaders(worksheet, headerRow, usedRange.RangeAddress.LastAddress.ColumnNumber);
        var columnMap = BuildColumnMap(headers, fileName);

        var rows = new List<ImportedSpreadsheetRow>();
        for (var rowNumber = headerRow + 1; rowNumber <= usedRange.RangeAddress.LastAddress.RowNumber; rowNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = worksheet.Row(rowNumber);
            var date = ParseDate(row.Cell(columnMap.DateColumn).Value);
            if (!date.HasValue)
            {
                continue;
            }

            var description = ReadCellAsString(row, columnMap.DescriptionColumn);
            if (string.IsNullOrWhiteSpace(description))
            {
                description = "Movimentacao importada";
            }

            var assetSymbol = ReadCellAsString(row, columnMap.AssetColumn);
            if (!string.IsNullOrWhiteSpace(assetSymbol))
            {
                assetSymbol = NormalizeAsset(assetSymbol);
            }

            var quantity = ParseDecimal(row, columnMap.QuantityColumn);
            var unitPriceAmount = ParseDecimal(row, columnMap.UnitPriceColumn);
            var grossAmount = ParseDecimal(row, columnMap.GrossAmountColumn);
            var netAmount = ParseDecimal(row, columnMap.NetAmountColumn);

            var currency = ReadCellAsString(row, columnMap.CurrencyColumn);
            if (string.IsNullOrWhiteSpace(currency))
            {
                currency = "BRL";
            }

            rows.Add(new ImportedSpreadsheetRow(
                RowNumber: rowNumber,
                OccurredAtUtc: DateTime.SpecifyKind(date.Value, DateTimeKind.Utc),
                EventDescription: description.Trim(),
                MovementDetail: ReadCellAsString(row, columnMap.DetailColumn)?.Trim(),
                Direction: ReadCellAsString(row, columnMap.DirectionColumn)?.Trim(),
                AssetSymbol: string.IsNullOrWhiteSpace(assetSymbol) ? null : assetSymbol,
                Quantity: quantity,
                UnitPriceAmount: unitPriceAmount,
                GrossAmount: grossAmount,
                NetAmount: netAmount,
                Currency: currency.Trim().ToUpperInvariant(),
                FileName: fileName));
        }

        return Task.FromResult<IReadOnlyCollection<ImportedSpreadsheetRow>>(rows);
    }

    private static int ResolveHeaderRow(IXLWorksheet worksheet, IXLRange usedRange)
    {
        var maxRow = Math.Min(usedRange.RangeAddress.LastAddress.RowNumber, 25);
        var maxColumn = Math.Min(usedRange.RangeAddress.LastAddress.ColumnNumber, 20);
        var bestRow = usedRange.RangeAddress.FirstAddress.RowNumber;
        var bestScore = int.MinValue;

        for (var rowNumber = usedRange.RangeAddress.FirstAddress.RowNumber; rowNumber <= maxRow; rowNumber++)
        {
            var score = 0;
            for (var columnNumber = 1; columnNumber <= maxColumn; columnNumber++)
            {
                var value = Normalize(worksheet.Cell(rowNumber, columnNumber).GetString());
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (ContainsHint(value, DateHeaderHints)) score += 1;
                if (ContainsHint(value, DescriptionHeaderHints)) score += 1;
                if (ContainsHint(value, AssetHeaderHints)) score += 1;
                if (ContainsHint(value, QuantityHeaderHints)) score += 1;
                if (ContainsHint(value, UnitPriceHeaderHints)) score += 1;
                if (ContainsHint(value, GrossHeaderHints)) score += 1;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestRow = rowNumber;
            }
        }

        return bestRow;
    }

    private static IReadOnlyDictionary<int, string> ReadHeaders(IXLWorksheet worksheet, int headerRow, int lastColumn)
    {
        var result = new Dictionary<int, string>();
        for (var column = 1; column <= lastColumn; column++)
        {
            var value = worksheet.Cell(headerRow, column).GetString();
            result[column] = Normalize(value);
        }

        return result;
    }

    private static ColumnMap BuildColumnMap(IReadOnlyDictionary<int, string> headers, string fileName)
    {
        var dateColumn = FindFirstColumn(headers, DateHeaderHints);
        var descriptionColumn = FindFirstColumn(headers, DescriptionHeaderHints);

        if (!dateColumn.HasValue || !descriptionColumn.HasValue)
        {
            throw new InvalidDataException(
                $"File {fileName}: required headers not found (date/movement description).");
        }

        return new ColumnMap(
            DateColumn: dateColumn.Value,
            DescriptionColumn: descriptionColumn.Value,
            DetailColumn: FindFirstColumn(headers, DetailHeaderHints),
            DirectionColumn: FindFirstColumn(headers, DirectionHeaderHints),
            AssetColumn: FindFirstColumn(headers, AssetHeaderHints),
            QuantityColumn: FindFirstColumn(headers, QuantityHeaderHints),
            UnitPriceColumn: FindFirstColumn(headers, UnitPriceHeaderHints),
            GrossAmountColumn: FindFirstColumn(headers, GrossHeaderHints),
            NetAmountColumn: FindFirstColumn(headers, NetHeaderHints),
            CurrencyColumn: FindFirstColumn(headers, CurrencyHeaderHints));
    }

    private static int? FindFirstColumn(IReadOnlyDictionary<int, string> headers, IReadOnlyCollection<string> hints)
    {
        foreach (var pair in headers)
        {
            if (ContainsHint(pair.Value, hints))
            {
                return pair.Key;
            }
        }

        return null;
    }

    private static bool ContainsHint(string value, IEnumerable<string> hints)
    {
        foreach (var hint in hints)
        {
            if (value.Contains(hint, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string? ReadCellAsString(IXLRow row, int? column)
    {
        if (!column.HasValue)
        {
            return null;
        }

        var cell = row.Cell(column.Value);
        return cell.GetString();
    }

    private static DateTime? ParseDate(XLCellValue value)
    {
        if (value.Type == XLDataType.DateTime)
        {
            return value.GetDateTime().Date;
        }

        var text = value.ToString().Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (DateTime.TryParse(text, new CultureInfo("pt-BR"), DateTimeStyles.AssumeLocal, out var date))
        {
            return date.Date;
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date))
        {
            return date.Date;
        }

        return null;
    }

    private static decimal? ParseDecimal(IXLRow row, int? column)
    {
        if (!column.HasValue)
        {
            return null;
        }

        var cell = row.Cell(column.Value);
        if (cell.DataType == XLDataType.Number)
        {
            return Convert.ToDecimal(cell.GetDouble());
        }

        var raw = cell.GetString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var sanitized = raw
            .Trim()
            .Replace("R$", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .Replace(",", ".", StringComparison.Ordinal);

        if (decimal.TryParse(sanitized, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return null;
    }

    private static string NormalizeAsset(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.StartsWith("TESOURO ", StringComparison.Ordinal))
        {
            return normalized.Replace(" ", string.Empty, StringComparison.Ordinal);
        }

        var parts = normalized
            .Split(" - ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length >= 2 && (parts[0] is "LCA" or "LCI" or "CDB" or "RDB" or "CRI" or "CRA"))
        {
            return $"{parts[0]}-{SanitizeSegment(parts[1])}";
        }

        var leadingToken = parts.Length > 0
            ? SanitizeSegment(parts[0])
            : SanitizeSegment(normalized);
        if (IsLikelyVariableIncomeTicker(leadingToken))
        {
            return leadingToken;
        }

        var tickerLike = Regex.Match(normalized, @"\b[A-Z0-9]{4,5}\d{1,2}\b");
        if (tickerLike.Success)
        {
            return tickerLike.Value;
        }

        return SanitizeSegment(normalized);
    }

    private static bool IsLikelyVariableIncomeTicker(string value)
        => Regex.IsMatch(value, @"^[A-Z0-9]{4,5}\d{1,2}$");

    private static string SanitizeSegment(string value)
    {
        var sanitized = value
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("/", string.Empty, StringComparison.Ordinal)
            .Replace(".", string.Empty, StringComparison.Ordinal)
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace("(", string.Empty, StringComparison.Ordinal)
            .Replace(")", string.Empty, StringComparison.Ordinal);

        return sanitized;
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
            .Replace("ç", "c", StringComparison.Ordinal)
            .Replace("_", " ", StringComparison.Ordinal);
    }

    private sealed record ColumnMap(
        int DateColumn,
        int DescriptionColumn,
        int? DetailColumn,
        int? DirectionColumn,
        int? AssetColumn,
        int? QuantityColumn,
        int? UnitPriceColumn,
        int? GrossAmountColumn,
        int? NetAmountColumn,
        int? CurrencyColumn);
}
