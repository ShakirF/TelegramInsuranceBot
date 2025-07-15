using Microsoft.AspNetCore.Mvc;

namespace TelegramBot.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("Bot is running");
    }
}
