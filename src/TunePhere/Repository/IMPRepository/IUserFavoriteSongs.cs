using TunePhere.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TunePhere.Repository.IMPRepository
{
    public interface IUserFavoriteSongs
    {
        Task<IEnumerable<UserFavoriteSong>> GetAllFavoritesAsync();
        Task<IEnumerable<UserFavoriteSong>> GetUserFavoritesAsync(string userId);
        Task<UserFavoriteSong> GetByIdAsync(int id);
        Task AddAsync(UserFavoriteSong userFavoriteSong);
        Task RemoveAsync(int id);
        Task<bool> IsSongFavoritedByUserAsync(int songId, string userId);
    }
}
