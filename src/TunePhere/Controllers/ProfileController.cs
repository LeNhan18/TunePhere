using Microsoft.AspNetCore.Mvc;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserRepository _userRepo;

        public ProfileController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userRepo.GetByUsernameAsync(User.Identity.Name);
            return View(user);
        }
    }
}
