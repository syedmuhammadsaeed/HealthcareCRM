using HealthcareCRM.Models;

namespace HealthcareCRM.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int userId);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
