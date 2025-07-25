using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Security.Claims;
using JoblyWebApi.Repositories;
using JoblyWebApi.Interface;
using JoblyWebApi.Services;
using joblywebapi.Services;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NaukriController : ControllerBase
{
  
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Func<int, IGroqService> _groqFactory; 
        private readonly NaukriLoginService _naukriLogin;
        private readonly LinkedInService _linkedIn;


        public NaukriController(
            IConfiguration config,
            IWebHostEnvironment env,
            IUnitOfWork unitOfWork,
            Func<int, IGroqService> groqFactory,
            NaukriLoginService naukriLogin,
            LinkedInService linkedIn)
        {
            _config = config;
            _env = env;
            _unitOfWork = unitOfWork;
            _groqFactory = groqFactory;
            _naukriLogin = naukriLogin;
            _linkedIn = linkedIn;
        }
    


    [HttpPost("naukri")]
    [AllowAnonymous]
    public async Task<IActionResult> Apply()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        userId = 2;

        // ✅ Await the Task to resolve the issue  
        var cred = await _unitOfWork.NaukriCredentials.GetByUserIdAsync(userId);
        if (cred == null)
            return BadRequest("No Naukri credentials found");

        string email = cred.Username;
        string password = cred.Password;

        // ✅ Pass credentials to engine with required parameters  
        var engine = new NaukriApplyEngine(email, password, userId, _config,_groqFactory, _unitOfWork);
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

        _unitOfWork.Resume.Save(userId, model.File.FileName);
        return Ok("Resume uploaded");
    }


    [HttpPost("save-credentials")]
    public async Task<IActionResult> SaveCredentials([FromBody] NaukriCredentialDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        bool loginSuccess = await _naukriLogin.TryLoginAsync(dto.Username, dto.Password);
        if (!loginSuccess)
            return BadRequest(new { message = "❌ Invalid Naukri login." });

        await _unitOfWork.NaukriCredentials.SaveNaukriCredentialsAsync(userId, dto.Username, dto.Password);

        return Ok(new { message = "✅ Saved successfully after verifying login." });
    }

    [HttpGet("credentials")]
    public IActionResult GetByUserId()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var data = _unitOfWork.NaukriCredentials.GetByUserIdAsync(userId);
        if (data == null)
            return NotFound(new { message = "❌ No credentials found." });

        return Ok(data);
    }

 
        [HttpGet("paged")]
        public IActionResult GetPagedJobs(int page = 1, int pageSize = 10)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var data = _unitOfWork.Naukri.GetPagedJobsByUserId(userId, page, pageSize);
            return Ok(data);
        }

    [HttpPost("run")]
    [AllowAnonymous]
    public IActionResult Run()
    {
        var result = _linkedIn.RunLinkedInAutomation();
        return Ok(new { message = result });
    }

}
