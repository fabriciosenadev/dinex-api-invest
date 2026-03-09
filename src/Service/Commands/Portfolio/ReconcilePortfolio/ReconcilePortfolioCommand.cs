namespace DinExApi.Service;

public sealed record ReconcilePortfolioSpreadsheetFile(
    string FileName,
    byte[] Content);

public sealed record ReconcilePortfolioCommand(
    Guid UserId,
    ReconcilePortfolioSpreadsheetFile File)
    : ICommand<OperationResult<ReconcilePortfolioResult>>;
