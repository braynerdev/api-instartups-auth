using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.UseCases.Auth.RevokeTokenCommand;

public class RevokeTokenCommandValidation
    : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidation()
    {
        RuleFor(revoke => revoke.RefreshToken)
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull());
    }
}
