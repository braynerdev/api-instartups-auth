using Api.Instartups.Auth.Common.Pagination;
using Api.Instartups.Auth.Constants;
using Api.Instartups.Auth.src.Interfaces;
using Api.Instartups.Auth.src.Interfaces.Query;

namespace Api.Instartups.Auth.UseCases.Permission.ListPermissionsQuery;

public class ListPermissionsQueryHandler(
        ICursorEncoder cursorEncoder
    ) : IQueryHandler<ListPermissionsQuery, ListPermissionsQueryResponse>
{
    public Task<ListPermissionsQueryResponse> Handle(ListPermissionsQuery query, CancellationToken ct)
    {
        var lastId = string.IsNullOrEmpty(query.Cursor)
            ? null
            : cursorEncoder.Decode(query.Cursor).LastId;

        var search = query.Search?.Trim();

        IEnumerable<PermissionConst.Permission> permissions = PermissionConst.All;

        if (!string.IsNullOrEmpty(search))
            permissions = permissions.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        permissions = permissions.OrderBy(p => p.Name, StringComparer.Ordinal);

        if (lastId is not null)
            permissions = permissions.Where(p => string.CompareOrdinal(p.Name, lastId) > 0);

        var fetched = permissions
            .Take(query.PageSize + 1)
            .Select(p => new PermissionListItem(p.Name))
            .ToList();

        var (items, hasMore) = CursorPage.Split(fetched, query.PageSize);
        var nextCursor = hasMore ? cursorEncoder.Encode(new CursorPayload(items[^1].Name)) : null;

        return Task.FromResult(new ListPermissionsQueryResponse(items, nextCursor, hasMore));
    }
}
