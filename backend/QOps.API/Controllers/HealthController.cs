using Microsoft.AspNetCore.Mvc;

namespace QOps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            application = "QOps",
            version = "1.0.0"
        });
    }
}
