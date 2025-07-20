using JoblyWebApi.Services.Interface;
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
    private readonly ICommonService _commonService;

    public AutoApplyController(IConfiguration config,ICommonService commonService)
    {
        _config = config;
        _commonService = commonService;
    }

    [HttpPost("naukri")]
    [AllowAnonymous]
    public IActionResult Apply(int userId)
    {
        userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
        {
            userId = 1;
        }
        var user = _commonService.ApplyService.Result.GetNaukriUser(userId).Result;
        if (user == null) return BadRequest("No filters found");

        var engine = new NaukriApplyEngine(user.NaukriId, user.NaukriPassword, userId, _config);
        engine.Run(user.Role, user.Location, user.Skills);

        return Ok("Naukri auto-apply completed");
    }
}
