namespace Api.Instartups.Auth.Common.Pagination;

public sealed record CursorPayload(
    string LastId,
    string? SortBy = null,
    string? SortDirection = null,
    IReadOnlyDictionary<string, string>? Filters = null
);
