using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using TunePhere.Repository.IMPRepository;

namespace TunePhere.Controllers
{
    public class PlaylistController : Controller
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PlaylistController(
            IPlaylistRepository playlistRepository,
            UserManager<AppUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _playlistRepository = playlistRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Playlist
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var playlists = await _playlistRepository.GetUserPlaylistsAsync(userId);
            return View(playlists);
        }

        // GET: Playlist/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlist = await _playlistRepository.GetPlaylistByIdAsync(id.Value);
            if (playlist == null)
            {
                return NotFound();
            }

            return View(playlist);
        }

        // GET: Playlist/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Playlist/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,IsPublic")] Playlist playlist)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                playlist.UserId = userId;
                playlist.CreatedAt = DateTime.UtcNow;
        

                await _playlistRepository.CreatePlaylistAsync(playlist);
                return RedirectToAction(nameof(Index));
            }
            return View(playlist);
        }

        // GET: Playlist/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlist = await _playlistRepository.GetPlaylistByIdAsync(id.Value);
            if (playlist == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (playlist.UserId != userId)
            {
                return Forbid();
            }

            return View(playlist);
        }

        // POST: Playlist/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsPublic")] Playlist playlist)
        {
            if (id != playlist.PlaylistId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPlaylist = await _playlistRepository.GetPlaylistByIdAsync(id);
                    if (existingPlaylist == null)
                    {
                        return NotFound();
                    }

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (existingPlaylist.UserId != userId)
                    {
                        return Forbid();
                    }

                    existingPlaylist.Title = playlist.Title;
                    existingPlaylist.Description = playlist.Description;
                    existingPlaylist.IsPublic = playlist.IsPublic;
                    existingPlaylist.CreatedAt = DateTime.UtcNow;

                    await _playlistRepository.UpdatePlaylistAsync(existingPlaylist);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _playlistRepository.PlaylistExistsAsync(playlist.PlaylistId))
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

        // GET: Playlist/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlist = await _playlistRepository.GetPlaylistByIdAsync(id.Value);
            if (playlist == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (playlist.UserId != userId)
            {
                return Forbid();
            }

            return View(playlist);
        }

        // POST: Playlist/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var playlist = await _playlistRepository.GetPlaylistByIdAsync(id);
            if (playlist == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (playlist.UserId != userId)
            {
                return Forbid();
            }

            await _playlistRepository.DeletePlaylistAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: Playlist/AddSong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSong(int playlistId, int songId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var playlist = await _playlistRepository.GetPlaylistByIdAsync(playlistId);
                
                if (playlist == null)
                {
                    return Json(new { success = false, message = "Playlist không tồn tại" });
                }

                if (playlist.UserId != userId)
                {
                    return Json(new { success = false, message = "Bạn không có quyền thêm bài hát vào playlist này" });
                }

                await _playlistRepository.AddSongToPlaylistAsync(playlistId, songId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm bài hát vào playlist" });
            }
        }

        // POST: Playlist/RemoveSong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSong(int playlistId, int songId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var playlist = await _playlistRepository.GetPlaylistByIdAsync(playlistId);
                
                if (playlist == null)
                {
                    return Json(new { success = false, message = "Playlist không tồn tại" });
                }

                if (playlist.UserId != userId)
                {
                    return Json(new { success = false, message = "Bạn không có quyền xóa bài hát khỏi playlist này" });
                }

                await _playlistRepository.RemoveSongFromPlaylistAsync(playlistId, songId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa bài hát khỏi playlist" });
            }
        }

        // POST: Playlist/ReorderSongs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderSongs(int playlistId, List<int> songIds)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var playlist = await _playlistRepository.GetPlaylistByIdAsync(playlistId);
                
                if (playlist == null)
                {
                    return Json(new { success = false, message = "Playlist không tồn tại" });
                }

                if (playlist.UserId != userId)
                {
                    return Json(new { success = false, message = "Bạn không có quyền sắp xếp lại bài hát trong playlist này" });
                }

                await _playlistRepository.ReorderPlaylistSongsAsync(playlistId, songIds);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi sắp xếp lại bài hát" });
            }
        }

        // POST: Playlist/UploadCover
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCover(int playlistId, IFormFile coverImage)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var playlist = await _playlistRepository.GetPlaylistByIdAsync(playlistId);
                
                if (playlist == null)
                {
                    return Json(new { success = false, message = "Playlist không tồn tại" });
                }

                if (playlist.UserId != userId)
                {
                    return Json(new { success = false, message = "Bạn không có quyền thay đổi ảnh bìa của playlist này" });
                }

                if (coverImage == null || coverImage.Length == 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn file ảnh" });
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "playlist-covers");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{playlistId}_{DateTime.UtcNow.Ticks}{Path.GetExtension(coverImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(fileStream);
                }

                playlist.ImageUrl = $"/uploads/playlist-covers/{uniqueFileName}";
                await _playlistRepository.UpdatePlaylistAsync(playlist);

                return Json(new { success = true, coverUrl = playlist.ImageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải lên ảnh bìa" });
            }
        }
    }
} 