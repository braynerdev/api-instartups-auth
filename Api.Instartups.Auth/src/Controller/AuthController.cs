using Api.Instartups.Auth.Configurations.Extension;
using Api.Instartups.Auth.DTOs;
using Api.Instartups.Auth.UseCases.Auth.LoginCommand;
using Api.Instartups.Auth.UseCases.Auth.RefreshTokenCommand;
using Api.Instartups.Auth.UseCases.Auth.RevokeAllTokensCommand;
using Api.Instartups.Auth.UseCases.Auth.RevokeTokenCommand;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public async Task<ActionResult<BaseResponseDTO<RefreshTokenCommandResponse>>> Refresh(
        CancellationToken ct,
        [FromBody] RefreshTokenCommand command
    )
    {
        var response = await bus.InvokeAsync<RefreshTokenCommandResponse>(command, ct);
        return Ok(BaseResponseDTO<RefreshTokenCommandResponse>.Success(response));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<BaseResponseDTO<string>>> Logout(
        CancellationToken ct,
        [FromBody] RevokeTokenCommand command
    )
    {
        await bus.InvokeAsync(command, ct);
        return Ok(BaseResponseDTO<string>.Success(null!, "Logout realizado com sucesso."));
    }

    [HttpPost("logout/all")]
    [Authorize]
    public async Task<ActionResult<BaseResponseDTO<string>>> LogoutAll(
        CancellationToken ct
    )
    {
        var command = new RevokeAllTokensCommand(User.GetUserId());
        await bus.InvokeAsync(command, ct);
        return Ok(BaseResponseDTO<string>.Success(null!, "Sessões encerradas com sucesso."));
    }
}
