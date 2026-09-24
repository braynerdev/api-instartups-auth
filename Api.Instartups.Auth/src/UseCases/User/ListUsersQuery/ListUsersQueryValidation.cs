using FluentValidation;

namespace Api.Instartups.Auth.UseCases.User.ListUsersQuery;

public class ListUsersQueryValidation : AbstractValidator<ListUsersQuery>
{
    public ListUsersQueryValidation()
    {
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("O tamanho da página deve estar entre 1 e 100.");
    }
}
