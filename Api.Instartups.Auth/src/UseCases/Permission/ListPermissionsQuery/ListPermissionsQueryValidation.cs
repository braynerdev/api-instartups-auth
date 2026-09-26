using FluentValidation;

namespace Api.Instartups.Auth.UseCases.Permission.ListPermissionsQuery;

public class ListPermissionsQueryValidation : AbstractValidator<ListPermissionsQuery>
{
    public ListPermissionsQueryValidation()
    {
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("O tamanho da página deve estar entre 1 e 100.");

        RuleFor(query => query.Search)
            .MaximumLength(100)
            .WithMessage("O filtro de busca deve ter no máximo 100 caracteres.");
    }
}
