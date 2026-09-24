using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Api.Instartups.Auth.Services;

public class GenerateJwtService(
        IOptions<JwtOptions> options
    ) : IGenerateJwtService
{
    private readonly JwtOptions _options = options.Value;
    public async Task<string> GenerateJwtAsync(ApplicationUser user, IEnumerable<string> roles)
    {
        var claims = GenerateClaims(user, roles);
        var signingCredentials = CreateSigningCredentials(_options.PrivateKeyPath);

        return CreateToken(user, _options.Issuer, claims, signingCredentials);
    }
    
    private IEnumerable<Claim> GenerateClaims(ApplicationUser user, IEnumerable<string> roles)
    {
        var now = DateTimeOffset.UtcNow;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty)
        };
        
        claims.AddRange(
            roles.Select(role => new Claim(ClaimTypes.Role, role))
        );

        return claims;
    }

    private SigningCredentials CreateSigningCredentials(string privateKeyPath)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(privateKeyPath));

        var key = new RsaSecurityKey(rsa);

        return new SigningCredentials(
            key,
            SecurityAlgorithms.RsaSha256);

    }

    private string CreateToken(ApplicationUser user, string issuer, IEnumerable<Claim> claims, SigningCredentials signingCredentials)
    {
        var dateTimeNow = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            issuer: issuer,
            claims: claims,
            notBefore: dateTimeNow,
            expires: dateTimeNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}