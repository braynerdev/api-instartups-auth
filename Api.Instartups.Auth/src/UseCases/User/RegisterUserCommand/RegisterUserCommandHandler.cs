using Api.Instartups.Auth.DTOs;
using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.src.Interfaces.Command;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;

public class RegisterUserCommandHandler
        : ICommandHandler<RegisterUserCommand,RegisterUserCommandResponse>
{
    private readonly UserManager<IdentityUser> _userManager;
    
    public RegisterUserCommandHandler(UserManager<IdentityUser> userManager) => _userManager = userManager;
    public async Task<RegisterUserCommandResponse> Handle(RegisterUserCommand Command,  CancellationToken ct)
    {
        var user = CreateUser(Command);
        var result = await _userManager.CreateAsync(user, Command.Password);
        Validate(result);
        return user.Adapt<RegisterUserCommandResponse>();
    }

    private IdentityUser CreateUser(RegisterUserCommand command)
    {
        return new IdentityUser
        {
            UserName = command.Username, 
            Email = command.Email, 
            PhoneNumber = command.PhoneNumber
        };
    }

    private void Validate(IdentityResult result)
    {
        if (result.Succeeded)
            return;

        throw new IdentityValidationException(
            result.Errors.Select(e =>
                new ValidateErrorDTO { Field = GetField(e.Code), Code = e.Code, Message = e.Description }
            )
        );
    }
    
    private static string GetField(string code)
    {
        return code switch
        {
            "DuplicateUserName" or "InvalidUserName" => "username",
            "DuplicateEmail" or "InvalidEmail" => "email",

            "PasswordTooShort" or
                "PasswordRequiresDigit" or
                "PasswordRequiresUpper" or
                "PasswordRequiresLower" or
                "PasswordRequiresNonAlphanumeric" or
                "PasswordRequiresUniqueChars" => "password",

            _ => string.Empty
        };
    }
}
