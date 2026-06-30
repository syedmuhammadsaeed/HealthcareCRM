using HealthcareCRM.Data;
using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthcareCRM.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext _dbContext;

        public PatientRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Patient patient)
        {
            await _dbContext.Patients.AddAsync(patient);
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            return await _dbContext.Patients.OrderByDescending(p => p.CreatedDate).ToListAsync();
        }

        public async Task<Patient?> GetByIdAsync(int patientId)
        {
            return await _dbContext.Patients.FindAsync(patientId);
        }

        public async Task<IEnumerable<Patient>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllAsync();
            }

            query = query.ToLowerInvariant();
            return await _dbContext.Patients
                .Where(p => p.Name.ToLower().Contains(query)
                            || p.Phone.ToLower().Contains(query)
                            || p.Address.ToLower().Contains(query))
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task UpdateAsync(Patient patient)
        {
            _dbContext.Patients.Update(patient);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
