using JoblyWebApi.Data.Models;
using JoblyWebApi.Services;
using JoblyWebApi.Services.Interface;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwt;
    private readonly UserRepository _repo = new UserRepository();
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
    public IActionResult Login([FromBody] UserLogin request)
    {
        var existing = _repo.GetByEmail(request.Email);
        if (existing == null || !BCrypt.Net.BCrypt.Verify(request.Password, existing.PasswordHash))
            return Unauthorized("Invalid credentials");

        string token = _jwt.GenerateToken(existing);
        return Ok(new { Token = token });
    }

}
