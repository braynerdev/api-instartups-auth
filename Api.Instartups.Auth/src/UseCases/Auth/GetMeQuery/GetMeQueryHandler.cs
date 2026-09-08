using Api.Instartups.Auth.Exceptions;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.Interfaces.Query;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.UseCases.Auth.GetMeQuery;

public class GetMeQueryHandler(
        UserManager<ApplicationUser> userManager
    ) : IQueryHandler<GetMeQuery, GetMeQueryResponse>
{
    public async Task<GetMeQueryResponse> Handle(GetMeQuery query, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(query.UserId)
            ?? throw new UnauthorizedException();

        return new GetMeQueryResponse(
            user.Id,
            user.UserName!,
            user.Email!,
            user.PhoneNumber
        );
    }
}
