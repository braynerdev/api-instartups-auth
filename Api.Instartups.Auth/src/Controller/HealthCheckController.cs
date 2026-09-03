using Microsoft.AspNetCore.Mvc;

namespace Api.Instartups.Auth.Controller;

[Route("api/[controller]")]
[ApiController]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public IActionResult Health()
    {
        return Ok("ok");
    }
}
