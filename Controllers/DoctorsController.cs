using HealthcareCRM.Helpers;
using HealthcareCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers
{
    /// <summary>
    /// API controller for doctor record management.
    /// All endpoints require a valid JWT Bearer token.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly DoctorService _doctorService;

        public DoctorsController(DoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        /// <summary>
        /// Retrieves a list of active doctors.
        /// </summary>
        /// <response code="200">Returns the list of active doctors.</response>
        /// <response code="401">JWT token is missing or invalid.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get()
        {
            var doctors = await _doctorService.GetActiveDoctorsAsync();
            return Ok(ApiResponse<object>.CreateSuccess(doctors, "Active doctors retrieved successfully."));
        }
    }
}
