namespace Service.Application.Common;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    public int TotalPages => TotalItems == 0
        ? 0
        : (int)Math.Ceiling((double)TotalItems / PageSize);

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
