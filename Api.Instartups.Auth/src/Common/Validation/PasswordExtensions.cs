using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.Common.Commands;

public static class PasswordExtensions
{
    public static IRuleBuilderOptions<T, string> ValidationPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull())
            .MinimumLength(8)
            .WithErrorCode(CodeError.MinLength)
            .WithMessage(MessageError.MinLength(8))
            .MaximumLength(20)
            .WithErrorCode(CodeError.MaxLength)
            .WithMessage(MessageError.MaxLength(20))
            .Must(PasswordComplexity)
            .WithErrorCode(CodeError.PasswordComplexity)
            .WithMessage(MessageError.PasswordComplexity());
    }
    
    private static bool PasswordComplexity(string password)
    {
        password = password.Trim();

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
        
        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}