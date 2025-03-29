using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface ISongRepository
    {
        Task<IEnumerable<Song>> GetAllAsync();
        Task<Song?> GetByIdAsync(int songId);
        Task AddAsync(Song song);
        Task UpdateAsync(Song song);
        Task DeleteAsync(int songId);
        Task<IEnumerable<Song>> GetTopSongsAsync();
        
        // Thêm phương thức mới cho chức năng yêu thích
        Task<bool> IsFavoritedByUserAsync(int songId, string userId);
        Task<bool> ToggleFavoriteAsync(int songId, string userId);
        Task<IEnumerable<Song>> GetUserFavoriteSongsAsync(string userId);
    }
}
