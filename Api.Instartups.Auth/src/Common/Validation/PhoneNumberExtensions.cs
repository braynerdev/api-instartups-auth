using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.Common.Commands;

public static class PhoneNumberExtensions
{
    public static IRuleBuilderOptions<T, string?> ValidationPhoneNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(20)
            .WithErrorCode(CodeError.MaxLength)
            .WithMessage(MessageError.MaxLength(20));
    }
}   