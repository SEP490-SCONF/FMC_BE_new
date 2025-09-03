using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ConferenceFWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(IStringLocalizer<HomeController> localizer)
        {
            _localizer = localizer;
        }

        [HttpGet("welcome")]
        public IActionResult GetWelcome()
        {
            var message = _localizer["Welcome"];
            return Ok(new { message });
        }

        [HttpPost("submit-paper")]
        public IActionResult SubmitPaper()
        {
            var message = _localizer["SubmitSuccess"];
            return Ok(new { success = true, message });
        }
    }
}
