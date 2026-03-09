namespace DinExApi.Service;

public sealed record ImportInvestmentsSpreadsheetFile(
    string FileName,
    byte[] Content);

public sealed record ImportInvestmentsSpreadsheetCommand(
    Guid UserId,
    IReadOnlyCollection<ImportInvestmentsSpreadsheetFile> Files)
    : ICommand<OperationResult<ImportInvestmentsSpreadsheetResult>>;
