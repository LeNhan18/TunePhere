using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository.EFRepository
{
    public class EFExploreRepository : IExploreRepository
    {
        private readonly AppDbContext _context;
        public EFExploreRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Song>> GetTopSongsAsync()
        {
            return await _context.Songs
                .OrderByDescending(s => s.UploadDate)
                .Take(10)
                .ToListAsync();
        }
        public async Task<IEnumerable<Remix>> GetTopRemixesAsync()
        {
            return await _context.Remixes
                .OrderByDescending(r => r.Likes)
                .Take(10)
                .ToListAsync();
        }
    }
}
