namespace Api.Instartups.Auth.UseCases.User.GetMeQuery;

public sealed record GetMeQueryResponse(
    string Id,
    string UserName,
    string Email,
    string? PhoneNumber
);
