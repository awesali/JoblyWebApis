using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using JoblyWebApi.Repositories;

namespace JoblyWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ResumeController : ControllerBase
    {
        private readonly ResumeRepository _repo = new ResumeRepository();
        private readonly IWebHostEnvironment _env;

        public ResumeController(IWebHostEnvironment env)
        {
            _env = env;
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
    }
}
