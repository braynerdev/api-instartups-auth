using Api.Instartups.Auth.Constants;
using Api.Instartups.Auth.DTOs;
using Api.Instartups.Auth.UseCases.Permission.ListPermissionsQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Api.Instartups.Auth.src.Controller;

[Route("api/[controller]")]
[ApiController]
public class PermissionController(
        IMessageBus bus
    ) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionConst.Admin)]
    public async Task<ActionResult<BaseResponseDTO<ListPermissionsQueryResponse>>> ListPermissions(
        [FromQuery] string? cursor,
        [FromQuery] int pageSize,
        [FromQuery] string? search,
        CancellationToken ct
    )
    {
        var query = new ListPermissionsQuery(cursor, pageSize <= 0 ? 20 : pageSize, search);
        var response = await bus.InvokeAsync<ListPermissionsQueryResponse>(query, ct);
        return Ok(BaseResponseDTO<ListPermissionsQueryResponse>.Success(response));
    }
}
