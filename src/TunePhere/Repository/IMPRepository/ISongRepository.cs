using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface ISongRepository
    {
        Task<IEnumerable<Song>> GetAllAsync();
        Task<Song?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task AddAsync(Song song);
        Task UpdateAsync(Song song);
        Task DeleteAsync(int id);
        Task<IEnumerable<Song>> GetActiveSongsAsync();
        
        // Thêm phương thức mới cho chức năng yêu thích
        Task<bool> IsFavoritedByUserAsync(int songId, string userId);
        Task<bool> ToggleFavoriteAsync(int songId, string userId);
        Task<IEnumerable<Song>> GetUserFavoriteSongsAsync(string userId);

        // Thêm phương thức tìm kiếm
        Task<IEnumerable<Song>> SearchSongsAsync(string searchTerm);
    }
}
