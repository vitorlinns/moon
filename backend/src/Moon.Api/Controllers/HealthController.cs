using Microsoft.AspNetCore.Mvc;
using Moon.Api.Data;

namespace Moon.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        return Ok(new
        {
            status = "ok",
            database = canConnect ? "connected" : "unavailable"
        });
    }
}
