using Api.Instartups.Auth.Common.Commands;
using Api.Instartups.Auth.Constants;
using Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;
using FluentValidation;

namespace Api.Instartups.Auth.UseCases.Auth.LoginCommand;

public class LoginCommandValidation 
    : AbstractValidator<LoginCommand>
{
    public LoginCommandValidation()
    {
        RuleFor(login => login.Password)
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull());

        RuleFor(login => login.UserNameOrEmail)
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull());
    }
}