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
            return await _context.ListeningRoomParticipants
                .Include(p => p.Room)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<ListeningRoomParticipant?> GetByIdAsync(int id)
        {
            return await _context.ListeningRoomParticipants
                .Include(p => p.Room)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<ListeningRoomParticipant?> GetByIdsAsync(int roomId, string userId)
        {
            return await _context.ListeningRoomParticipants
                .Include(p => p.Room)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.UserId == userId);
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ListeningRoomParticipants.AnyAsync(p => p.Id == id);
        }
        public async Task AddAsync(ListeningRoomParticipant participant)
        {
            participant.JoinedAt = DateTime.Now;
            _context.ListeningRoomParticipants.Add(participant);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ListeningRoomParticipant participant)
        {
            _context.ListeningRoomParticipants.Update(participant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int roomId, string userId)
        {
            var participant = await _context.ListeningRoomParticipants
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.UserId == userId);
            
            if (participant != null)
            {
                _context.ListeningRoomParticipants.Remove(participant);
                await _context.SaveChangesAsync();
            }
        }

        public Task<ListeningRoomParticipant?> GetParticipantAsync(int roomId, string userId)
        {
            throw new NotImplementedException();
        }
        public Task DeleteAsync(ListeningRoomParticipant participant)
        {

            var participants =  _context.ListeningRoomParticipants
               .FirstOrDefaultAsync(p => p.RoomId == participant.RoomId && p.UserId == participant.UserId);

            if (participant != null)
            {
                _context.ListeningRoomParticipants.Remove(participant);
              
            }
            return _context.SaveChangesAsync();
        }
    }
}
