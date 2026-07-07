using HealthcareCRM.Helpers;
using HealthcareCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        /// <summary>
        /// Updates the currently logged-in doctor's fee.
        /// </summary>
        [HttpPost("fee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateFee([FromBody] UpdateFeeRequest request)
        {
            var userRole = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);
            if (userRole != "Doctor")
            {
                return Unauthorized(ApiResponse<object>.CreateError("Only doctors can update fees."));
            }

            var doctorId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) 
                           ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(doctorId))
            {
                return Unauthorized(ApiResponse<object>.CreateError("Invalid doctor identity."));
            }

            var result = await _doctorService.UpdateFeeAsync(doctorId, request.Fee, request.Currency);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.CreateError(result.Message));
            }

            return Ok(ApiResponse<object>.CreateSuccess(null, result.Message));
        }
    }

    /// <summary>
    /// Payload structure for updating doctor's fee.
    /// </summary>
    public class UpdateFeeRequest
    {
        public decimal Fee { get; set; }
        public string Currency { get; set; } = "Rs";
    }
}
