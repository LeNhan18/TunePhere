using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication.Data;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [Authorize]
    public class PlaylistsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlaylistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Playlists
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Playlists.Include(p => p.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Playlists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlist = await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(m => m.PlaylistId == id);
            
            if (playlist == null)
            {
                return NotFound();
            }

            return View(playlist);
        }

        // GET: Playlists/Create
        public IActionResult Create()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Create", "Playlists") });
            }
            
            // Thử lấy UserId
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                TempData["Error"] = "Không thể xác định người dùng hiện tại. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }
            
            return View();
        }

        // POST: Playlists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,IsPublic")] Playlist playlist)
        {
            try
            {
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null)
                {
                    ModelState.AddModelError("", "Không thể xác định người dùng. Vui lòng đăng nhập lại.");
                    return View(playlist);
                }

                // Create new playlist with user
                var newPlaylist = new Playlist
                {
                    Title = playlist.Title,
                    IsPublic = playlist.IsPublic,
                    UserId = userId,
                    User = user,
                    CreatedAt = DateTime.Now
                };

                if (ModelState.IsValid)
                {
                    // Thêm playlist vào database
                    _context.Playlists.Add(newPlaylist);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                
                // If we got this far, something failed, redisplay form
                return View(playlist);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError(string.Empty, $"Chi tiết: {ex.InnerException.Message}");
                }
            }
            
            return View(playlist);
        }

        // GET: Playlists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null)
            {
                return NotFound();
            }
            
            // Kiểm tra quyền chỉnh sửa (nếu cần)
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (playlist.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            
            return View(playlist);
        }

        // POST: Playlists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlaylistId,UserId,Title,IsPublic,CreatedAt")] Playlist playlist)
        {
            if (id != playlist.PlaylistId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(playlist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlaylistExists(playlist.PlaylistId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            return View(playlist);
        }

        // GET: Playlists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlist = await _context.Playlists
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PlaylistId == id);
            
            if (playlist == null)
            {
                return NotFound();
            }
            
            // Kiểm tra quyền xóa (nếu cần)
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (playlist.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(playlist);
        }

        // POST: Playlists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist != null)
            {
                // Kiểm tra quyền xóa (nếu cần)
                string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (playlist.UserId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }
                
                _context.Playlists.Remove(playlist);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlaylistExists(int id)
        {
            return _context.Playlists.Any(e => e.PlaylistId == id);
        }
    }
} 