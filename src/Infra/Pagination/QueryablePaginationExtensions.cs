namespace DinExApi.Infra;

internal static class QueryablePaginationExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var page = pagination.NormalizedPage;
        var pageSize = pagination.NormalizedPageSize;

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
