using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IUserPreferenceRepository
    {
        Task<UserPreference?> GetByUserIdAsync(int userId);
        Task AddOrUpdateAsync(UserPreference preference);
        Task DeleteAsync(int userId);
    }
}
