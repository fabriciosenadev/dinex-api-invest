namespace DinExApi.Core;

public sealed class OperationResult : OperationResultBase
{
    public static OperationResult Ok() => new();
}
