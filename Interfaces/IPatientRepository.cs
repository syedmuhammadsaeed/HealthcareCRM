using HealthcareCRM.Models;

namespace HealthcareCRM.Interfaces
{
    /// <summary>
    /// Repository contract for Patient data access against MongoDB.
    /// </summary>
    public interface IPatientRepository
    {
        /// <summary>Retrieves a paginated list of patients, optionally filtered by a search query and doctor ID.</summary>
        Task<(IEnumerable<Patient> Items, int TotalCount)> GetPagedAsync(string? query, int page, int pageSize, string? doctorId = null);

        /// <summary>Retrieves a patient by their MongoDB ObjectId string.</summary>
        Task<Patient?> GetByIdAsync(string patientId);

        /// <summary>Inserts a new patient document into the Patients collection.</summary>
        Task AddAsync(Patient patient);

        /// <summary>Updates an existing patient document identified by its ObjectId string.</summary>
        Task UpdateAsync(string id, Patient patient);

        /// <summary>Deletes a patient document identified by its ObjectId string.</summary>
        Task<bool> DeleteAsync(string id);

        /// <summary>Retrieves all patients that have an assigned doctor (appointments).</summary>
        Task<IEnumerable<Patient>> GetAppointmentsAsync();
    }
}
