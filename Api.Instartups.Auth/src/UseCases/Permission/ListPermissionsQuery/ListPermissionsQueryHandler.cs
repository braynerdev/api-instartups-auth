using Api.Instartups.Auth.Constants;
using Api.Instartups.Auth.src.Interfaces.Query;

namespace Api.Instartups.Auth.UseCases.Permission.ListPermissionsQuery;

public class ListPermissionsQueryHandler : IQueryHandler<ListPermissionsQuery, ListPermissionsQueryResponse>
{
    public Task<ListPermissionsQueryResponse> Handle(ListPermissionsQuery query, CancellationToken ct)
    {
        var permissions = PermissionConst.All.Select(p => p.Name).ToList();

        return Task.FromResult(new ListPermissionsQueryResponse(permissions));
    }
}
