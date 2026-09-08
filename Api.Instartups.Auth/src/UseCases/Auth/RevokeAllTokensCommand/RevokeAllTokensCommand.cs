using Api.Instartups.Auth.src.Interfaces.Command;

namespace Api.Instartups.Auth.UseCases.Auth.RevokeAllTokensCommand;

public sealed record RevokeAllTokensCommand(
    string RefreshToken) : ICommand;
