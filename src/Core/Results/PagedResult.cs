namespace DinExApi.Core;

public sealed class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
