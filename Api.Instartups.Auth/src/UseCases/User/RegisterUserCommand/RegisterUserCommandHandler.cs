using Api.Instartups.Auth.Common.Identity;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.Interfaces.Command;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;

public class RegisterUserCommandHandler(
    UserManager<ApplicationUser> userManager)
        : ICommandHandler<RegisterUserCommand,RegisterUserCommandResponse>
{
    public async Task<RegisterUserCommandResponse> Handle(RegisterUserCommand command,  CancellationToken ct)
    {
        var user = CreateUser(command);
        var result = await userManager.CreateAsync(user, command.Password);
        result.EnsureSucceeded();
        return user.Adapt<RegisterUserCommandResponse>();
    }

    private ApplicationUser CreateUser(RegisterUserCommand command)
    {
        return new ApplicationUser
        {
            UserName = command.UserName,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber
        };
    }
}
