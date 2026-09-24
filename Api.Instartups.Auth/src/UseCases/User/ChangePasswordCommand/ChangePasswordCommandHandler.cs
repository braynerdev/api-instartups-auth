using Api.Instartups.Auth.Common.Identity;
using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.Interfaces.Command;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.src.UseCases.User.ChangePasswordCommand;

public class ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IUserSessionRepository sessionRepository
    ) : IVoidCommandHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId)
            ?? throw new UnauthorizedException();

        var result = await userManager.ChangePasswordAsync(
            user, command.CurrentPassword, command.NewPassword);

        if (result.Errors.Any(e => e.Code == "PasswordMismatch"))
            throw new InvalidCredentialsException();

        result.EnsureSucceeded();

        await sessionRepository.LoadActiveSessionsAsync(user, ct);
        user.RevokeAllSessions();
        await sessionRepository.SaveChangesAsync(ct);
    }
}
