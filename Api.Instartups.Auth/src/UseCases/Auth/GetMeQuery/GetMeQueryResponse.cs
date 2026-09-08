namespace Api.Instartups.Auth.UseCases.Auth.GetMeQuery;

public sealed record GetMeQueryResponse(
    string Id,
    string UserName,
    string Email,
    string? PhoneNumber
);
