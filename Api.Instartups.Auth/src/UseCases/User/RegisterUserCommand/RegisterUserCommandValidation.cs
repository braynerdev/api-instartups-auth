using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;

public class RegisterUserCommandValidation
    : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidation()
    {
        RuleFor(user => user.UserName)
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull())
            .MaximumLength(20)
            .WithErrorCode(CodeError.MaxLength)
            .WithMessage(MessageError.MaxLength(20));
        
        RuleFor(user => user.Email)
            .EmailAddress()
            .WithErrorCode(CodeError.Format)
            .WithMessage(MessageError.Format())
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull())
            .MaximumLength(100)
            .WithErrorCode(CodeError.MaxLength)
            .WithMessage(MessageError.MaxLength(100));
        
        RuleFor(user => user.PhoneNumber)
            .MaximumLength(20)
            .WithErrorCode(CodeError.MaxLength)
            .WithMessage(MessageError.MaxLength(20))
            .When(user => !string.IsNullOrWhiteSpace(user.PhoneNumber));

        RuleFor(user => user.Password)
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
