using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TunePhere.Repository.IMPRepository;
using TunePhere.Models;

namespace TunePhere.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PlaylistManagementController : Controller
    {
        private readonly IPlaylistRepository _playlistRepository;

        public PlaylistManagementController(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }

        public async Task<IActionResult> Index()
        {
            var playlists = await _playlistRepository.GetAllAsync();
            return View(playlists);
        }

        public async Task<IActionResult> Details(int id)
        {
            var playlist = await _playlistRepository.GetByIdAsync(id);
            if (playlist == null)
            {
                return NotFound();
            }
            return View(playlist);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _playlistRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}