using Api.Instartups.Auth.Common.Identity;
using Api.Instartups.Auth.Exceptions.Base;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.Interfaces.Command;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.src.UseCases.User.AddUserPermissionCommand;

public class AddUserPermissionCommandHandler(
        UserManager<ApplicationUser> userManager
    ) : IVoidCommandHandler<AddUserPermissionCommand>
{
    public async Task Handle(AddUserPermissionCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId)
            ?? throw new NotFoundException("Usuário não encontrado.");

        if (await userManager.IsInRoleAsync(user, command.PermissionName))
            throw new ConflictException("Usuário já possui essa permissão.");

        var result = await userManager.AddToRoleAsync(user, command.PermissionName);
        result.EnsureSucceeded();
    }
}
