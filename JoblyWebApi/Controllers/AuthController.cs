using JoblyWebApi.Data.Models;
using JoblyWebApi.Services.Interface;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwt;
    private readonly ICommonService _commonService;

    public AuthController(IConfiguration config, ICommonService commonService)
    {
        _jwt = new JwtService(config["Jwt:Key"]);
        _commonService = commonService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] UserRegister user)
    {
        var result = await _commonService.AuthService.Result.Register(user);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLogin request)
    {
        bool userExists = await _commonService.AuthService.Result.CheckUser(request);

        if (userExists)
        {
            var token = _jwt.GenerateToken(request);
            return Ok(token);
        }
        else
        {
            return Unauthorized("Invalid credentials");
        }
    }

}
