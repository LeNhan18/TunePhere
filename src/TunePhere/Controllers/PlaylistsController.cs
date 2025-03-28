using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using System.Security.Claims; // Thêm để dùng ClaimTypes
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace TunePhere.Controllers
{
    public class PlaylistsController : Controller
    {
        private readonly AppDbContext _context;

        public PlaylistsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Playlists
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Chỉ lấy playlist của user hiện tại
            var playlists = await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs)
                .Where(p => p.UserId == currentUserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Chỉ cần gán playlist vào ViewBag.MyPlaylists
            ViewBag.MyPlaylists = playlists;

            return View(playlists);
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
                        .ThenInclude(s => s.Artists)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                        .ThenInclude(s => s.Albums)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,IsPublic,UserId")] Playlist playlist, IFormFile? ImageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(playlist);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError("", "Không thể xác định người dùng. Vui lòng đăng nhập lại.");
                    return View(playlist);
                }

                playlist.UserId = userId;
                playlist.CreatedAt = DateTime.Now;

                // Xử lý upload ảnh
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "playlists");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    playlist.ImageUrl = $"/uploads/playlists/{uniqueFileName}";
                }

                _context.Playlists.Add(playlist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError(string.Empty, $"Chi tiết: {ex.InnerException.Message}");
                }
                return View(playlist);
            }
        }

        // GET: Playlists/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

            if (playlist == null)
            {
                return NotFound();
            }

            return View(playlist);
        }

        // POST: Playlists/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlaylistId,Title,IsPublic,ImageUrl")] Playlist playlist, IFormFile? ImageFile = null)
        {
            if (id != playlist.PlaylistId)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingPlaylist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

            if (existingPlaylist == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin cơ bản
                    existingPlaylist.Title = playlist.Title;
                    existingPlaylist.IsPublic = playlist.IsPublic;

                    // Xử lý upload ảnh mới nếu có
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "playlists");
                        Directory.CreateDirectory(uploadsFolder);

                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingPlaylist.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingPlaylist.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        existingPlaylist.ImageUrl = "/uploads/playlists/" + uniqueFileName;
                    }

                    _context.Update(existingPlaylist);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id = playlist.PlaylistId });
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
            }

            return View(playlist);
        }

        // GET: Playlists/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

            if (playlist == null)
            {
                return NotFound();
            }

            return View(playlist);
        }

        // POST: Playlists/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.PlaylistId == id && p.UserId == userId);

            if (playlist == null)
            {
                return NotFound();
            }

            try
            {
                // Xóa ảnh playlist nếu có
                if (!string.IsNullOrEmpty(playlist.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", playlist.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Xóa tất cả bài hát trong playlist
                var playlistSongs = await _context.PlaylistSongs
                    .Where(ps => ps.PlaylistId == id)
                    .ToListAsync();

                _context.PlaylistSongs.RemoveRange(playlistSongs);

                // Xóa playlist
                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi xóa playlist: " + ex.Message);
                return View(playlist);
            }
        }

        // GET: Playlists/Search
        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var query = _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs)
                .Where(p => p.IsPublic || p.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p => 
                    p.Title.ToLower().Contains(searchTerm) || 
                    p.User.UserName.ToLower().Contains(searchTerm)
                );
            }

            var playlists = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            return View("SearchResults", playlists);
        }

        private bool PlaylistExists(int id)
        {
            return _context.Playlists.Any(e => e.PlaylistId == id);
        }
    }
}