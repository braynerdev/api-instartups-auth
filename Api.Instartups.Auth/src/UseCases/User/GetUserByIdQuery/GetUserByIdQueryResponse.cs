namespace Api.Instartups.Auth.UseCases.User.GetUserByIdQuery;

public sealed record GetUserByIdQueryResponse(
    string Id,
    string UserName,
    string Email,
    string? PhoneNumber,
    IReadOnlyList<string> Roles
);
