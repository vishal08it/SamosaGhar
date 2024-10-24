using Microsoft.AspNetCore.Mvc;

namespace SamosaGhar.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Welcome to Samosa Ghar!");
        }
    }
}
