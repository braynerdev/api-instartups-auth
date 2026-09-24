using Api.Instartups.Auth.Common.Identity;
using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.Exceptions.Base;
using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.Interfaces.Command;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.src.UseCases.User.AdminUpdateUserCommand;

public class AdminUpdateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IUserSessionRepository sessionRepository
    ) : ICommandHandler<AdminUpdateUserCommand, AdminUpdateUserCommandResponse>
{
    public async Task<AdminUpdateUserCommandResponse> Handle(AdminUpdateUserCommand command, CancellationToken ct)
    {
        EnsureNotSelfLock(command);

        var user = await FindUserAsync(command.UserId);
        var wasLockedOut = await userManager.IsLockedOutAsync(user);

        await UpdateProfileAsync(user, command);
        var passwordChanged = await UpdatePasswordIfRequestedAsync(user, command.NewPassword);
        await UpdateLockoutAsync(user, command.IsLocked);

        if (passwordChanged || (command.IsLocked && !wasLockedOut))
            await RevokeActiveSessionsAsync(user, ct);

        return BuildResponse(user, command.IsLocked);
    }

    private static void EnsureNotSelfLock(AdminUpdateUserCommand command)
    {
        if (command.IsLocked && command.AdminUserId == command.UserId)
            throw new ForbiddenException("Não é possível bloquear seu próprio usuário.");
    }

    private async Task<ApplicationUser> FindUserAsync(string userId)
    {
        return await userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("Usuário não encontrado.");
    }

    private async Task UpdateProfileAsync(ApplicationUser user, AdminUpdateUserCommand command)
    {
        user.UserName = command.UserName;
        user.Email = command.Email;
        user.PhoneNumber = command.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        result.EnsureSucceeded();
    }

    private async Task<bool> UpdatePasswordIfRequestedAsync(ApplicationUser user, string? newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            return false;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        result.EnsureSucceeded();
        return true;
    }

    private async Task UpdateLockoutAsync(ApplicationUser user, bool isLocked)
    {
        if (isLocked && !user.LockoutEnabled)
            await userManager.SetLockoutEnabledAsync(user, true);

        var result = await userManager.SetLockoutEndDateAsync(
            user, isLocked ? DateTimeOffset.MaxValue : null);
        result.EnsureSucceeded();
    }

    private async Task RevokeActiveSessionsAsync(ApplicationUser user, CancellationToken ct)
    {
        await sessionRepository.LoadActiveSessionsAsync(user, ct);
        user.RevokeAllSessions();
        await sessionRepository.SaveChangesAsync(ct);
    }

    private static AdminUpdateUserCommandResponse BuildResponse(ApplicationUser user, bool isLocked)
    {
        return new AdminUpdateUserCommandResponse(
            user.Id, user.UserName!, user.Email!, user.PhoneNumber, isLocked);
    }
}
