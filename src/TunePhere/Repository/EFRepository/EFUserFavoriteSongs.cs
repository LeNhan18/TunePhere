using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository.EFRepository
{
    public class EFUserFavoriteSongs : IUserFavoriteSongs
    {
        private readonly AppDbContext _context;

        public EFUserFavoriteSongs(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserFavoriteSong>> GetAllFavoritesAsync()
        {
            return await _context.UserFavoriteSongs
                .Include(u => u.Song)
                .Include(u => u.Song.Artists)
                .Include(u => u.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserFavoriteSong>> GetUserFavoritesAsync(string userId)
        {
            return await _context.UserFavoriteSongs
                .Include(u => u.Song)
                .Include(u => u.Song.Artists)
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.AddedDate)
                .ToListAsync();
        }

        public async Task<UserFavoriteSong> GetByIdAsync(int id)
        {
            return await _context.UserFavoriteSongs
                .Include(u => u.Song)
                .Include(u => u.Song.Artists)
                .Include(u => u.User)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task AddAsync(UserFavoriteSong userFavoriteSong)
        {
            _context.UserFavoriteSongs.Add(userFavoriteSong);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(int id)
        {
            var favorite = await _context.UserFavoriteSongs.FindAsync(id);
            if (favorite != null)
            {
                _context.UserFavoriteSongs.Remove(favorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsSongFavoritedByUserAsync(int songId, string userId)
        {
            return await _context.UserFavoriteSongs
                .AnyAsync(u => u.SongId == songId && u.UserId == userId);
        }
    }
}
