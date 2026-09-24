using Api.Instartups.Auth.Common.Identity;
using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.Interfaces.Command;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.src.UseCases.User.UpdateMeCommand;

public class UpdateMeCommandHandler(
        UserManager<ApplicationUser> userManager
    ) : ICommandHandler<UpdateMeCommand, UpdateMeCommandResponse>
{
    public async Task<UpdateMeCommandResponse> Handle(UpdateMeCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(command.UserId)
            ?? throw new UnauthorizedException();

        UpdateUser(user, command);

        var result = await userManager.UpdateAsync(user);
        result.EnsureSucceeded();

        return user.Adapt<UpdateMeCommandResponse>();
    }

    public void UpdateUser(ApplicationUser user, UpdateMeCommand command)
    {
        user.UserName = command.UserName;
        user.Email = command.Email;
        user.PhoneNumber = command.PhoneNumber;
    }
}
