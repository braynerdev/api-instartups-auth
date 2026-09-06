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
    public async Task<RegisterUserCommandResponse> Handle(RegisterUserCommand command,  CancellationToken ct)
    {
        var user = CreateUser(command);
        var result = await _userManager.CreateAsync(user, command.Password);
        Validate(result);
        return user.Adapt<RegisterUserCommandResponse>();
    }

    private IdentityUser CreateUser(RegisterUserCommand command)
    {
        return new IdentityUser
        {
            UserName = command.UserName, 
            Email = command.Email, 
            PhoneNumber = command.PhoneNumber
        };
    }

    private void Validate(IdentityResult result)
    {
        if (result.Succeeded)
            return;
        
        throw new IdentityValidationException(
            result.Errors
                .GroupBy(r => new
                {
                    Field = GetField(r.Code),
                    Code = r.Code
                })
                .Select(group =>
                    new ValidateErrorDTO
                    (
                        GetField(group.First().Code), 
                        group.First().Code, 
                        group.Select(e => e.Description).ToList()
                    )
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
