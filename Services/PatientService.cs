using HealthcareCRM.Helpers;
using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;
using HealthcareCRM.ViewModels;

namespace HealthcareCRM.Services
{
    public class PatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<Patient>> GetPatientsAsync(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await _patientRepository.GetAllAsync();
            }

            return await _patientRepository.SearchAsync(query);
        }

        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            return await _patientRepository.GetByIdAsync(id);
        }

        public async Task<(bool IsSuccess, string Message)> AddPatientAsync(PatientViewModel model)
        {
            var patient = new Patient
            {
                Name = model.Name,
                Age = model.Age,
                Gender = model.Gender,
                Phone = model.Phone,
                Address = model.Address,
                CreatedDate = DateTime.UtcNow
            };

            await _patientRepository.AddAsync(patient);
            await _patientRepository.SaveChangesAsync();
            return (true, "Patient record created successfully.");
        }

        public async Task<(bool IsSuccess, string Message)> UpdatePatientAsync(int id, PatientViewModel model)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
            {
                return (false, "Patient record not found.");
            }

            patient.Name = model.Name;
            patient.Age = model.Age;
            patient.Gender = model.Gender;
            patient.Phone = model.Phone;
            patient.Address = model.Address;

            await _patientRepository.UpdateAsync(patient);
            await _patientRepository.SaveChangesAsync();
            return (true, "Patient record updated successfully.");
        }
    }
}
