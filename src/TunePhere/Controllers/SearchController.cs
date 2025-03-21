using Microsoft.AspNetCore.Mvc;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchRepository _searchRepo;

        public SearchController(ISearchRepository searchRepo)
        {
            _searchRepo = searchRepo;
        }

        public async Task<IActionResult> Index(string query)
        {
            var results = await _searchRepo.SearchAsync(query);
            return View(results);
        }
    }
}
