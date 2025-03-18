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
            return await _context.ListeningRooms.ToListAsync();
        }

        public async Task<ListeningRoom?> GetByIdAsync(int roomId)
        {
            return await _context.ListeningRooms.FindAsync(roomId);
        }

        public async Task AddAsync(ListeningRoom room)
        {
            _context.ListeningRooms.Add(room);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ListeningRoom room)
        {
            _context.ListeningRooms.Update(room);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int roomId)
        {
            var room = await _context.ListeningRooms.FindAsync(roomId);
            if (room != null)
            {
                _context.ListeningRooms.Remove(room);
                await _context.SaveChangesAsync();
            }
        }
    }
}
