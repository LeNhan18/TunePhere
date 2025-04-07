using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IArtistRepository
    {
        Task<IEnumerable<Artists>> GetAllAsync();
        Task<Artists> GetByIdAsync(int id);
        Task AddAsync(Artists artist);
        Task UpdateAsync(Artists artist);
        Task DeleteAsync(int id);
    }
} 