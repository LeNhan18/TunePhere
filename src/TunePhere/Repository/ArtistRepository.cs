using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository
{
    public class ArtistRepository : IArtistRepository
    {
        private readonly AppDbContext _context;

        public ArtistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Artists>> GetAllAsync()
        {
            return await _context.Artists
                .Include(a => a.Songs)
                .Include(a => a.Followers)
                .ToListAsync();
        }

        public async Task<Artists> GetByIdAsync(int id)
        {
            return await _context.Artists
                .Include(a => a.Songs)
                .Include(a => a.Followers)
                .FirstOrDefaultAsync(a => a.ArtistId == id);
        }

        public async Task AddAsync(Artists artist)
        {
            await _context.Artists.AddAsync(artist);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Artists artist)
        {
            _context.Artists.Update(artist);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist != null)
            {
                _context.Artists.Remove(artist);
                await _context.SaveChangesAsync();
            }
        }
    }
} 