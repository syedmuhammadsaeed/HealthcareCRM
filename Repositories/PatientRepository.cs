using HealthcareCRM.Data;
using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HealthcareCRM.Repositories
{
    /// <summary>
    /// MongoDB implementation of <see cref="IPatientRepository"/>.
    /// </summary>
    public class PatientRepository : IPatientRepository
    {
        private readonly IMongoCollection<Patient> _patients;

        /// <summary>
        /// Resolves the Patients collection from the shared <see cref="MongoDbContext"/>.
        /// </summary>
        public PatientRepository(MongoDbContext context)
        {
            _patients = context.Patients;
        }

        /// <inheritdoc/>
        public async Task AddAsync(Patient patient)
        {
            await _patients.InsertOneAsync(patient);
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<Patient> Items, int TotalCount)> GetPagedAsync(string? query, int page, int pageSize)
        {
            FilterDefinition<Patient> filter = Builders<Patient>.Filter.Empty;
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                var regexPattern = new BsonRegularExpression(query, "i"); // case-insensitive
                filter = Builders<Patient>.Filter.Or(
                    Builders<Patient>.Filter.Regex(p => p.Name,    regexPattern),
                    Builders<Patient>.Filter.Regex(p => p.Phone,   regexPattern),
                    Builders<Patient>.Filter.Regex(p => p.Address, regexPattern)
                );
            }

            var totalCount = await _patients.CountDocumentsAsync(filter);
            
            var items = await _patients
                .Find(filter)
                .SortByDescending(p => p.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        /// <inheritdoc/>
        public async Task<Patient?> GetByIdAsync(string patientId)
        {
            return await _patients
                .Find(p => p.Id == patientId)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(string id, Patient patient)
        {
            var update = Builders<Patient>.Update
                .Set(p => p.Name,    patient.Name)
                .Set(p => p.DateOfBirth, patient.DateOfBirth)
                .Set(p => p.Gender,  patient.Gender)
                .Set(p => p.Status,  patient.Status)
                .Set(p => p.Phone,   patient.Phone)
                .Set(p => p.Address, patient.Address);

            await _patients.UpdateOneAsync(p => p.Id == id, update);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _patients.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
