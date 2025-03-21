using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IPlaylistRepository
    {
        Task<IEnumerable<Playlist>> GetAllAsync();
        Task<Playlist?> GetByIdAsync(int playlistId);
        Task AddAsync(Playlist playlist);
        Task UpdateAsync(Playlist playlist);
        Task DeleteAsync(int playlistId);
        Task<IEnumerable<Playlist>> GetUserPlaylistsAsync(string username);
        Task<IEnumerable<Playlist>> GetPublicPlaylistsAsync();
        Task<IEnumerable<Playlist>> GetSuggestedPlaylistsAsync();
    }
}
