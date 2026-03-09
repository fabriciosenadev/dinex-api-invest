
namespace DinExApi.Service;

public sealed record GetPortfolioPositionsQuery(Guid UserId) : IQuery<OperationResult<IReadOnlyCollection<PortfolioPositionItem>>>;
