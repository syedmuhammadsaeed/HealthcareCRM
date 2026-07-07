using HealthcareCRM.Models;

namespace HealthcareCRM.Interfaces
{
    /// <summary>
    /// Repository contract for User data access against MongoDB.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>Retrieves a user by email address.</summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>Retrieves a user by their MongoDB ObjectId string.</summary>
        Task<User?> GetByIdAsync(string userId);

        /// <summary>Inserts a new user document into the Users collection.</summary>
        Task AddAsync(User user);

        /// <summary>Updates an existing user document.</summary>
        Task UpdateAsync(User user);

        /// <summary>Retrieves all users with a Pending status.</summary>
        Task<IEnumerable<User>> GetAllPendingAsync();

        /// <summary>Retrieves all approved doctors.</summary>
        Task<IEnumerable<User>> GetApprovedDoctorsAsync();
    }
}
