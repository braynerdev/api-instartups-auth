using Api.Instartups.Auth.Constants;
using FluentValidation;

namespace Api.Instartups.Auth.UseCases.User.GetMeQuery;

public class GetMeQueryValidation : AbstractValidator<GetMeQuery>
{
    public GetMeQueryValidation()
    {
        RuleFor(query => query.UserId)
            .NotEmpty()
            .WithErrorCode(CodeError.NotEmptyOrNull)
            .WithMessage(MessageError.NotEmptyOrNull());
    }
}
