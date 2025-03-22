using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IUserRepository
    {
        Task<IEnumerable<AppUser>> GetAllAsync();
        Task<AppUser?> GetByIdAsync(string Id);
        Task<AppUser?> GetByUsernameAsync(string username);
        Task<AppUser?> AuthenticateAsync(string email, string password);
        Task AddAsync(AppUser user);
        Task UpdateAsync(AppUser user);
        Task DeleteAsync(string Id);
    }
}
