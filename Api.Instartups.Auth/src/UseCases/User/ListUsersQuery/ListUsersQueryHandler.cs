using Api.Instartups.Auth.Common.Pagination;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.Interfaces;
using Api.Instartups.Auth.src.Interfaces.Query;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Instartups.Auth.UseCases.User.ListUsersQuery;

public class ListUsersQueryHandler(
        UserManager<ApplicationUser> userManager,
        ICursorEncoder cursorEncoder
    ) : IQueryHandler<ListUsersQuery, ListUsersQueryResponse>
{
    public async Task<ListUsersQueryResponse> Handle(ListUsersQuery query, CancellationToken ct)
    {
        var lastId = string.IsNullOrEmpty(query.Cursor)
            ? null
            : cursorEncoder.Decode(query.Cursor).LastId;

        var users = userManager.Users.OrderBy(u => u.Id);

        var filtered = lastId is null
            ? users
            : users.Where(u => string.Compare(u.Id, lastId) > 0);

        var fetched = await filtered
            .Take(query.PageSize + 1)
            .Select(u => new UserListItem(u.Id, u.UserName!, u.Email!, u.PhoneNumber))
            .ToListAsync(ct);

        var (items, hasMore) = CursorPage.Split(fetched, query.PageSize);
        var nextCursor = hasMore ? cursorEncoder.Encode(new CursorPayload(items[^1].Id)) : null;

        return new ListUsersQueryResponse(items, nextCursor, hasMore);
    }
}
