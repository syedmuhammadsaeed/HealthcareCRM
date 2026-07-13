using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;
using HealthcareCRM.ViewModels;

namespace HealthcareCRM.Services
{
    /// <summary>
    /// Business logic for patient record management.
    /// </summary>
    public class PatientService
    {
        private readonly IPatientRepository _patientRepository;

        /// <summary>
        /// Initialises PatientService with the patient repository.
        /// </summary>
        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        /// <summary>
        /// Returns all patients, or filters by a search query if provided.
        /// </summary>
        public async Task<IEnumerable<Patient>> GetPatientsAsync(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await _patientRepository.GetAllAsync();
            }

            return await _patientRepository.SearchAsync(query);
        }

        /// <summary>
        /// Returns a single patient by their MongoDB ObjectId string.
        /// </summary>
        public async Task<Patient?> GetPatientByIdAsync(string id)
        {
            return await _patientRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Creates a new patient record from the provided view model.
        /// </summary>
        public async Task<(bool IsSuccess, string Message)> AddPatientAsync(PatientViewModel model)
        {
            var patient = new Patient
            {
                Name        = model.Name,
                Age         = model.Age,
                Gender      = model.Gender,
                Status      = model.Status,
                Phone       = model.Phone,
                Address     = model.Address,
                CreatedDate = DateTime.UtcNow
            };

            await _patientRepository.AddAsync(patient);
            return (true, "Patient record created successfully.");
        }

        /// <summary>
        /// Updates an existing patient record identified by ObjectId string.
        /// </summary>
        public async Task<(bool IsSuccess, string Message)> UpdatePatientAsync(string id, PatientViewModel model)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
            {
                return (false, "Patient record not found.");
            }

            patient.Name    = model.Name;
            patient.Age     = model.Age;
            patient.Gender  = model.Gender;
            patient.Status  = model.Status;
            patient.Phone   = model.Phone;
            patient.Address = model.Address;

            await _patientRepository.UpdateAsync(id, patient);
            return (true, "Patient record updated successfully.");
        }
        /// <summary>
        /// Deletes a patient record identified by ObjectId string.
        /// </summary>
        public async Task<(bool IsSuccess, string Message)> DeletePatientAsync(string id)
        {
            var deleted = await _patientRepository.DeleteAsync(id);
            return deleted
                ? (true,  "Patient record deleted successfully.")
                : (false, "Patient record not found.");
        }
    }
}
