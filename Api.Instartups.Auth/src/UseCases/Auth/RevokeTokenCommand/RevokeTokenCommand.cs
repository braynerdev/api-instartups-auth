using Api.Instartups.Auth.src.Interfaces.Command;

namespace Api.Instartups.Auth.UseCases.Auth.RevokeTokenCommand;

public sealed record RevokeTokenCommand(
    string RefreshToken) : ICommand;
