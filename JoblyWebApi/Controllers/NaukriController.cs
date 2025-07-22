using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Security.Claims;
using JoblyWebApi.Services.Interfaces;
using JoblyWebApi.Repositories;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NaukriController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ResumeRepository _repo = new ResumeRepository();
    private readonly IWebHostEnvironment _env;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INaukriLoginService _loginService;
    public NaukriController(IConfiguration config, IWebHostEnvironment env, IUnitOfWork unitOfWork, INaukriLoginService loginService)
    {
        _config = config;
        _env = env;
        _unitOfWork = unitOfWork;
        _loginService = loginService;
    }

    [HttpPost("naukri")]
    [AllowAnonymous]
    public IActionResult Apply()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        userId = 2;
        // ✅ Get Credentials using Stored Procedure
        var cred = new NaukriCredentialRepository().GetByUserId(userId);
        if (cred == null)
            return BadRequest("No Naukri credentials found");

        string email = cred.Username;
        string password = cred.Password;

        // ✅ Pass credentials to engine without filters
        var engine = new NaukriApplyEngine(email, password, userId, _config);
        engine.Run();

        return Ok("Naukri auto-apply started");
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] ResumeUploadModel model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (model.File == null || model.File.Length == 0)
            return BadRequest("Invalid file");

        // ✅ Dynamic path using _env.WebRootPath
        string folder = Path.Combine(_env.WebRootPath, "resumes", userId.ToString());
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder, model.File.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await model.File.CopyToAsync(stream);
        }

        _repo.Save(userId, model.File.FileName); // just the file name is saved in DB
        return Ok("Resume uploaded");
    }


    [HttpPost("save-credentials")]
    public async Task<IActionResult> SaveCredentials([FromBody] NaukriCredentialDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        bool loginSuccess = await _loginService.TryLoginAsync(dto.Username, dto.Password);
        if (!loginSuccess)
            return BadRequest(new { message = "❌ Invalid Naukri login." });

        await _unitOfWork.NaukriCredentialRepository.SaveNaukriCredentialsAsync(userId, dto.Username, dto.Password);

        return Ok(new { message = "✅ Saved successfully after verifying login." });
    }

    [HttpGet("credentials")]
    public IActionResult GetByUserId()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var data = new NaukriCredentialRepository().GetByUserId(userId);
        if (data == null)
            return NotFound(new { message = "❌ No credentials found." });

        return Ok(data);
    }

  

        [HttpGet("paged")]
        public IActionResult GetPagedJobs(int page = 1, int pageSize = 10)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var data = new AppliedJobRepository().GetPagedJobsByUserId(userId, page, pageSize);
            return Ok(data);
        }
   

}
