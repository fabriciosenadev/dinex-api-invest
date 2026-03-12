namespace DinExApi.Core;

public sealed record PaginationRequest(int Page = 1, int PageSize = 50)
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    public int NormalizedPage => Page < 1 ? DefaultPage : Page;

    public int NormalizedPageSize
    {
        get
        {
            if (PageSize < 1)
            {
                return DefaultPageSize;
            }

            return PageSize > MaxPageSize ? MaxPageSize : PageSize;
        }
    }
}
