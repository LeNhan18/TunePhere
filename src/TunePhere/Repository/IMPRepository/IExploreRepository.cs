using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public interface IExploreRepository
    {
        Task<IEnumerable<Song>> GetTopSongsAsync();
        Task<IEnumerable<Remix>> GetTopRemixesAsync();
    }
}
