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
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initialises PatientService with the patient repository.
        /// </summary>
        public PatientService(IPatientRepository patientRepository, IUserRepository userRepository)
        {
            _patientRepository = patientRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Returns a paginated list of patients, or filters by a search query and doctor ID if provided.
        /// </summary>
        public async Task<PagedResult<Patient>> GetPatientsAsync(string? query, int page, int pageSize, string? doctorId = null)
        {
            var (items, totalCount) = await _patientRepository.GetPagedAsync(query, page, pageSize, doctorId);
            return new PagedResult<Patient>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Returns all patients with an assigned doctor.
        /// </summary>
        public async Task<IEnumerable<Patient>> GetAppointmentsAsync()
        {
            return await _patientRepository.GetAppointmentsAsync();
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
                DateOfBirth = model.DateOfBirth,
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
            patient.DateOfBirth = model.DateOfBirth;
            patient.Gender  = model.Gender;
            patient.Status  = model.Status;
            patient.Phone   = model.Phone;
            patient.Address = model.Address;

            await _patientRepository.UpdateAsync(id, patient);
            return (true, "Patient record updated successfully.");
        }

        /// <summary>
        /// Assigns a patient to a specific doctor with an appointment date and time.
        /// </summary>
        public async Task<(bool IsSuccess, string Message)> AssignPatientAsync(string id, string doctorId, DateTime appointmentDate, string appointmentTime)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
            {
                return (false, "Patient record not found.");
            }

            patient.AssignedDoctorId = doctorId;
            patient.AppointmentDate = appointmentDate;
            patient.AppointmentTime = appointmentTime;
            patient.AppointmentStatus = "Scheduled";

            var doctor = await _userRepository.GetByIdAsync(doctorId);
            if (doctor != null)
            {
                patient.AppointmentFee = doctor.Fee;
                patient.AppointmentCurrency = doctor.Currency;
            }

            await _patientRepository.UpdateAsync(id, patient);
            return (true, "Patient assigned to doctor successfully.");
        }

        /// <summary>
        /// Marks an appointment as completed.
        /// </summary>
        public async Task<(bool IsSuccess, string Message)> CompleteAppointmentAsync(string id, string doctorId)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
            {
                return (false, "Patient record not found.");
            }
            if (patient.AssignedDoctorId != doctorId)
            {
                return (false, "Not authorized to complete this appointment.");
            }

            patient.AppointmentStatus = "Completed";
            await _patientRepository.UpdateAsync(id, patient);
            return (true, "Appointment marked as completed.");
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
