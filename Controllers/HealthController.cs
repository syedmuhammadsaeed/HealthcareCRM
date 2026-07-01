using HealthcareCRM.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers
{
    /// <summary>
    /// Lightweight health-check endpoint for infrastructure monitoring.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Returns a 200 OK response confirming the API is operational.
        /// </summary>
        /// <response code="200">API is running and healthy.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(ApiResponse<object>.CreateSuccess(new { }, "API is running"));
        }
    }
}
