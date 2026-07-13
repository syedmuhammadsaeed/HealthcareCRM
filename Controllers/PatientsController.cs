using HealthcareCRM.Helpers;
using HealthcareCRM.Services;
using HealthcareCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        /// Retrieves all patient records, with optional case-insensitive search.
        /// </summary>
        /// <param name="search">Optional search term matched against name, phone, and address.</param>
        /// <response code="200">Returns the list of patients.</response>
        /// <response code="401">JWT token is missing or invalid.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get([FromQuery] string? search)
        {
            var patients = await _patientService.GetPatientsAsync(search);
            return Ok(ApiResponse<object>.CreateSuccess(patients, "Patients retrieved successfully."));
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
    }
}
