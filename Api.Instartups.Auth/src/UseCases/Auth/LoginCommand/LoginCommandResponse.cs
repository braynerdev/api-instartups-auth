namespace Api.Instartups.Auth.UseCases.Auth.LoginCommand;

public sealed record LoginCommandResponse(string AccessToken, string RefreshToken);
