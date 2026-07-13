using HealthcareCRM.Helpers;
using HealthcareCRM.Services;
using HealthcareCRM.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers
{
    /// <summary>
    /// API controller for user registration and login.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        /// <summary>
        /// Initialises AuthController with the authentication service.
        /// </summary>
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user account with a securely hashed password.
        /// </summary>
        /// <param name="model">Registration data: full name, email, password, and confirmation.</param>
        /// <response code="201">Account created successfully.</response>
        /// <response code="400">Validation failed or email already registered.</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

            return Created(string.Empty, ApiResponse<object?>.CreateSuccess(null, result.Message));
        }

        /// <summary>
        /// Validates user credentials and issues a signed JWT token on success.
        /// </summary>
        /// <param name="model">Login credentials: email and password.</param>
        /// <response code="200">Login successful — JWT token returned in data.token.</response>
        /// <response code="400">Validation failed — missing or malformed input.</response>
        /// <response code="401">Invalid email or password.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

            return Ok(ApiResponse<object?>.CreateSuccess(new { token = result.Token }, result.Message));
        }
    }
}
