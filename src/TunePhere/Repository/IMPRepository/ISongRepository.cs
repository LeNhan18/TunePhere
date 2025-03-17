using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface ISongRepository
    {
        Task<IEnumerable<Song>> GetAllAsync();
        Task<Song> GetByIdAsync(int songId);
        Task AddAsync(Song song);
        Task UpdateAsync(Song song);
        Task DeleteAsync(int songId);
    }
}
