using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.Interfaces.Command;

namespace Api.Instartups.Auth.UseCases.Auth.RevokeTokenCommand;

public class RevokeTokenCommandHandler(
        IGenerateRefreshTokenService refreshTokenService,
        IUserSessionRepository sessionRepository
    ) : IVoidCommandHandler<RevokeTokenCommand>
{
    public async Task Handle(RevokeTokenCommand command, CancellationToken ct)
    {
        var session = await GetSessionAsync(command.RefreshToken, ct);

        session.User.RevokeSession(session);

        await sessionRepository.SaveChangesAsync(ct);
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
