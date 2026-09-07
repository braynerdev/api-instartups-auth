using Api.Instartups.Auth.DTOs;
using Api.Instartups.Auth.UseCases.Auth.LoginCommand;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Api.Instartups.Auth.src.Controller;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
        IMessageBus bus
    ) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<BaseResponseDTO<LoginCommandResponse>>> Login(
        CancellationToken ct,
        [FromBody] LoginCommand command
    )
    {
        var response = await bus.InvokeAsync<LoginCommandResponse>(command, ct);
        return Ok(BaseResponseDTO<LoginCommandResponse>.Success(response));
    }
}
