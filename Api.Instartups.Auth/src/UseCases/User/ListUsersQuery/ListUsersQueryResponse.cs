namespace Api.Instartups.Auth.UseCases.User.ListUsersQuery;

public sealed record UserListItem(
    string Id,
    string UserName,
    string Email,
    string? PhoneNumber
);

public sealed record ListUsersQueryResponse(
    IReadOnlyList<UserListItem> Items,
    string? NextCursor,
    bool HasMore
);
