namespace Api.Instartups.Auth.UseCases.Permission.ListPermissionsQuery;

public sealed record ListPermissionsQueryResponse(
    IReadOnlyList<string> Permissions
);
