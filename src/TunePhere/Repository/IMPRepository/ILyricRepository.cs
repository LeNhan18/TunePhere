using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface ILyricRepository
    {
        Task<IEnumerable<Lyric>> GetAllAsync();
        Task<Lyric?> GetByIdAsync(int lyricId);
        Task<IEnumerable<Lyric>> GetBySongIdAsync(int songId);
        Task AddAsync(Lyric lyric);
        Task UpdateAsync(Lyric lyric);
        Task DeleteAsync(int lyricId);
    }
}
