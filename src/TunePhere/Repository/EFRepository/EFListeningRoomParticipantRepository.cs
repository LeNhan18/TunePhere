using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository.EFRepository
{
    public class EFListeningRoomParticipantRepository : IListeningRoomParticipantRepository
    {
        private readonly AppDbContext _context;

        public EFListeningRoomParticipantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ListeningRoomParticipant>> GetAllAsync()
        {
            return await _context.ListeningRoomParticipants.ToListAsync();
        }

        public async Task<ListeningRoomParticipant> GetByIdsAsync(int roomId, int userId)
        {
            return await _context.ListeningRoomParticipants
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.UserId == userId);
        }

        public async Task AddAsync(ListeningRoomParticipant participant)
        {
            _context.ListeningRoomParticipants.Add(participant);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ListeningRoomParticipant participant)
        {
            _context.ListeningRoomParticipants.Update(participant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int roomId, int userId)
        {
            var participant = await _context.ListeningRoomParticipants
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.UserId == userId);
            if (participant != null)
            {
                _context.ListeningRoomParticipants.Remove(participant);
                await _context.SaveChangesAsync();
            }
        }
    }
}
