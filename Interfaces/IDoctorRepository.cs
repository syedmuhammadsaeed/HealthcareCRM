using HealthcareCRM.Models;

namespace HealthcareCRM.Interfaces
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<Doctor>> GetActiveDoctorsAsync();
    }
}
