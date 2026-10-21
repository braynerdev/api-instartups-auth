using Api.Instartups.Auth.src.Interfaces.Query;

namespace Api.Instartups.Auth.UseCases.Permission.ListPermissionsQuery;

public sealed record ListPermissionsQuery(
    string? Cursor,
    int PageSize,
    string? Search) : IQuery;
