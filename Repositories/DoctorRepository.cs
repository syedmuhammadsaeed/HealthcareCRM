using HealthcareCRM.Data;
using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;
using MongoDB.Driver;

namespace HealthcareCRM.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly IMongoCollection<Doctor> _doctors;

        public DoctorRepository(MongoDbContext context)
        {
            _doctors = context.Doctors;
        }

        public async Task<IEnumerable<Doctor>> GetActiveDoctorsAsync()
        {
            return await _doctors.Find(d => d.IsActive).ToListAsync();
        }
    }
}
