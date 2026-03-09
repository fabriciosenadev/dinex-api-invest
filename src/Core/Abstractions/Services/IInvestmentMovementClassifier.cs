namespace DinExApi.Core;

public interface IInvestmentMovementClassifier
{
    MovementClassificationResult Classify(ImportedSpreadsheetRow row);
}

public sealed record MovementClassificationResult(
    OperationType? OperationType,
    string? NormalizedAssetSymbol)
{
    public bool CreatesMovement => OperationType.HasValue;
}
