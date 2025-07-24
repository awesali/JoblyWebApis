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
    public async Task<IActionResult> Apply(int userId = 0)
    {
        // Try to get userId from the ClaimsPrincipal if not passed or 0
        if (userId == 0)
        {
            var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimUserId, out userId))
            {
                userId = 1; // fallback default userId
            }
        }

        var user = await _commonService.ApplyService.Result.GetNaukriUser(userId);
        if (user == null)
            return NotFound("No filters found");

        var result = await _commonService.ApplyService.Result.ExecuteApply(user);

        return Ok(result ? "Naukri auto-apply completed" : "Error While applying");
    }

}
