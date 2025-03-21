using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Repository.EFRepository
{
    public class EFPlaylistRepository : IPlaylistRepository
    {
        private readonly AppDbContext _context;

        public EFPlaylistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Playlist>> GetAllAsync()
        {
            return await _context.Playlists.ToListAsync();
        }

        public async Task<Playlist?> GetByIdAsync(int playlistId)
        {
            return await _context.Playlists.FindAsync(playlistId);
        }

        public async Task AddAsync(Playlist playlist)
        {
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Playlist playlist)
        {
            _context.Playlists.Update(playlist);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int playlistId)
        {
            var playlist = await _context.Playlists.FindAsync(playlistId);
            if (playlist != null)
            {
                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();
            }
        }
        // Lấy playlist của người dùng theo username (giả sử Playlist.User có thuộc tính Username)
        public async Task<IEnumerable<Playlist>> GetUserPlaylistsAsync(string username)
        {
            return await _context.Playlists
                .Include(p => p.User)
                .Where(p => p.User.Username == username)
                .ToListAsync();
        }
        // Lấy playlist công khai
        public async Task<IEnumerable<Playlist>> GetPublicPlaylistsAsync()
        {
            return await _context.Playlists
                .Where(p => p.IsPublic)
                .ToListAsync();
        }
        // Gợi ý playlist (ví dụ: sử dụng playlist công khai)
        public async Task<IEnumerable<Playlist>> GetSuggestedPlaylistsAsync()
        {
            return await GetPublicPlaylistsAsync();
        }
    }
}
