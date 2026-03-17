namespace DinExApi.Service;

public sealed class GetIncomeTaxSummaryQueryHandler(
    IInvestmentOperationRepository repository,
    ILedgerEntryRepository ledgerEntryRepository)
    : IQueryHandler<GetIncomeTaxSummaryQuery, OperationResult<IReadOnlyCollection<IncomeTaxYearSummaryItem>>>
{
    private const decimal StockCommonMonthlyExemptionLimit = 20_000m;

    private sealed record YearRealizedAggregation(
        IReadOnlyDictionary<string, IncomeTaxRealizedAssetSummaryItem> Assets,
        decimal TotalProfit,
        decimal TotalLoss);

    private sealed record BucketKey(string AssetClass, string TradeMode);

    private sealed class MonthlyBucketAggregation
    {
        public decimal GrossResult { get; set; }
        public decimal GrossProceeds { get; set; }
    }

    private sealed class MonthlyBucketComputation
    {
        public required BucketKey Key { get; init; }
        public decimal GrossResult { get; init; }
        public decimal GrossProceeds { get; init; }
        public decimal LossCompensated { get; set; }
        public decimal TaxableBase { get; set; }
        public decimal TaxRate { get; init; }
        public decimal TaxDue { get; set; }
        public decimal IrrfMonth { get; set; }
        public decimal IrrfCompensated { get; set; }
        public decimal DarfGenerated { get; set; }
    }

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

            var ledgerEntries = await ledgerEntryRepository.GetByUserIdAsync(query.UserId, cancellationToken: cancellationToken);
            var orderedOperations = operations
                .OrderBy(x => x.OccurredAtUtc)
                .ThenBy(x => x.AssetSymbol, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var realizedByYear = BuildRealizedByYear(orderedOperations);
            var monthlyByYear = BuildMonthlyTaxationByYear(orderedOperations, ledgerEntries);

            var years = orderedOperations
                .Select(x => x.OccurredAtUtc.Year)
                .Union(monthlyByYear.Keys)
                .Distinct()
                .OrderByDescending(x => x)
                .ToArray();

            var yearly = new List<IncomeTaxYearSummaryItem>(years.Length);
            foreach (var year in years)
            {
                var cutoff = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                var portfolioByAsset = BuildPortfolio(orderedOperations.Where(x => x.OccurredAtUtc <= cutoff));
                var companySummaries = BuildCompanySummaries(portfolioByAsset);
                var realized = BuildYearRealizedSummary(year, realizedByYear);
                var monthlyTaxation = monthlyByYear.TryGetValue(year, out var monthly) ? monthly : [];

                yearly.Add(new IncomeTaxYearSummaryItem(year, companySummaries, realized, monthlyTaxation));
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

    private static IReadOnlyDictionary<int, IReadOnlyCollection<IncomeTaxMonthlySummaryItem>> BuildMonthlyTaxationByYear(
        IReadOnlyCollection<InvestmentOperationSnapshot> operations,
        IReadOnlyCollection<LedgerEntry> ledgerEntries)
    {
        var dayTradeRemainingByAssetDate = operations
            .GroupBy(x => (Date: DateOnly.FromDateTime(x.OccurredAtUtc), x.AssetSymbol), x => x)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var buyQuantity = g.Where(x => x.Type == OperationType.Buy).Sum(x => x.Quantity);
                    var sellQuantity = g.Where(x => x.Type == OperationType.Sell).Sum(x => x.Quantity);
                    return Math.Min(buyQuantity, sellQuantity);
                });

        var positionByAsset = new Dictionary<string, (decimal Quantity, decimal AveragePrice, string Currency)>(StringComparer.OrdinalIgnoreCase);
        var monthlyBucketAggregation = new Dictionary<(int Year, int Month), Dictionary<BucketKey, MonthlyBucketAggregation>>();

        foreach (var operation in operations)
        {
            if (!positionByAsset.TryGetValue(operation.AssetSymbol, out var current))
            {
                current = (0, 0, operation.Currency);
            }

            if (operation.Type == OperationType.Buy)
            {
                var totalCost = (current.Quantity * current.AveragePrice) + (operation.Quantity * operation.UnitPriceAmount);
                var quantity = current.Quantity + operation.Quantity;
                var averagePrice = quantity == 0 ? 0 : totalCost / quantity;
                positionByAsset[operation.AssetSymbol] = (quantity, averagePrice, operation.Currency);
                continue;
            }

            var executableQuantity = Math.Min(operation.Quantity, current.Quantity);
            if (executableQuantity > 0)
            {
                var assetClass = ClassifyAsset(operation.AssetSymbol);
                var dateKey = (DateOnly.FromDateTime(operation.OccurredAtUtc), operation.AssetSymbol);
                dayTradeRemainingByAssetDate.TryGetValue(dateKey, out var dayTradeRemaining);

                var dayTradeQuantity = Math.Min(executableQuantity, dayTradeRemaining);
                var commonQuantity = executableQuantity - dayTradeQuantity;

                if (dayTradeQuantity > 0)
                {
                    AppendMonthlySegment(
                        monthlyBucketAggregation,
                        operation,
                        assetClass,
                        "daytrade",
                        dayTradeQuantity,
                        current.AveragePrice);
                }

                if (commonQuantity > 0)
                {
                    AppendMonthlySegment(
                        monthlyBucketAggregation,
                        operation,
                        assetClass,
                        "common",
                        commonQuantity,
                        current.AveragePrice);
                }

                if (dayTradeQuantity > 0)
                {
                    dayTradeRemainingByAssetDate[dateKey] = dayTradeRemaining - dayTradeQuantity;
                }
            }

            var remainingQuantity = current.Quantity - operation.Quantity;
            if (remainingQuantity <= 0)
            {
                positionByAsset[operation.AssetSymbol] = (0, 0, operation.Currency);
                continue;
            }

            positionByAsset[operation.AssetSymbol] = (remainingQuantity, current.AveragePrice, operation.Currency);
        }

        var monthlyIrrf = ledgerEntries
            .Where(entry => entry.Type == LedgerEntryType.Tax && IsIrrfDescription(entry.Description))
            .GroupBy(entry => (entry.OccurredAtUtc.Year, entry.OccurredAtUtc.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => Math.Abs(x.NetAmount)));

        var allMonths = monthlyBucketAggregation.Keys
            .Union(monthlyIrrf.Keys)
            .Distinct()
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToArray();

        var lossCarryByBucket = new Dictionary<BucketKey, decimal>();
        var irrfCarry = 0m;
        var byYear = new Dictionary<int, List<IncomeTaxMonthlySummaryItem>>();

        foreach (var monthKey in allMonths)
        {
            monthlyBucketAggregation.TryGetValue(monthKey, out var bucketAggregationForMonth);
            bucketAggregationForMonth ??= new Dictionary<BucketKey, MonthlyBucketAggregation>();

            monthlyIrrf.TryGetValue(monthKey, out var irrfMonth);
            var bucketComputations = new List<MonthlyBucketComputation>(bucketAggregationForMonth.Count);

            foreach (var (bucketKey, aggregation) in bucketAggregationForMonth
                         .OrderBy(x => x.Key.AssetClass, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(x => x.Key.TradeMode, StringComparer.OrdinalIgnoreCase))
            {
                lossCarryByBucket.TryGetValue(bucketKey, out var carry);
                var grossResult = aggregation.GrossResult;
                var stockCommonIsExempt = bucketKey.AssetClass == "acao"
                    && bucketKey.TradeMode == "common"
                    && aggregation.GrossProceeds <= StockCommonMonthlyExemptionLimit;

                var lossCompensated = 0m;
                var taxableBase = 0m;

                if (grossResult > 0 && !stockCommonIsExempt)
                {
                    lossCompensated = Math.Min(grossResult, carry);
                    taxableBase = grossResult - lossCompensated;
                    carry -= lossCompensated;
                }
                else if (grossResult < 0)
                {
                    carry += Math.Abs(grossResult);
                }

                lossCarryByBucket[bucketKey] = carry;
                var taxRate = ResolveTaxRate(bucketKey);
                var taxDue = taxableBase * taxRate;

                bucketComputations.Add(new MonthlyBucketComputation
                {
                    Key = bucketKey,
                    GrossResult = grossResult,
                    GrossProceeds = aggregation.GrossProceeds,
                    LossCompensated = lossCompensated,
                    TaxableBase = taxableBase,
                    TaxRate = taxRate,
                    TaxDue = taxDue
                });
            }

            var totalTax = bucketComputations.Sum(x => x.TaxDue);
            var availableIrrf = irrfCarry + irrfMonth;
            var irrfCompensatedMonth = Math.Min(totalTax, availableIrrf);
            var darfDue = totalTax - irrfCompensatedMonth;
            irrfCarry = availableIrrf - irrfCompensatedMonth;

            AllocateIrrfByBucket(bucketComputations, irrfMonth, irrfCompensatedMonth, totalTax);

            var bucketResponses = bucketComputations
                .Select(x => new IncomeTaxMonthlyBucketSummaryItem(
                    x.Key.AssetClass,
                    x.Key.TradeMode,
                    x.GrossResult,
                    x.LossCompensated,
                    x.TaxableBase,
                    x.TaxRate,
                    x.TaxDue,
                    x.IrrfMonth,
                    x.IrrfCompensated,
                    x.DarfGenerated))
                .ToArray();

            var carryResponses = lossCarryByBucket
                .Where(x => x.Value > 0)
                .OrderBy(x => x.Key.AssetClass, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Key.TradeMode, StringComparer.OrdinalIgnoreCase)
                .Select(x => new IncomeTaxMonthlyBucketCarryItem(
                    x.Key.AssetClass,
                    x.Key.TradeMode,
                    x.Value))
                .ToArray();

            var monthlySummary = new IncomeTaxMonthlySummaryItem(
                monthKey.Year,
                monthKey.Month,
                totalTax,
                irrfMonth,
                irrfCompensatedMonth,
                darfDue,
                bucketResponses,
                carryResponses);

            if (!byYear.TryGetValue(monthKey.Year, out var yearList))
            {
                yearList = [];
                byYear[monthKey.Year] = yearList;
            }

            yearList.Add(monthlySummary);
        }

        return byYear.ToDictionary(
            x => x.Key,
            x => (IReadOnlyCollection<IncomeTaxMonthlySummaryItem>)x.Value
                .OrderBy(item => item.Month)
                .ToArray());
    }

    private static void AppendMonthlySegment(
        IDictionary<(int Year, int Month), Dictionary<BucketKey, MonthlyBucketAggregation>> monthlyBuckets,
        InvestmentOperationSnapshot operation,
        string assetClass,
        string tradeMode,
        decimal quantity,
        decimal averagePrice)
    {
        var monthKey = (operation.OccurredAtUtc.Year, operation.OccurredAtUtc.Month);
        if (!monthlyBuckets.TryGetValue(monthKey, out var buckets))
        {
            buckets = new Dictionary<BucketKey, MonthlyBucketAggregation>();
            monthlyBuckets[monthKey] = buckets;
        }

        var bucketKey = new BucketKey(assetClass, tradeMode);
        if (!buckets.TryGetValue(bucketKey, out var bucket))
        {
            bucket = new MonthlyBucketAggregation();
            buckets[bucketKey] = bucket;
        }

        var proceeds = quantity * operation.UnitPriceAmount;
        var costBasis = quantity * averagePrice;

        bucket.GrossResult += proceeds - costBasis;
        if (bucketKey.AssetClass == "acao" && bucketKey.TradeMode == "common")
        {
            bucket.GrossProceeds += proceeds;
        }
    }

    private static void AllocateIrrfByBucket(
        IReadOnlyList<MonthlyBucketComputation> buckets,
        decimal irrfMonth,
        decimal irrfCompensatedMonth,
        decimal totalTax)
    {
        if (buckets.Count == 0 || totalTax <= 0)
        {
            return;
        }

        var taxableBuckets = buckets.Where(x => x.TaxDue > 0).ToArray();
        if (taxableBuckets.Length == 0)
        {
            return;
        }

        var remainingIrrfMonth = irrfMonth;
        var remainingIrrfCompensated = irrfCompensatedMonth;
        var remainingTaxWeight = taxableBuckets.Sum(x => x.TaxDue);

        for (var i = 0; i < taxableBuckets.Length; i++)
        {
            var current = taxableBuckets[i];
            var isLast = i == taxableBuckets.Length - 1;
            decimal irrfMonthSlice;
            decimal irrfCompensatedSlice;

            if (isLast || remainingTaxWeight <= 0)
            {
                irrfMonthSlice = remainingIrrfMonth;
                irrfCompensatedSlice = remainingIrrfCompensated;
            }
            else
            {
                var weight = current.TaxDue / remainingTaxWeight;
                irrfMonthSlice = Math.Round(remainingIrrfMonth * weight, 2, MidpointRounding.AwayFromZero);
                irrfCompensatedSlice = Math.Round(remainingIrrfCompensated * weight, 2, MidpointRounding.AwayFromZero);
            }

            irrfMonthSlice = Math.Min(irrfMonthSlice, remainingIrrfMonth);
            irrfCompensatedSlice = Math.Min(irrfCompensatedSlice, remainingIrrfCompensated);

            current.IrrfMonth = irrfMonthSlice;
            current.IrrfCompensated = irrfCompensatedSlice;
            current.DarfGenerated = Math.Max(0, current.TaxDue - irrfCompensatedSlice);

            remainingIrrfMonth -= irrfMonthSlice;
            remainingIrrfCompensated -= irrfCompensatedSlice;
            remainingTaxWeight -= current.TaxDue;
        }
    }

    private static decimal ResolveTaxRate(BucketKey bucket)
    {
        if (bucket.AssetClass == "fii")
        {
            return 0.20m;
        }

        if (bucket.TradeMode == "daytrade")
        {
            return 0.20m;
        }

        return 0.15m;
    }

    private static string ClassifyAsset(string assetSymbol)
    {
        var normalized = assetSymbol.Trim().ToUpperInvariant();

        if (normalized == "TAEE11")
        {
            return "acao";
        }

        if (IsKnownEtf(normalized)
            || Regex.IsMatch(normalized, "^[A-Z]{4}39$")
            || Regex.IsMatch(normalized, "^[A-Z]{4}11B$"))
        {
            return "etf";
        }

        if (Regex.IsMatch(normalized, "^[A-Z]{4,5}11$"))
        {
            return "fii";
        }

        if (Regex.IsMatch(normalized, "^[A-Z]{4,5}[3456]$") || Regex.IsMatch(normalized, "^[A-Z]{4,5}\\d{1,2}$"))
        {
            return "acao";
        }

        return "outro";
    }

    private static bool IsKnownEtf(string assetSymbol)
    {
        return assetSymbol is
            "BOVA11" or
            "SMAL11" or
            "IVVB11" or
            "GOLD11" or
            "HASH11" or
            "BOVV11" or
            "XBOV11" or
            "DIVO11" or
            "ECOO11" or
            "PIBB11";
    }

    private static bool IsIrrfDescription(string description)
    {
        var normalized = Normalize(description);
        return normalized.Contains("irrf", StringComparison.Ordinal)
            || normalized.Contains("imposto de renda", StringComparison.Ordinal)
            || normalized.Contains("dedo duro", StringComparison.Ordinal)
            || normalized.Contains("dedo-duro", StringComparison.Ordinal);
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

