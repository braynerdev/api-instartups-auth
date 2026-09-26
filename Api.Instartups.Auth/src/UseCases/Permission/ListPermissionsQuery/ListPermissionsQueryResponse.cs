namespace Api.Instartups.Auth.UseCases.Permission.ListPermissionsQuery;

public sealed record PermissionListItem(
    string Name
);

public sealed record ListPermissionsQueryResponse(
    IReadOnlyList<PermissionListItem> Items,
    string? NextCursor,
    bool HasMore
);
