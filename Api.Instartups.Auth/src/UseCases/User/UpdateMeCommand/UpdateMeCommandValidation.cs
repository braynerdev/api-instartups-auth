using Api.Instartups.Auth.Common.Commands;
using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.src.UseCases.User.UpdateMeCommand;

public class UpdateMeCommandValidation
    : AbstractValidator<UpdateMeCommand>
{
    public UpdateMeCommandValidation()
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
    }
}
