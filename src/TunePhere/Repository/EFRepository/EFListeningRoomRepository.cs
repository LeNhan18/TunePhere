using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository.EFRepository
{
    public class EFListeningRoomRepository : IListeningRoomRepository
    {
        private readonly AppDbContext _context;

        public EFListeningRoomRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ListeningRoom>> GetAllAsync()
        {
            return await _context.ListeningRooms
                .Include(r => r.Creator)
                .Include(r => r.CurrentSong)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<ListeningRoom?> GetByIdAsync(int id)
        {
            return await _context.ListeningRooms
                .Include(r => r.Creator)
                .Include(r => r.CurrentSong)
                .Include(r => r.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.RoomId == id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ListeningRooms.AnyAsync(r => r.RoomId == id);
        }

        public async Task AddAsync(ListeningRoom room)
        {
            _context.ListeningRooms.Add(room);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ListeningRoom room)
        {
            var existingRoom = await _context.ListeningRooms.FindAsync(room.RoomId);
            if (existingRoom != null)
            {
                _context.Entry(existingRoom).CurrentValues.SetValues(room);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var room = await _context.ListeningRooms.FindAsync(id);
            if (room != null)
            {
                room.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        // Lấy các phòng nghe nhạc đang hoạt động
        public async Task<IEnumerable<ListeningRoom>> GetActiveRoomsAsync()
        {
            return await _context.ListeningRooms.Where(r => r.IsActive).ToListAsync();
        }
    }
}
