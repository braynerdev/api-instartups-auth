using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.src.UseCases.User.RemoveUserPermissionCommand;

public class RemoveUserPermissionCommandValidation : AbstractValidator<RemoveUserPermissionCommand>
{
    public RemoveUserPermissionCommandValidation()
    {
        RuleFor(command => command.UserId)
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull());

        RuleFor(command => command.PermissionName)
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull())
            .Must(name => PermissionConst.All.Any(p => p.Name == name))
            .WithErrorCode(CodeError.Format)
            .WithMessage("Permissão inválida.");
    }
}
