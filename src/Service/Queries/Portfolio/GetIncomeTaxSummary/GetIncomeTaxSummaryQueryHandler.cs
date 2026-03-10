namespace DinExApi.Service;

public sealed class GetIncomeTaxSummaryQueryHandler(IInvestmentOperationRepository repository)
    : IQueryHandler<GetIncomeTaxSummaryQuery, OperationResult<IReadOnlyCollection<IncomeTaxYearSummaryItem>>>
{
    public async Task<OperationResult<IReadOnlyCollection<IncomeTaxYearSummaryItem>>> HandleAsync(
        GetIncomeTaxSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<IReadOnlyCollection<IncomeTaxYearSummaryItem>>();

        try
        {
            var operations = await repository.GetByUserIdAsync(query.UserId, cancellationToken);
            if (operations.Count == 0)
            {
                result.SetData([]);
                return result;
            }

            var orderedOperations = operations
                .OrderBy(x => x.OccurredAtUtc)
                .ThenBy(x => x.AssetSymbol, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var years = orderedOperations
                .Select(x => x.OccurredAtUtc.Year)
                .Distinct()
                .OrderByDescending(x => x)
                .ToArray();

            var yearly = new List<IncomeTaxYearSummaryItem>(years.Length);
            foreach (var year in years)
            {
                var cutoff = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                var portfolioByAsset = BuildPortfolio(orderedOperations.Where(x => x.OccurredAtUtc <= cutoff));
                var companySummaries = BuildCompanySummaries(portfolioByAsset);
                yearly.Add(new IncomeTaxYearSummaryItem(year, companySummaries));
            }

            result.SetData(yearly);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while getting income tax summary.");
            return result;
        }
    }

    private static IReadOnlyCollection<IncomeTaxCompanySummaryItem> BuildCompanySummaries(
        IReadOnlyDictionary<string, IncomeTaxAssetSummaryItem> portfolioByAsset)
    {
        return portfolioByAsset.Values
            .GroupBy(x => ExtractCompanyCode(x.AssetSymbol), StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var totalQuantity = group.Sum(x => x.Quantity);
                var totalCost = group.Sum(x => x.TotalCost);
                var averagePrice = totalQuantity == 0 ? 0 : totalCost / totalQuantity;
                var assets = group
                    .OrderBy(x => x.AssetSymbol, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var currency = assets.FirstOrDefault()?.Currency ?? "BRL";
                return new IncomeTaxCompanySummaryItem(
                    group.Key,
                    totalQuantity,
                    averagePrice,
                    totalCost,
                    currency,
                    assets);
            })
            .OrderBy(x => x.CompanyCode, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, IncomeTaxAssetSummaryItem> BuildPortfolio(IEnumerable<InvestmentOperationSnapshot> operations)
    {
        var state = new Dictionary<string, (decimal Quantity, decimal AveragePrice, string Currency)>(StringComparer.OrdinalIgnoreCase);

        foreach (var operation in operations)
        {
            if (!state.TryGetValue(operation.AssetSymbol, out var current))
            {
                current = (0, 0, operation.Currency);
            }

            if (operation.Type == OperationType.Buy)
            {
                var totalCost = (current.Quantity * current.AveragePrice) + (operation.Quantity * operation.UnitPriceAmount);
                var quantity = current.Quantity + operation.Quantity;
                var averagePrice = quantity == 0 ? 0 : totalCost / quantity;
                state[operation.AssetSymbol] = (quantity, averagePrice, operation.Currency);
                continue;
            }

            var sellQuantity = current.Quantity - operation.Quantity;
            if (sellQuantity <= 0)
            {
                state[operation.AssetSymbol] = (0, 0, operation.Currency);
                continue;
            }

            state[operation.AssetSymbol] = (sellQuantity, current.AveragePrice, operation.Currency);
        }

        var result = state
            .Where(x => x.Value.Quantity > 0)
            .Select(x =>
            {
                var totalCost = x.Value.Quantity * x.Value.AveragePrice;
                return new IncomeTaxAssetSummaryItem(
                    x.Key,
                    x.Value.Quantity,
                    x.Value.AveragePrice,
                    totalCost,
                    x.Value.Currency);
            })
            .ToDictionary(x => x.AssetSymbol, StringComparer.OrdinalIgnoreCase);

        return result;
    }

    private static string ExtractCompanyCode(string assetSymbol)
    {
        var normalized = assetSymbol.Trim().ToUpperInvariant();
        var match = Regex.Match(normalized, @"^([A-Z]{4,5})\d{1,2}$");
        return match.Success ? match.Groups[1].Value : normalized;
    }
}
