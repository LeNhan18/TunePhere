using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using System.Security.Claims;

namespace TunePhere.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly AppDbContext _context;

        public PlaylistController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPlaylists()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var playlists = await _context.Playlists
                .Where(p => p.UserId == userId)
                .Select(p => new { id = p.PlaylistId, name = p.User })
                .ToListAsync();

            return Json(playlists);
        }
    }
} 