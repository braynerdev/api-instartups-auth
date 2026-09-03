using Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;
using Microsoft.AspNetCore.Mvc;

namespace Api.Instartups.Auth.src.Controller;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    // Vou trocar o fromService pelo Wolverine
    // 
    [HttpPost]
    public async Task<IActionResult> RegisterUser(
        [FromServices] RegisterUserCommandHandler service,
        [FromBody] RegisterUserCommand command
    )
    {
        await service.Handle(command);
        return Created();
    }
}
