using Api.Instartups.Auth.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Instartups.Auth.Configurations.Extension;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
           ?? throw new UnauthorizedException();
}
