using Api.Instartups.Auth.src.Interfaces.Query;

namespace Api.Instartups.Auth.UseCases.User.GetMeQuery;

public sealed record GetMeQuery(
    string UserId) : IQuery;
