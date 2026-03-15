namespace DinExApi.Service;

public sealed class GetIncomeTaxSummaryQueryHandler(IInvestmentOperationRepository repository)
    : IQueryHandler<GetIncomeTaxSummaryQuery, OperationResult<IReadOnlyCollection<IncomeTaxYearSummaryItem>>>
{
    private sealed record YearRealizedAggregation(
        IReadOnlyDictionary<string, IncomeTaxRealizedAssetSummaryItem> Assets,
        decimal TotalProfit,
        decimal TotalLoss);

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

            var realizedByYear = BuildRealizedByYear(orderedOperations);
            var yearly = new List<IncomeTaxYearSummaryItem>(years.Length);
            foreach (var year in years)
            {
                var cutoff = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                var portfolioByAsset = BuildPortfolio(orderedOperations.Where(x => x.OccurredAtUtc <= cutoff));
                var companySummaries = BuildCompanySummaries(portfolioByAsset);
                var realized = BuildYearRealizedSummary(year, realizedByYear);
                yearly.Add(new IncomeTaxYearSummaryItem(year, companySummaries, realized));
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

    private static IReadOnlyDictionary<int, YearRealizedAggregation> BuildRealizedByYear(
        IEnumerable<InvestmentOperationSnapshot> operations)
    {
        var state = new Dictionary<string, (decimal Quantity, decimal AveragePrice, string Currency)>(StringComparer.OrdinalIgnoreCase);
        var realizedByYear = new Dictionary<int, Dictionary<string, (decimal SoldQuantity, decimal GrossProceeds, decimal CostBasis, decimal RealizedResult, string Currency)>>();
        var totalsByYear = new Dictionary<int, (decimal Profit, decimal Loss)>();

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

            var executableQuantity = Math.Min(operation.Quantity, current.Quantity);
            if (executableQuantity > 0)
            {
                var year = operation.OccurredAtUtc.Year;
                var proceeds = executableQuantity * operation.UnitPriceAmount;
                var costBasis = executableQuantity * current.AveragePrice;
                var realized = proceeds - costBasis;

                if (!realizedByYear.TryGetValue(year, out var yearlyAssets))
                {
                    yearlyAssets = new Dictionary<string, (decimal SoldQuantity, decimal GrossProceeds, decimal CostBasis, decimal RealizedResult, string Currency)>(StringComparer.OrdinalIgnoreCase);
                    realizedByYear[year] = yearlyAssets;
                }

                if (!yearlyAssets.TryGetValue(operation.AssetSymbol, out var currentRealized))
                {
                    currentRealized = (0, 0, 0, 0, operation.Currency);
                }

                yearlyAssets[operation.AssetSymbol] = (
                    currentRealized.SoldQuantity + executableQuantity,
                    currentRealized.GrossProceeds + proceeds,
                    currentRealized.CostBasis + costBasis,
                    currentRealized.RealizedResult + realized,
                    operation.Currency);

                if (!totalsByYear.TryGetValue(year, out var totals))
                {
                    totals = (0, 0);
                }

                if (realized > 0)
                {
                    totals.Profit += realized;
                }
                else if (realized < 0)
                {
                    totals.Loss += Math.Abs(realized);
                }

                totalsByYear[year] = totals;
            }

            var remainingQuantity = current.Quantity - operation.Quantity;
            if (remainingQuantity <= 0)
            {
                state[operation.AssetSymbol] = (0, 0, operation.Currency);
                continue;
            }

            state[operation.AssetSymbol] = (remainingQuantity, current.AveragePrice, operation.Currency);
        }

        return realizedByYear.ToDictionary(
            x => x.Key,
            x =>
            {
                totalsByYear.TryGetValue(x.Key, out var totals);
                var assets = (IReadOnlyDictionary<string, IncomeTaxRealizedAssetSummaryItem>)x.Value
                    .Select(asset => new IncomeTaxRealizedAssetSummaryItem(
                        asset.Key,
                        asset.Value.SoldQuantity,
                        asset.Value.GrossProceeds,
                        asset.Value.CostBasis,
                        asset.Value.RealizedResult,
                        asset.Value.Currency))
                    .ToDictionary(item => item.AssetSymbol, StringComparer.OrdinalIgnoreCase);

                return new YearRealizedAggregation(assets, totals.Profit, totals.Loss);
            });
    }

    private static IncomeTaxRealizedSummaryItem BuildYearRealizedSummary(
        int year,
        IReadOnlyDictionary<int, YearRealizedAggregation> realizedByYear)
    {
        if (!realizedByYear.TryGetValue(year, out var aggregation) || aggregation.Assets.Count == 0)
        {
            return new IncomeTaxRealizedSummaryItem(0, 0, 0, []);
        }

        var assets = aggregation.Assets.Values
            .OrderBy(x => x.AssetSymbol, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var netResult = aggregation.TotalProfit - aggregation.TotalLoss;

        return new IncomeTaxRealizedSummaryItem(aggregation.TotalProfit, aggregation.TotalLoss, netResult, assets);
    }

    private static string ExtractCompanyCode(string assetSymbol)
    {
        var normalized = assetSymbol.Trim().ToUpperInvariant();
        var match = Regex.Match(normalized, @"^([A-Z]{4,5})\d{1,2}$");
        return match.Success ? match.Groups[1].Value : normalized;
    }
}
