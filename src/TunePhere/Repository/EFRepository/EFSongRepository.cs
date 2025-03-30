using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using TunePhere.Repository.IMPRepository;
using System.Linq;
using System.Collections.Generic;

namespace TunePhere.Repository.EFRepository
{
    public class EFSongRepository : ISongRepository
    {
        private readonly AppDbContext _context;

        public EFSongRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Song>> GetAllAsync()
        {
            return await _context.Songs
                .OrderBy(s => s.Title)
                .ToListAsync();
        }

        public async Task<Song?> GetByIdAsync(int id)
        {
            return await _context.Songs.FindAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Songs.AnyAsync(s => s.SongId == id);
        }

        public async Task AddAsync(Song song)
        {
            song.CreatedAt = DateTime.Now;
            _context.Songs.Add(song);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Song song)
        {
            song.UpdatedAt = DateTime.Now;
            _context.Songs.Update(song);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song != null)
            {
                song.IsActive = false;
                song.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Song>> GetActiveSongsAsync()
        {
            return await _context.Songs
                .Where(s => s.IsActive)
                .OrderBy(s => s.Title)
                .ToListAsync();
        }

        public async Task<bool> IsFavoritedByUserAsync(int songId, string userId)
        {
            return await _context.UserFavoriteSongs
                .AnyAsync(f => f.SongId == songId && f.UserId == userId);
        }

        public async Task<bool> ToggleFavoriteAsync(int songId, string userId)
        {
            var song = await _context.Songs.FindAsync(songId);
            if (song == null)
                return false;

            var favorite = await _context.UserFavoriteSongs
                .FirstOrDefaultAsync(f => f.UserId == userId && f.SongId == songId);

            bool isNowLiked = false;
            
            if (favorite == null)
            {
                // Chưa thích - thêm vào danh sách yêu thích
                _context.UserFavoriteSongs.Add(new UserFavoriteSong
                {
                    UserId = userId,
                    SongId = songId,
                    AddedDate = DateTime.Now
                });
                
                // Tăng LikeCount của bài hát
                song.LikeCount++;
                isNowLiked = true;
            }
            else
            {
                // Đã thích - xóa khỏi danh sách yêu thích
                _context.UserFavoriteSongs.Remove(favorite);
                
                // Giảm LikeCount của bài hát
                if (song.LikeCount > 0)
                    song.LikeCount--;
                isNowLiked = false;
            }
            
            await _context.SaveChangesAsync();
            
            return isNowLiked;
        }

        public async Task<IEnumerable<Song>> GetUserFavoriteSongsAsync(string userId)
        {
            return await _context.UserFavoriteSongs
                .Where(f => f.UserId == userId)
                .Include(f => f.Song)
                .ThenInclude(s => s.Artists)
                .Select(f => f.Song)
                .ToListAsync();
        }

        public async Task<IEnumerable<Song>> SearchSongsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveSongsAsync();

            searchTerm = searchTerm.ToLower();
            return await _context.Songs
                .Include(s => s.Artists)
                .Where(s => s.IsActive &&
                    (s.Title.ToLower().Contains(searchTerm) ||
                     (s.Artists != null && s.Artists.ArtistName.ToLower().Contains(searchTerm))))
                .OrderBy(s => s.Title)
                .ToListAsync();
        }
    }
}
