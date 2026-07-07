using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;

namespace HealthcareCRM.Services
{
    public class DoctorService
    {
        private readonly IUserRepository _userRepository;

        public DoctorService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetActiveDoctorsAsync()
        {
            return await _userRepository.GetApprovedDoctorsAsync();
        }

        public async Task<(bool IsSuccess, string Message)> UpdateFeeAsync(string doctorId, decimal fee, string currency)
        {
            var doctor = await _userRepository.GetByIdAsync(doctorId);
            if (doctor == null || doctor.Role != "Doctor")
            {
                return (false, "Doctor record not found.");
            }

            doctor.Fee = fee;
            doctor.Currency = currency;

            await _userRepository.UpdateAsync(doctor);
            return (true, "Fee updated successfully.");
        }
    }
}
