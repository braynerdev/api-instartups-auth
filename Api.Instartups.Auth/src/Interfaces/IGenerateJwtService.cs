using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.Interfaces;

public interface IGenerateJwtService
{
    public Task<string> GenerateJwtAsync(IdentityUser user, IEnumerable<string> roles);
}