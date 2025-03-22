using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IListeningRoomParticipantRepository
    {
        Task<IEnumerable<ListeningRoomParticipant>> GetAllAsync();
        Task<ListeningRoomParticipant?> GetByIdsAsync(int roomId, string userId);
        Task AddAsync(ListeningRoomParticipant participant);
        Task UpdateAsync(ListeningRoomParticipant participant);
        Task DeleteAsync(int roomId, string userId);
    }
}
