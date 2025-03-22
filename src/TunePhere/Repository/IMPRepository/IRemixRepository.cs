using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IRemixRepository
    {
        Task<IEnumerable<Remix>> GetAllAsync();
        Task<Remix?> GetByIdAsync(int remixId);
        Task AddAsync(Remix remix);
        Task UpdateAsync(Remix remix);
        Task DeleteAsync(int remixId);
        // Phương thức bổ sung
        Task<IEnumerable<Remix>> GetTopRemixesAsync();
        Task<IEnumerable<Remix>> GetUserRemixesAsync(string username);
    }
}
