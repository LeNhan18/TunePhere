using Microsoft.AspNetCore.Mvc;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    public class ExploreController : Controller
    {
        private readonly IExploreRepository _exploreRepo;

        public ExploreController(IExploreRepository exploreRepo)
        {
            _exploreRepo = exploreRepo;
        }

        public async Task<IActionResult> Index()
        {
            var topSongs = await _exploreRepo.GetTopSongsAsync();
            var topRemixes = await _exploreRepo.GetTopRemixesAsync();
            return View(new { TopSongs = topSongs, TopRemixes = topRemixes });
        }
    }
}
