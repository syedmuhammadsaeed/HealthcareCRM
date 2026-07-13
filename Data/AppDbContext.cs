using HealthcareCRM.Helpers;
using HealthcareCRM.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HealthcareCRM.Data
{
    /// <summary>
    /// MongoDB database context that provides typed collection accessors.
    /// Registered as a singleton — MongoClient is thread-safe by design.
    /// </summary>
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        /// <summary>
        /// Initialises the MongoClient and resolves the target database.
        /// </summary>
        /// <param name="settings">Bound MongoDbSettings configuration.</param>
        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        /// <summary>Gets the Users collection.</summary>
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        /// <summary>Gets the Patients collection.</summary>
        public IMongoCollection<Patient> Patients => _database.GetCollection<Patient>("Patients");

        /// <summary>Gets the Doctors collection.</summary>
        public IMongoCollection<Doctor> Doctors => _database.GetCollection<Doctor>("Doctors");
    }
}
