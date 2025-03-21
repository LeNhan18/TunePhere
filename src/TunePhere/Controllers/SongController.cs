using Microsoft.AspNetCore.Mvc;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    public class SongController : Controller
    {
        private readonly ISongRepository _songRepo;
        private readonly ILyricRepository _lyricRepo;

        public SongController(ISongRepository songRepo, ILyricRepository lyricRepo)
        {
            _songRepo = songRepo;
            _lyricRepo = lyricRepo;
        }

        public async Task<IActionResult> Details(int id)
        {
            var song = await _songRepo.GetByIdAsync(id);
            if (song == null) return NotFound();

            var lyrics = await _lyricRepo.GetBySongIdAsync(id);
            ViewBag.Lyrics = lyrics;
            return View(song);
        }
    }
}
