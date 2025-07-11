using Microsoft.AspNetCore.Mvc;
using ReqPrioritizer;

namespace TestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestPriorityController : ControllerBase
    {
        [HttpGet("high")]
        [Priority("high")]
        public IActionResult HighPriority()
        {
            return Ok("This is a high priority request.");
        }

        [HttpGet("default")]
        [Priority("default")]
        public IActionResult DefaultPriority()
        {
            return Ok("This is a default priority request.");
        }

        [HttpGet("no-priority")]
        public IActionResult NoPriority()
        {
            return Ok("This request has no explicit priority.");
        }
    }
}
