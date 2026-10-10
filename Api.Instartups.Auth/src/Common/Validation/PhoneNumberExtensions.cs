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
            .WithMessage(MessageError.MaxLength(20))
            .Must(OnlyDigits)
            .WithErrorCode(CodeError.Format)
            .WithMessage(MessageError.Format());
    }

    private static bool OnlyDigits(string? phoneNumber)
        => phoneNumber is null || phoneNumber.All(char.IsAsciiDigit);
}   