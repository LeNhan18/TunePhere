using Microsoft.AspNetCore.Mvc;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    public class RemixController : Controller
    {
        private readonly IRemixRepository _remixRepo;

        public RemixController(IRemixRepository remixRepo)
        {
            _remixRepo = remixRepo;
        }

        public async Task<IActionResult> Index()
        {
            var remixes = await _remixRepo.GetUserRemixesAsync(User.Identity.Name);
            return View(remixes);
        }
    }
}
