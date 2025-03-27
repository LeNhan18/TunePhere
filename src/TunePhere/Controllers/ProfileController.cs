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
 
        [AllowAnonymous]
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
                        .ThenInclude(f => f.Follower)
                    .Include(u => u.Following)
                        .ThenInclude(f => f.Following)
                    .FirstOrDefaultAsync(u => u.Id == currentUserId);
            }
            else
            {
                // Hiển thị profile của user được chỉ định
                user = await _userManager.Users
                    .Include(u => u.Playlists)
                        .ThenInclude(p => p.PlaylistSongs)
                    .Include(u => u.Followers)
                        .ThenInclude(f => f.Follower)
                    .Include(u => u.Following)
                        .ThenInclude(f => f.Following)
                    .FirstOrDefaultAsync(u => u.UserName == username);
            }

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Profile/ToggleFollow
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleFollow(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Không cho phép follow chính mình
            if (currentUserId == userId)
            {
                return Json(new { success = false, message = "Không thể theo dõi chính mình" });
            }

            // Kiểm tra xem đã follow chưa
            var existingFollow = await _context.UserFollowers
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == userId);

            if (existingFollow != null)
            {
                // Unfollow
                _context.UserFollowers.Remove(existingFollow);
            }
            else
            {
                // Follow
                var newFollow = new UserFollower
                {
                    FollowerId = currentUserId,
                    FollowingId = userId,
                    FollowedAt = DateTime.Now
                };
                _context.UserFollowers.Add(newFollow);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
