
namespace DinExApi.Service;

public sealed class GetPortfolioPositionsQueryHandler(IInvestmentOperationRepository repository)
    : IQueryHandler<GetPortfolioPositionsQuery, OperationResult<IReadOnlyCollection<PortfolioPositionItem>>>
{
    public async Task<OperationResult<IReadOnlyCollection<PortfolioPositionItem>>> HandleAsync(
        GetPortfolioPositionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<IReadOnlyCollection<PortfolioPositionItem>>();

        try
        {
            var positions = await repository.GetPortfolioPositionsPagedAsync(
                query.UserId,
                new PaginationRequest(query.Page, query.PageSize),
                cancellationToken);
            var data = positions.Items
                .Select(x => new PortfolioPositionItem(x.AssetSymbol, x.Quantity, x.AveragePrice, x.Currency))
                .ToArray();

            result.SetData(data);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while getting portfolio.");
            return result;
        }
    }
}
