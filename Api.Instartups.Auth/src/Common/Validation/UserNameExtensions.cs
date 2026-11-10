using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.Common.Commands;

public static class UserNameExtensions
{
    public static IRuleBuilderOptions<T, string> ValidationUserName<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull())
            .MaximumLength(20)
            .WithErrorCode(CodeError.MaxLength)
            .WithMessage(MessageError.MaxLength(20))
            // O login trata qualquer valor com '@' como e-mail, então o nome de usuário não pode conter '@'.
            .Must(userName => userName is null || !userName.Contains('@'))
            .WithErrorCode(CodeError.Format)
            .WithMessage(MessageError.UserNameWithAt());
    }
}