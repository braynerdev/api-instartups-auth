using Api.Instartups.Auth.Configurations.Extension;
using Api.Instartups.Auth.DTOs;
using Api.Instartups.Auth.src.UseCases.User.ChangePasswordCommand;
using Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;
using Api.Instartups.Auth.src.UseCases.User.UpdateMeCommand;
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

    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<BaseResponseDTO<UpdateMeCommandResponse>>> UpdateMe(
        [FromBody] UpdateMeRequest request,
        CancellationToken ct
    )
    {
        var command = new UpdateMeCommand(User.GetUserId(), request.UserName, request.Email, request.PhoneNumber);
        var response = await bus.InvokeAsync<UpdateMeCommandResponse>(command, ct);
        return Ok(BaseResponseDTO<UpdateMeCommandResponse>.Success(response));
    }

    [HttpPut("me/password")]
    [Authorize]
    public async Task<ActionResult<BaseResponseDTO<string>>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct
    )
    {
        var command = new ChangePasswordCommand(User.GetUserId(), request.CurrentPassword, request.NewPassword);
        await bus.InvokeAsync(command, ct);
        return Ok(BaseResponseDTO<string>.Success(null!, "Senha alterada com sucesso."));
    }
}
