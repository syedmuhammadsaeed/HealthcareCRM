using HealthcareCRM.Models;

namespace HealthcareCRM.Interfaces
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetAllAsync();
        Task<Patient?> GetByIdAsync(int patientId);
        Task<IEnumerable<Patient>> SearchAsync(string query);
        Task AddAsync(Patient patient);
        Task UpdateAsync(Patient patient);
        Task SaveChangesAsync();
    }
}
