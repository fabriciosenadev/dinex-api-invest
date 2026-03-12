
namespace DinExApi.Service;

public sealed record GetPortfolioPositionsQuery(
    Guid UserId,
    int Page = PaginationRequest.DefaultPage,
    int PageSize = PaginationRequest.DefaultPageSize) : IQuery<OperationResult<IReadOnlyCollection<PortfolioPositionItem>>>;
