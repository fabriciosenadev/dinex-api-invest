
namespace DinExApi.Service;

public sealed record GetPortfolioPositionsQuery : IQuery<OperationResult<IReadOnlyCollection<PortfolioPositionItem>>>;
