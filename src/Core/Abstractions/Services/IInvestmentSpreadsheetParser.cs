namespace DinExApi.Core;

public interface IInvestmentSpreadsheetParser
{
    Task<IReadOnlyCollection<ImportedSpreadsheetRow>> ParseAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default);
}
