using Microsoft.AspNetCore.Mvc;
using TunePhere.Repository;
using System.Threading.Tasks;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISongRepository _songRepo;
        private readonly IPlaylistRepository _playlistRepo;
        private readonly IRemixRepository _remixRepo;

        public HomeController(ISongRepository songRepo, IPlaylistRepository playlistRepo, IRemixRepository remixRepo)
        {
            _songRepo = songRepo;
            _playlistRepo = playlistRepo;
            _remixRepo = remixRepo;
        }

        public async Task<IActionResult> Index()
        {
            var topSongs = await _songRepo.GetTopSongsAsync();
            var suggestedPlaylists = await _playlistRepo.GetSuggestedPlaylistsAsync();
            var trendingRemixes = await _remixRepo.GetTopRemixesAsync();

            ViewBag.SuggestedPlaylists = suggestedPlaylists;
            ViewBag.TrendingRemixes = trendingRemixes;
            return View(topSongs);
        }
    }
}
