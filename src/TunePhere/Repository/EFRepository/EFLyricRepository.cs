using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository.EFRepository
{
    public class EFLyricRepository : ILyricRepository
    {
        private readonly AppDbContext _context;

        public EFLyricRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lyric>> GetAllAsync()
        {
            return await _context.Lyrics.ToListAsync();
        }

        public async Task<Lyric?> GetByIdAsync(int lyricId)
        {
            return await _context.Lyrics.FindAsync(lyricId);
        }

        public async Task AddAsync(Lyric lyric)
        {
            _context.Lyrics.Add(lyric);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Lyric lyric)
        {
            _context.Lyrics.Update(lyric);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int lyricId)
        {
            var lyric = await _context.Lyrics.FindAsync(lyricId);
            if (lyric != null)
            {
                _context.Lyrics.Remove(lyric);
                await _context.SaveChangesAsync();
            }
        }
    }
}
