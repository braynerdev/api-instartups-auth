using Api.Instartups.Auth.Common.Commands;
using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;

public class RegisterUserCommandValidation
    : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidation()
    {
        RuleFor(user => user.UserName)
            .ValidationUserName();
        
        RuleFor(user => user.Email)
            .ValidationEmail();
        
        RuleFor(user => user.PhoneNumber)
            .ValidationPhoneNumber()
            .When(user => !string.IsNullOrWhiteSpace(user.PhoneNumber));

        RuleFor(user => user.Password)
            .ValidationPassword();
    }
}
