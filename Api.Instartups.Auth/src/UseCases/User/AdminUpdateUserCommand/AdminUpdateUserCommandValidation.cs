using Api.Instartups.Auth.Common.Commands;
using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.src.UseCases.User.AdminUpdateUserCommand;

public class AdminUpdateUserCommandValidation
    : AbstractValidator<AdminUpdateUserCommand>
{
    public AdminUpdateUserCommandValidation()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull());

        RuleFor(command => command.UserName)
            .ValidationUserName();

        RuleFor(command => command.Email)
            .ValidationEmail();

        RuleFor(command => command.PhoneNumber)
            .ValidationPhoneNumber()
            .When(command => !string.IsNullOrWhiteSpace(command.PhoneNumber));

        RuleFor(command => command.NewPassword!)
            .ValidationPassword()
            .When(command => !string.IsNullOrWhiteSpace(command.NewPassword));
    }
}
