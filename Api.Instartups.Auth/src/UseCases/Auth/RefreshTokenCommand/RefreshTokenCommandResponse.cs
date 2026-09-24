namespace Api.Instartups.Auth.UseCases.Auth.RefreshTokenCommand;

public sealed record RefreshTokenCommandResponse(string AccessToken, string RefreshToken);
