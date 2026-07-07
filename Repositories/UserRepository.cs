using HealthcareCRM.Data;
using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;
using MongoDB.Driver;

namespace HealthcareCRM.Repositories
{
    /// <summary>
    /// MongoDB implementation of <see cref="IUserRepository"/>.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        /// <summary>
        /// Resolves the Users collection from the shared <see cref="MongoDbContext"/>.
        /// </summary>
        public UserRepository(MongoDbContext context)
        {
            _users = context.Users;
        }

        /// <inheritdoc/>
        public async Task AddAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        /// <inheritdoc/>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _users
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<User?> GetByIdAsync(string userId)
        {
            return await _users
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(User user)
        {
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        public async Task<IEnumerable<User>> GetAllPendingAsync()
        {
            return await _users
                .Find(u => u.Status == "Pending")
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetApprovedDoctorsAsync()
        {
            return await _users
                .Find(u => u.Role == "Doctor" && (u.Status == "Approved" || string.IsNullOrEmpty(u.Status)))
                .ToListAsync();
        }
    }
}
