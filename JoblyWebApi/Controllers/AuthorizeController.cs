using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoblyWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        [HttpPost("Register")]
        public IActionResult Register([FromBody] User user)
        {

            return Ok();
        }
    }
}
