using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("heltz")]
    public class HeltzController : Controller
    {
        [HttpGet("check")]
        public IActionResult Index()
        {
            return Ok("Tudo certo!");
        }
    }
}
