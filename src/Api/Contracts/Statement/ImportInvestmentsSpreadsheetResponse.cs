namespace DinExApi.Api.Contracts.Statement;

public sealed record ImportInvestmentsSpreadsheetResponse(
    int ProcessedFiles,
    int TotalRowsRead,
    int ImportedMovements,
    int ImportedStatementEntries,
    int SkippedRows,
    IReadOnlyCollection<string> Warnings);
