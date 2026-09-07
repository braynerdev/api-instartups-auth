using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.Common.Commands;

public static class EmailExtensions
{
    public static IRuleBuilderOptions<T, string> ValidationEmail<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull())
            .EmailAddress()
            .WithErrorCode(CodeError.Format)
            .WithMessage(MessageError.Format())
            .MaximumLength(100)
            .WithErrorCode(CodeError.MaxLength)
            .WithMessage(MessageError.MaxLength(100));
    }
}