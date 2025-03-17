using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository.EFRepository
{
    public class EFPlaylistSongRepository : IPlaylistSongRepository
    {
        private readonly AppDbContext _context;

        public EFPlaylistSongRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PlaylistSong>> GetAllAsync()
        {
            return await _context.PlaylistSongs.ToListAsync();
        }

        public async Task<PlaylistSong> GetByIdsAsync(int playlistId, int songId)
        {
            return await _context.PlaylistSongs.FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
        }

        public async Task AddAsync(PlaylistSong playlistSong)
        {
            _context.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PlaylistSong playlistSong)
        {
            _context.PlaylistSongs.Update(playlistSong);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int playlistId, int songId)
        {
            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (playlistSong != null)
            {
                _context.PlaylistSongs.Remove(playlistSong);
                await _context.SaveChangesAsync();
            }
        }
    }
}
