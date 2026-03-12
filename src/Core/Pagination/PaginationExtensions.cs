namespace DinExApi.Core;

public static class PaginationExtensions
{
    public static PagedResult<T> ToPagedResult<T>(this IReadOnlyCollection<T> items, PaginationRequest pagination)
    {
        var page = pagination.NormalizedPage;
        var pageSize = pagination.NormalizedPageSize;
        var totalCount = items.Count;

        var pagedItems = items
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArray();

        return new PagedResult<T>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
