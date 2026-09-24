using Api.Instartups.Auth.Exceptions.Base;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.Interfaces.Query;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.UseCases.User.GetUserByIdQuery;

public class GetUserByIdQueryHandler(
        UserManager<ApplicationUser> userManager
    ) : IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse>
{
    public async Task<GetUserByIdQueryResponse> Handle(GetUserByIdQuery query, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(query.UserId);

        if (user is null)
            throw new NotFoundException("Usuário não encontrado.");

        var roles = await userManager.GetRolesAsync(user);
        var isLocked = await userManager.IsLockedOutAsync(user);

        return new GetUserByIdQueryResponse(
            user.Id,
            user.UserName!,
            user.Email!,
            user.PhoneNumber,
            roles.ToList(),
            isLocked
        );
    }
}
