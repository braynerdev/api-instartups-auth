namespace Api.Instartups.Auth.Interfaces;

public interface IGenerateRefreshTokenService
{
    public string GenerateRefreshToken();
    public string Hash(string token);
}
