using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int userId);
        Task<User> GetByUsernameAsync(string username);
        Task<User> AuthenticateAsync(string email, string password);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int userId);
    }
}
