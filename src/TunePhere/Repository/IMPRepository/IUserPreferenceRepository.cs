using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IUserPreferenceRepository
    {
        Task<UserPreference?> GetByUserIdAsync(string userId);
        Task AddOrUpdateAsync(UserPreference preference);
        Task DeleteAsync(string userId);
    }
}
