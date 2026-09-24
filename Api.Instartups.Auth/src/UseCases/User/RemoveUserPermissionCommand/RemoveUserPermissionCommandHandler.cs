using Api.Instartups.Auth.Common.Identity;
using Api.Instartups.Auth.Exceptions.Base;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.Interfaces.Command;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.src.UseCases.User.RemoveUserPermissionCommand;

public class RemoveUserPermissionCommandHandler(
        UserManager<ApplicationUser> userManager
    ) : IVoidCommandHandler<RemoveUserPermissionCommand>
{
    public async Task Handle(RemoveUserPermissionCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId)
            ?? throw new NotFoundException("Usuário não encontrado.");

        if (!await userManager.IsInRoleAsync(user, command.PermissionName))
            throw new NotFoundException("Usuário não possui essa permissão.");

        var result = await userManager.RemoveFromRoleAsync(user, command.PermissionName);
        result.EnsureSucceeded();
    }
}
