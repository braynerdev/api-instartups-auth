using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.UseCases.User.GetUserByIdQuery;

public class GetUserByIdQueryValidation : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidation()
    {
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull());
    }
}
