using HealthcareCRM.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Checks API health.
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(ApiResponse<object>.CreateSuccess(new { status = "Healthy" }, "API is running"));
        }
    }
}
