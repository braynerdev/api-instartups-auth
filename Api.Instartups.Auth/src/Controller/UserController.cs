using Api.Instartups.Auth.Configurations.Extension;
using Api.Instartups.Auth.DTOs;
using Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;
using Api.Instartups.Auth.UseCases.User.GetMeQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Api.Instartups.Auth.src.Controller;

[Route("api/[controller]")]
[ApiController]
public class UserController(
        IMessageBus bus
    ) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RegisterUserCommandResponse>> RegisterUser(
        CancellationToken ct,
        [FromBody] RegisterUserCommand command
    )
    {
        var response = await bus.InvokeAsync<RegisterUserCommandResponse>(command, ct);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<BaseResponseDTO<GetMeQueryResponse>>> Me(
        CancellationToken ct
    )
    {
        var query = new GetMeQuery(User.GetUserId());
        var response = await bus.InvokeAsync<GetMeQueryResponse>(query, ct);
        return Ok(BaseResponseDTO<GetMeQueryResponse>.Success(response));
    }
}
