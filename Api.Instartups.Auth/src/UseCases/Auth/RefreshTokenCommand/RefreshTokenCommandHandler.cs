using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.Options;
using Api.Instartups.Auth.src.Interfaces.Command;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Api.Instartups.Auth.UseCases.Auth.RefreshTokenCommand;

public class RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        IGenerateJwtService jwtService,
        IGenerateRefreshTokenService refreshTokenService,
        IUserSessionRepository sessionRepository,
        IOptions<RefreshTokenOptions> refreshTokenOptions
    ) : ICommandHandler<RefreshTokenCommand, RefreshTokenCommandResponse>
{
    private readonly RefreshTokenOptions _refreshTokenOptions = refreshTokenOptions.Value;

    public async Task<RefreshTokenCommandResponse> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var session = await GetSessionAsync(command.RefreshToken, ct);
        var user = session.User;

        var newRefreshToken = refreshTokenService.GenerateRefreshToken();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_refreshTokenOptions.ExpirationDays);

        var newSession = user.RotateSession(session, refreshTokenService.Hash(newRefreshToken), expiresAt);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = await jwtService.GenerateJwtAsync(user, roles);

        await sessionRepository.AddAsync(newSession, ct);

        return new RefreshTokenCommandResponse(accessToken, newRefreshToken);
    }

    private async Task<UserSessionsModel> GetSessionAsync(string refreshToken, CancellationToken ct)
    {
        var tokenHash = refreshTokenService.Hash(refreshToken);
        var session = await sessionRepository.GetByTokenHashAsync(tokenHash, ct);

        if (session is not null)
            return session;

        throw new InvalidRefreshTokenException();
    }
}
