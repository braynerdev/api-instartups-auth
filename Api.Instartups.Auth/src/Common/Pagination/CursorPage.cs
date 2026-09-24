namespace Api.Instartups.Auth.Common.Pagination;

public static class CursorPage
{
    public static (IReadOnlyList<T> Items, bool HasMore) Split<T>(IReadOnlyList<T> fetched, int pageSize)
    {
        var hasMore = fetched.Count > pageSize;
        var items = hasMore ? fetched.Take(pageSize).ToList() : fetched;

        return (items, hasMore);
    }
}
