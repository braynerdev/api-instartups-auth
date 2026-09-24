using Api.Instartups.Auth.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Api.Instartups.Auth.Services;

public class GenerateRefreshTokenService : IGenerateRefreshTokenService
{
    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexStringLower(bytes);
    }

    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }
}