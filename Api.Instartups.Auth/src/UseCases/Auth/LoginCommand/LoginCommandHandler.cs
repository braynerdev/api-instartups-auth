using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.Options;
using Api.Instartups.Auth.src.Interfaces.Command;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Api.Instartups.Auth.UseCases.Auth.LoginCommand;

public class LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IGenerateJwtService jwtService,
        IGenerateRefreshTokenService refreshTokenService,
        IUserSessionRepository sessionRepository,
        IOptions<RefreshTokenOptions> refreshTokenOptions
    ) : ICommandHandler<LoginCommand, LoginCommandResponse>
{
    private readonly RefreshTokenOptions _refreshTokenOptions = refreshTokenOptions.Value;

    public async Task<LoginCommandResponse> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await GetUserAsync(command.UserNameOrEmail);
        await CheckPasswordAsync(user, command.Password);

        await sessionRepository.LoadActiveSessionsAsync(user, ct);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = await jwtService.GenerateJwtAsync(user, roles);

        var refreshToken = refreshTokenService.GenerateRefreshToken();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_refreshTokenOptions.ExpirationDays);

        var session = user.AddSession(refreshTokenService.Hash(refreshToken), expiresAt);
        await sessionRepository.AddAsync(session, ct);

        return new LoginCommandResponse(accessToken, refreshToken);
    }

    private async Task<ApplicationUser> GetUserAsync(string userNameOrEmail)
    {
        var user = userNameOrEmail.Contains('@')
            ? await userManager.FindByEmailAsync(userNameOrEmail)
            : await userManager.FindByNameAsync(userNameOrEmail);

        if (user is not null)
            return user;

        throw new InvalidCredentialsException();
    }

    private async Task CheckPasswordAsync(ApplicationUser user, string password)
    {
        if (await userManager.CheckPasswordAsync(user, password))
            return;

        await userManager.AccessFailedAsync(user);
        throw new InvalidCredentialsException();
    }
}
