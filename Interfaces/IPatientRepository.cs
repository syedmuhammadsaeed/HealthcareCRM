using HealthcareCRM.Models;

namespace HealthcareCRM.Interfaces
{
    /// <summary>
    /// Repository contract for Patient data access against MongoDB.
    /// </summary>
    public interface IPatientRepository
    {
        /// <summary>Retrieves all patient records ordered by creation date descending.</summary>
        Task<IEnumerable<Patient>> GetAllAsync();

        /// <summary>Retrieves a patient by their MongoDB ObjectId string.</summary>
        Task<Patient?> GetByIdAsync(string patientId);

        /// <summary>Searches patients by name, phone, or address using a case-insensitive regex.</summary>
        Task<IEnumerable<Patient>> SearchAsync(string query);

        /// <summary>Inserts a new patient document into the Patients collection.</summary>
        Task AddAsync(Patient patient);

        /// <summary>Updates an existing patient document identified by its ObjectId string.</summary>
        Task UpdateAsync(string id, Patient patient);

        /// <summary>Deletes a patient document identified by its ObjectId string.</summary>
        Task<bool> DeleteAsync(string id);
    }
}
