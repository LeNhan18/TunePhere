using Microsoft.AspNetCore.Mvc;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly IPlaylistRepository _playlistRepo;

        public PlaylistController(IPlaylistRepository playlistRepo)
        {
            _playlistRepo = playlistRepo;
        }
        public async Task<IActionResult> Index()
        {
            var personalPlaylists = await _playlistRepo.GetUserPlaylistsAsync(User.Identity.Name);
            var publicPlaylists = await _playlistRepo.GetPublicPlaylistsAsync();
            ViewBag.PublicPlaylists = publicPlaylists;
            return View(personalPlaylists);
        }
    }
}
