using Api.Instartups.Auth.src.Interfaces.Command;

namespace Api.Instartups.Auth.UseCases.Auth.RefreshTokenCommand;

public sealed record RefreshTokenCommand(
    string RefreshToken) : ICommand;
