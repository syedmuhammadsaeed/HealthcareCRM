using HealthcareCRM.Helpers;
using HealthcareCRM.Services;
using HealthcareCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthcareCRM.Controllers
{
    /// <summary>
    /// API controller for patient record management.
    /// All endpoints require a valid JWT Bearer token.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly PatientService _patientService;

        /// <summary>
        /// Initialises PatientsController with the patient service.
        /// </summary>
        public PatientsController(PatientService patientService)
        {
            _patientService = patientService;
        }

        /// <summary>
        /// Retrieves all patient records, with optional case-insensitive search and pagination.
        /// </summary>
        /// <param name="search">Optional search term matched against name, phone, and address.</param>
        /// <param name="page">Page number (default 1).</param>
        /// <param name="pageSize">Number of items per page (default 20).</param>
        /// <response code="200">Returns the list of patients.</response>
        /// <response code="401">JWT token is missing or invalid.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userRole = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) 
                         ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            string? doctorId = null;
            if (userRole == "Doctor")
            {
                doctorId = userId;
            }

            var patients = await _patientService.GetPatientsAsync(search, page, pageSize, doctorId);
            return Ok(ApiResponse<object>.CreateSuccess(patients, "Patients retrieved successfully."));
        }

        /// <summary>
        /// Retrieves a single patient by ID.
        /// </summary>
        /// <param name="id">MongoDB ObjectId string.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null) return NotFound(ApiResponse<object>.CreateError("Patient not found."));
            return Ok(ApiResponse<object>.CreateSuccess(patient, "Patient retrieved successfully."));
        }

        /// <summary>
        /// Retrieves all appointments.
        /// </summary>
        [HttpGet("appointments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _patientService.GetAppointmentsAsync();
            return Ok(ApiResponse<object>.CreateSuccess(appointments, "Appointments retrieved successfully."));
        }

        /// <summary>
        /// Creates a new patient record.
        /// </summary>
        /// <param name="model">Patient data to create.</param>
        /// <response code="201">Patient record created successfully.</response>
        /// <response code="400">Validation failed — invalid patient data.</response>
        /// <response code="401">JWT token is missing or invalid.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] PatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.CreateError("Invalid patient data."));
            }

            var result = await _patientService.AddPatientAsync(model);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.CreateError(result.Message));
            }

            return Created(string.Empty, ApiResponse<object?>.CreateSuccess(null, result.Message));
        }

        /// <summary>
        /// Updates an existing patient record identified by its MongoDB ObjectId.
        /// </summary>
        /// <param name="id">MongoDB ObjectId string of the patient to update.</param>
        /// <param name="model">Updated patient data.</param>
        /// <response code="200">Patient record updated successfully.</response>
        /// <response code="400">Validation failed — invalid patient data.</response>
        /// <response code="401">JWT token is missing or invalid.</response>
        /// <response code="404">No patient found with the given ID.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] PatientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.CreateError("Invalid patient data."));
            }

            var result = await _patientService.UpdatePatientAsync(id, model);
            if (!result.IsSuccess)
            {
                return NotFound(ApiResponse<object>.CreateError(result.Message));
            }

            return Ok(ApiResponse<object?>.CreateSuccess(null, result.Message));
        }
        /// <summary>
        /// Deletes a patient record identified by its MongoDB ObjectId.
        /// </summary>
        /// <param name="id">MongoDB ObjectId string of the patient to delete.</param>
        /// <response code="200">Patient record deleted successfully.</response>
        /// <response code="401">JWT token is missing or invalid.</response>
        /// <response code="404">No patient found with the given ID.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _patientService.DeletePatientAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(ApiResponse<object?>.CreateError(result.Message));
            }

            return Ok(ApiResponse<object?>.CreateSuccess(null, result.Message));
        }

        public class AssignDoctorRequest
        {
            public string DoctorId { get; set; } = string.Empty;
            public DateTime AppointmentDate { get; set; }
            public string AppointmentTime { get; set; } = string.Empty;
        }

        /// <summary>
        /// Assigns a patient to a doctor with an appointment date and time.
        /// </summary>
        [HttpPost("{id}/assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Assign(string id, [FromBody] AssignDoctorRequest request)
        {
            if (string.IsNullOrEmpty(request.DoctorId) || string.IsNullOrEmpty(request.AppointmentTime))
            {
                return BadRequest(ApiResponse<object>.CreateError("Invalid assignment data."));
            }

            var result = await _patientService.AssignPatientAsync(id, request.DoctorId, request.AppointmentDate, request.AppointmentTime);
            if (!result.IsSuccess)
            {
                return NotFound(ApiResponse<object>.CreateError(result.Message));
            }

            return Ok(ApiResponse<object?>.CreateSuccess(null, result.Message));
        }

        /// <summary>
        /// Marks an appointment as completed by the assigned doctor.
        /// </summary>
        [HttpPost("{id}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CompleteAppointment(string id)
        {
            var userRole = User.FindFirstValue(System.Security.Claims.ClaimTypes.Role);
            if (userRole != "Doctor") 
            {
                return Unauthorized(ApiResponse<object>.CreateError("Only doctors can complete appointments."));
            }
            
            var doctorId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) 
                           ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(doctorId)) 
            {
                return Unauthorized(ApiResponse<object>.CreateError("Invalid doctor identity."));
            }

            var result = await _patientService.CompleteAppointmentAsync(id, doctorId);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.CreateError(result.Message));
            }
            
            return Ok(ApiResponse<object>.CreateSuccess(null, result.Message));
        }
    }
}
