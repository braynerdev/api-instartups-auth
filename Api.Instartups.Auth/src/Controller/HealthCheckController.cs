using Api.Instartups.Auth.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Instartups.Auth.Controller;

[Route("api/[controller]")]
[ApiController]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public ActionResult<BaseResponseDTO<string>> Health()
    {
        return Ok(BaseResponseDTO<string>.Success("ok"));
    }
}
