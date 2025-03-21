using TunePhere.Models;

namespace TunePhere.Repository.IMPRepository
{
    public class SearchResult
    {
        public IEnumerable<Song> Songs { get; set; }
        public IEnumerable<Playlist> Playlists { get; set; }
        public IEnumerable<Remix> Remixes { get; set; }
    }
    public interface ISearchRepository
    {
        Task<SearchResult> SearchAsync(string query);
    }
}
