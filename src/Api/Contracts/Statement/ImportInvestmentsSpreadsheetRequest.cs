namespace DinExApi.Api.Contracts.Statement;

public sealed class ImportInvestmentsSpreadsheetRequest
{
    public List<IFormFile> Files { get; set; } = [];
}
