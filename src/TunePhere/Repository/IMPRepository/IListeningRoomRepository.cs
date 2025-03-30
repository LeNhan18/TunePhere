using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IListeningRoomRepository
    {
        Task<IEnumerable<ListeningRoom>> GetAllAsync();
        Task<ListeningRoom?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task AddAsync(ListeningRoom room);
        Task UpdateAsync(ListeningRoom room);
        Task DeleteAsync(int id);
        Task<IEnumerable<ListeningRoom>> GetActiveRoomsAsync();
    }
}
