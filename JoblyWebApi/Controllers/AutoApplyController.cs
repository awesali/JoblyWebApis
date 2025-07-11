using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AutoApplyController : ControllerBase
{
    private readonly IConfiguration _config;

    public AutoApplyController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("naukri")]
    [AllowAnonymous]
    public IActionResult Apply()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        userId = 1;
       
        var filters = new JobFilterRepository().GetByUserId(userId).FirstOrDefault();
        if (filters == null) return BadRequest("No filters found");

        string email = "sayedawesali190@gmail.com";
        string password = "awes@123";

        var engine = new NaukriApplyEngine(email, password, userId, _config);
        engine.Run(filters.Role, filters.Location, filters.Skills);

        return Ok("Naukri auto-apply completed");
    }
}
