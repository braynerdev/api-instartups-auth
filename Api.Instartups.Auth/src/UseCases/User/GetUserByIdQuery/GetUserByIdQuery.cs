using Api.Instartups.Auth.src.Interfaces.Query;

namespace Api.Instartups.Auth.UseCases.User.GetUserByIdQuery;

public sealed record GetUserByIdQuery(
    string UserId) : IQuery;
