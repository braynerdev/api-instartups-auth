using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Options;
using Api.Instartups.Auth.src.Interfaces.Command;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

namespace Api.Instartups.Auth.UseCases.Auth.LoginCommand;

    public class LoginCommandHandler(
                UserManager<IdentityUser> userManager,
                IGenerateJwtService jwtService,
                IGenerateRefreshTokenService refreshTokenService
            ) : ICommandHandler<LoginCommand, LoginCommandResponse>
    {
    public async Task<LoginCommandResponse> Handle(LoginCommand Command, CancellationToken ct)
    {
        IdentityUser user = await GetUserAsync(Command.UserNameOrEmail);
        await CheckPasswordAsync(user, Command.Password);
        var userRoles = await userManager.GetRolesAsync(user);
        var token = await jwtService.GenerateJwtAsync(user, userRoles);
        var refreshToken = refreshTokenService.GenerateRefreshToken();
        return new LoginCommandResponse();
    }

    private async Task<IdentityUser> GetUserAsync(string userNameOrEmail)
    {
        var user = userNameOrEmail.Contains("@")
            ? await userManager.FindByEmailAsync(userNameOrEmail)
            : await userManager.FindByNameAsync(userNameOrEmail);

        if (user is not null)
            return user;

        throw new InvalidCredentialsException();    
    }

    private async Task CheckPasswordAsync(IdentityUser user, string password)
    {
        bool checkPassword = await userManager.CheckPasswordAsync(user, password);

        if (!checkPassword)
            await userManager.AccessFailedAsync(user);
        
        throw new InvalidCredentialsException();
    }
    
}