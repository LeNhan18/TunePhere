using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TunePhere.Models;

namespace TunePhere.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public ProfileController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Profile
        [Authorize]
        public async Task<IActionResult> Index(string? username)
        {
            AppUser user;
            if (string.IsNullOrEmpty(username))
            {
                // Nếu không có username, hiển thị profile của user đang đăng nhập
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                user = await _userManager.Users
                    .Include(u => u.Playlists)
                        .ThenInclude(p => p.PlaylistSongs)
                    .Include(u => u.Followers)
                    .Include(u => u.Following)
                    .FirstOrDefaultAsync(u => u.Id == currentUserId);
            }
            else
            {
                // Hiển thị profile của user được chỉ định
                user = await _userManager.Users
                    .Include(u => u.Playlists)
                        .ThenInclude(p => p.PlaylistSongs)
                    .Include(u => u.Followers)
                    .Include(u => u.Following)
                    .FirstOrDefaultAsync(u => u.UserName == username);
            }

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}
