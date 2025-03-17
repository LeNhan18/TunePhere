using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IListeningRoomRepository
    {
        Task<IEnumerable<ListeningRoom>> GetAllAsync();
        Task<ListeningRoom> GetByIdAsync(int roomId);
        Task AddAsync(ListeningRoom room);
        Task UpdateAsync(ListeningRoom room);
        Task DeleteAsync(int roomId);
    }
}
