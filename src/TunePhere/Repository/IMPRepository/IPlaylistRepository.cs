using System.Collections.Generic;
using System.Threading.Tasks;
using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IPlaylistRepository
    {
        Task<IEnumerable<Playlist>> GetAllAsync();
        Task<Playlist> GetByIdAsync(int id);
        Task<Playlist> GetPlaylistByIdAsync(int id);
        Task<Playlist> AddAsync(Playlist playlist);
        Task<Playlist> CreatePlaylistAsync(Playlist playlist);
        Task<Playlist> UpdateAsync(Playlist playlist);
        Task<Playlist> UpdatePlaylistAsync(Playlist playlist);
        Task DeleteAsync(int id);
        Task DeletePlaylistAsync(int id);
        Task<IEnumerable<Playlist>> GetUserPlaylistsAsync(string userId);
        Task<bool> PlaylistExistsAsync(int id);
        Task AddSongToPlaylistAsync(int playlistId, int songId);
        Task RemoveSongFromPlaylistAsync(int playlistId, int songId);
        Task ReorderPlaylistSongsAsync(int playlistId, List<int> songIds);
    }
}
