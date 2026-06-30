using HealthcareCRM.Helpers;
using HealthcareCRM.Services;
using HealthcareCRM.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.CreateError("Invalid registration data."));
            }

            var result = await _authService.RegisterAsync(model);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.CreateError(result.Message));
            }

            return Created(string.Empty, ApiResponse<object>.CreateSuccess(null, result.Message));
        }

        /// <summary>
        /// Validates user credentials and generates a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.CreateError("Invalid login data."));
            }

            var result = await _authService.LoginAsync(model);
            if (!result.IsSuccess)
            {
                return Unauthorized(ApiResponse<object>.CreateError(result.Message));
            }

            return Ok(ApiResponse<object>.CreateSuccess(new { token = result.Token }, result.Message));
        }
    }
}
