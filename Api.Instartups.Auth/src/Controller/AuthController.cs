using Api.Instartups.Auth.DTOs;
using Api.Instartups.Auth.UseCases.Auth.LoginCommand;
using Api.Instartups.Auth.UseCases.Auth.RefreshTokenCommand;
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

    [HttpPost("refresh")]
    public async Task<ActionResult<BaseResponseDTO<RefreshTokenCommandResponse>>> Refresh(
        CancellationToken ct,
        [FromBody] RefreshTokenCommand command
    )
    {
        var response = await bus.InvokeAsync<RefreshTokenCommandResponse>(command, ct);
        return Ok(BaseResponseDTO<RefreshTokenCommandResponse>.Success(response));
    }
}
