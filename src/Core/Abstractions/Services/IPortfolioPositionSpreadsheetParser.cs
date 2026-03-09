namespace DinExApi.Core;

public interface IPortfolioPositionSpreadsheetParser
{
    Task<IReadOnlyCollection<ImportedPortfolioPositionRow>> ParseAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default);
}
