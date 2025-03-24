using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository.EFRepository
{
    public class EFSearchRepository
    {
        private readonly AppDbContext _context;
        public EFSearchRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<SearchResult> SearchAsync(string query)
        {
            var songs = await _context.Songs
                .Where(s => s.Title.Contains(query) || s.Artist.ArtistName.Contains(query))
                .ToListAsync();
            var playlists = await _context.Playlists
                .Where(p => p.Title.Contains(query))
                .ToListAsync();
            var remixes = await _context.Remixes
                .Where(r => r.Title.Contains(query))
                .ToListAsync();
            return new SearchResult { Songs = songs, Playlists = playlists, Remixes = remixes };
        }
    }
}
