using Service.Application.Common;

namespace Service.Api.Common;

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage)
{
    public static PagedResponse<TResponse> From<TSource, TResponse>(
        PagedResult<TSource> result,
        Func<TSource, TResponse> map)
    {
        return new PagedResponse<TResponse>(
            result.Items.Select(map).ToList(),
            result.Page,
            result.PageSize,
            result.TotalItems,
            result.TotalPages,
            result.HasPreviousPage,
            result.HasNextPage);
    }
}
