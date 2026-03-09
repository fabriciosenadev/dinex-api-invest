namespace DinExApi.Service;

public sealed record ImportInvestmentsSpreadsheetResult(
    int ProcessedFiles,
    int TotalRowsRead,
    int ImportedMovements,
    int ImportedStatementEntries,
    int SkippedRows,
    IReadOnlyCollection<string> Warnings);
