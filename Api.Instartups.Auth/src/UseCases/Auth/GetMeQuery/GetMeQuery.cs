using Api.Instartups.Auth.src.Interfaces.Query;

namespace Api.Instartups.Auth.UseCases.Auth.GetMeQuery;

public sealed record GetMeQuery(
    string UserId) : IQuery;
