using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.UseCases.Auth.RefreshTokenCommand;

public class RefreshTokenCommandValidation
    : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidation()
    {
        RuleFor(refresh => refresh.RefreshToken)
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull());
    }
}
