using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;

namespace HealthcareCRM.Services
{
    public class DoctorService
    {
        private readonly IDoctorRepository _doctorRepository;

        public DoctorService(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<IEnumerable<Doctor>> GetActiveDoctorsAsync()
        {
            return await _doctorRepository.GetActiveDoctorsAsync();
        }
    }
}
