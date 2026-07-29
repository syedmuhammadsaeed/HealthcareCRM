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

        public async Task<(bool IsSuccess, string Message)> UpdateProfileAsync(string doctorId, string name, string email, string phone, string spec, string qual, string base64Image)
        {
            var doctor = await _userRepository.GetByIdAsync(doctorId);
            if (doctor == null || doctor.Role != "Doctor") return (false, "Doctor record not found.");

            doctor.Name = name;
            doctor.Email = email;
            doctor.Phone = phone;
            doctor.Specialization = spec;
            doctor.Qualification = qual;

            if (!string.IsNullOrEmpty(base64Image))
            {
                doctor.ProfilePictureUrl = base64Image;
            }

            await _userRepository.UpdateAsync(doctor);
            return (true, "Profile updated successfully.");
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
