using HealthcareCRM.Helpers;
using HealthcareCRM.Services;
using HealthcareCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly PatientService _patientService;

        public PatientsController(PatientService patientService)
        {
            _patientService = patientService;
        }

        /// <summary>
        /// Retrieves patient records with optional search.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? search)
        {
            var patients = await _patientService.GetPatientsAsync(search);
            return Ok(ApiResponse<object>.CreateSuccess(patients, "Patients retrieved successfully."));
        }

        /// <summary>
        /// Creates a new patient record.
        /// </summary>
        [HttpPost]
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

            return Created(string.Empty, ApiResponse<object>.CreateSuccess(null, result.Message));
        }

        /// <summary>
        /// Updates an existing patient record.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] PatientViewModel model)
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

            return Ok(ApiResponse<object>.CreateSuccess(null, result.Message));
        }
    }
}
