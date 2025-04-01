using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IListeningRoomParticipantRepository
    {
        Task<IEnumerable<ListeningRoomParticipant>> GetAllAsync();
        Task<ListeningRoomParticipant?> GetByIdAsync(int id);
        Task<ListeningRoomParticipant?> GetByIdsAsync(int roomId, string userId);
        Task<ListeningRoomParticipant?> GetParticipantAsync(int roomId, string userId);
        Task<bool> ExistsAsync(int id);
        Task AddAsync(ListeningRoomParticipant participant);
        Task UpdateAsync(ListeningRoomParticipant participant);
        Task DeleteAsync(ListeningRoomParticipant participant);
    }
}
