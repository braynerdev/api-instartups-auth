using Api.Instartups.Auth.Configurations.Extension;
using Api.Instartups.Auth.Constants;
using Api.Instartups.Auth.DTOs;
using Api.Instartups.Auth.src.UseCases.User.AddUserPermissionCommand;
using Api.Instartups.Auth.src.UseCases.User.AdminUpdateUserCommand;
using Api.Instartups.Auth.src.UseCases.User.ChangePasswordCommand;
using Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;
using Api.Instartups.Auth.src.UseCases.User.RemoveUserPermissionCommand;
using Api.Instartups.Auth.src.UseCases.User.UpdateMeCommand;
using Api.Instartups.Auth.UseCases.User.GetMeQuery;
using Api.Instartups.Auth.UseCases.User.GetUserByIdQuery;
using Api.Instartups.Auth.UseCases.User.ListUsersQuery;
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
    public async Task<ActionResult<BaseResponseDTO<RegisterUserCommandResponse>>> RegisterUser(
        CancellationToken ct,
        [FromBody] RegisterUserCommand command
    )
    {
        var response = await bus.InvokeAsync<RegisterUserCommandResponse>(command, ct);
        return Ok(BaseResponseDTO<RegisterUserCommandResponse>.Success(response));
    }

    [HttpGet]
    [Authorize(Policy = PermissionConst.Admin)]
    public async Task<ActionResult<BaseResponseDTO<ListUsersQueryResponse>>> ListUsers(
        [FromQuery] string? cursor,
        [FromQuery] int pageSize,
        CancellationToken ct
    )
    {
        var query = new ListUsersQuery(cursor, pageSize <= 0 ? 20 : pageSize);
        var response = await bus.InvokeAsync<ListUsersQueryResponse>(query, ct);
        return Ok(BaseResponseDTO<ListUsersQueryResponse>.Success(response));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionConst.Admin)]
    public async Task<ActionResult<BaseResponseDTO<GetUserByIdQueryResponse>>> GetUserById(
        [FromRoute] string id,
        CancellationToken ct
    )
    {
        var query = new GetUserByIdQuery(id);
        var response = await bus.InvokeAsync<GetUserByIdQueryResponse>(query, ct);
        return Ok(BaseResponseDTO<GetUserByIdQueryResponse>.Success(response));
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

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionConst.Admin)]
    public async Task<ActionResult<BaseResponseDTO<AdminUpdateUserCommandResponse>>> AdminUpdateUser(
        [FromRoute] string id,
        [FromBody] AdminUpdateUserRequest request,
        CancellationToken ct
    )
    {
        var command = new AdminUpdateUserCommand(
            User.GetUserId(), id, request.UserName, request.Email,
            request.PhoneNumber, request.NewPassword, request.IsLocked);
        var response = await bus.InvokeAsync<AdminUpdateUserCommandResponse>(command, ct);
        return Ok(BaseResponseDTO<AdminUpdateUserCommandResponse>.Success(response));
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

    [HttpPost("{id}/permissions/{permissionName}")]
    [Authorize(Policy = PermissionConst.Admin)]
    public async Task<ActionResult<BaseResponseDTO<string>>> AddUserPermission(
        [FromRoute] string id,
        [FromRoute] string permissionName,
        CancellationToken ct
    )
    {
        var command = new AddUserPermissionCommand(id, permissionName);
        await bus.InvokeAsync(command, ct);
        return Ok(BaseResponseDTO<string>.Success(null!, "Permissão adicionada com sucesso."));
    }

    [HttpDelete("{id}/permissions/{permissionName}")]
    [Authorize(Policy = PermissionConst.Admin)]
    public async Task<ActionResult<BaseResponseDTO<string>>> RemoveUserPermission(
        [FromRoute] string id,
        [FromRoute] string permissionName,
        CancellationToken ct
    )
    {
        var command = new RemoveUserPermissionCommand(id, permissionName);
        await bus.InvokeAsync(command, ct);
        return Ok(BaseResponseDTO<string>.Success(null!, "Permissão removida com sucesso."));
    }
}
