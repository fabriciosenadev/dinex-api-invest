namespace DinExApi.Core;

public sealed class OperationResult<T> : OperationResultBase
{
    public T? Data { get; private set; }

    public static OperationResult<T> Ok(T data)
    {
        var result = new OperationResult<T>();
        result.SetData(data);
        return result;
    }

    public OperationResult<T> SetData(T data)
    {
        Data = data;
        return this;
    }
}
