using Api.Instartups.Auth.src.Interfaces.Query;

namespace Api.Instartups.Auth.UseCases.User.ListUsersQuery;

public sealed record ListUsersQuery(
    string? Cursor,
    int PageSize) : IQuery;
