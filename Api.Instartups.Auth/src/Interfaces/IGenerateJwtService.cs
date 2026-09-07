using Api.Instartups.Auth.Models;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.Interfaces;

public interface IGenerateJwtService
{
    public Task<string> GenerateJwtAsync(ApplicationUser user, IEnumerable<string> roles);
}