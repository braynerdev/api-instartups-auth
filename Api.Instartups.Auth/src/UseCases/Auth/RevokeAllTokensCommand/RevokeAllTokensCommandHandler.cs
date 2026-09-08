using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.Interfaces.Command;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.UseCases.Auth.RevokeAllTokensCommand;

public class RevokeAllTokensCommandHandler(
        UserManager<ApplicationUser> userManager,
        IUserSessionRepository sessionRepository
    ) : IVoidCommandHandler<RevokeAllTokensCommand>
{
    public async Task Handle(RevokeAllTokensCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId)
            ?? throw new UnauthorizedException();

        await sessionRepository.LoadActiveSessionsAsync(user, ct);
        user.RevokeAllSessions();

        await sessionRepository.SaveChangesAsync(ct);
    }
}
