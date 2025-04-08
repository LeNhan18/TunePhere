using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly AppDbContext _context;

        public PlaylistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Playlist>> GetAllAsync()
        {
            return await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                .ToListAsync();
        }

        public async Task<Playlist> GetByIdAsync(int id)
        {
            return await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p => p.PlaylistId == id);
        }

        public async Task<Playlist> GetPlaylistByIdAsync(int id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<Playlist> AddAsync(Playlist playlist)
        {
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();
            return playlist;
        }

        public async Task<Playlist> CreatePlaylistAsync(Playlist playlist)
        {
            return await AddAsync(playlist);
        }

        public async Task<Playlist> UpdateAsync(Playlist playlist)
        {
            _context.Entry(playlist).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return playlist;
        }

        public async Task<Playlist> UpdatePlaylistAsync(Playlist playlist)
        {
            return await UpdateAsync(playlist);
        }

        public async Task DeleteAsync(int id)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist != null)
            {
                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePlaylistAsync(int id)
        {
            await DeleteAsync(id);
        }

        public async Task<IEnumerable<Playlist>> GetUserPlaylistsAsync(string userId)
        {
            return await _context.Playlists
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> PlaylistExistsAsync(int id)
        {
            return await _context.Playlists.AnyAsync(p => p.PlaylistId == id);
        }

        public async Task AddSongToPlaylistAsync(int playlistId, int songId)
        {
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.PlaylistId == playlistId);

            if (playlist != null && !playlist.PlaylistSongs.Any(ps => ps.SongId == songId))
            {
                var playlistSong = new PlaylistSong
                {
                    PlaylistId = playlistId,
                    SongId = songId,
                    Order = playlist.PlaylistSongs.Count + 1
                };
                _context.PlaylistSongs.Add(playlistSong);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveSongFromPlaylistAsync(int playlistId, int songId)
        {
            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (playlistSong != null)
            {
                _context.PlaylistSongs.Remove(playlistSong);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ReorderPlaylistSongsAsync(int playlistId, List<int> songIds)
        {
            var playlistSongs = await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId)
                .ToListAsync();

            for (int i = 0; i < songIds.Count; i++)
            {
                var playlistSong = playlistSongs.FirstOrDefault(ps => ps.SongId == songIds[i]);
                if (playlistSong != null)
                {
                    playlistSong.Order = i + 1;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
} 