using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IPlaylistSongRepository
    {
        Task<IEnumerable<PlaylistSong>> GetAllAsync();
        Task<PlaylistSong?> GetByIdsAsync(int playlistId, int songId);
        Task AddAsync(PlaylistSong playlistSong);
        Task UpdateAsync(PlaylistSong playlistSong);
        Task DeleteAsync(int playlistId, int songId);
    }
}
