using Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;
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
}
