using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.UseCases.Auth.RevokeAllTokensCommand;

public class RevokeAllTokensCommandValidation
    : AbstractValidator<RevokeAllTokensCommand>
{
    public RevokeAllTokensCommandValidation()
    {
        RuleFor(revoke => revoke.RefreshToken)
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull());
    }
}
