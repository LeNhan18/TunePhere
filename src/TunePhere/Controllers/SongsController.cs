using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using Microsoft.AspNetCore.Hosting;
using TagLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace TunePhere.Controllers
{
    [Authorize] // Require authentication
    public class SongsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<AppUser> _userManager;

        public SongsController(
            AppDbContext context, 
            IWebHostEnvironment environment,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
        }

        // GET: Songs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Songs.Include(s => s.Artists).ToListAsync());
        }

        // GET: Songs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Songs
                .Include(s => s.Artists)
                .Include(s => s.Lyrics)
                .FirstOrDefaultAsync(m => m.SongId == id);
            if (song == null)
            {
                return NotFound();
            }

            // Thêm headers để tránh cache
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return View(song);
        }

        // GET: Songs/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            // Kiểm tra xem người dùng có phải là nghệ sĩ không
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var artist = await _context.Artists
                .FirstOrDefaultAsync(a => a.userId == user.Id);

            if (artist == null)
            {
                return RedirectToAction("Create", "Artists", new { returnUrl = Url.Action("Create", "Songs") });
            }

            return View();
        }

        // POST: Songs/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Song song, IFormFile audioFile, IFormFile imageFile)
        {
            try
            {
                // Get current user's artist profile
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge();
                }

                var artist = await _context.Artists
                    .FirstOrDefaultAsync(a => a.userId == user.Id);

                if (artist == null)
                {
                    return RedirectToAction("Create", "Artists", new { returnUrl = Url.Action("Create", "Songs") });
                }

                if (audioFile == null || imageFile == null)
                {
                    ModelState.AddModelError("", "Vui lòng chọn file nhạc và ảnh bìa");
                    return View(song);
                }

                // Validate VideoUrl if provided
                if (!string.IsNullOrEmpty(song.VideoUrl) && !song.VideoUrl.StartsWith("https://"))
                {
                    ModelState.AddModelError("VideoUrl", "Link video phải bắt đầu bằng https://");
                    return View(song);
                }

                // Create upload directories if they don't exist
                var songUploadPath = Path.Combine(_environment.WebRootPath, "uploads", "songs");
                var coverUploadPath = Path.Combine(_environment.WebRootPath, "uploads", "covers");
                Directory.CreateDirectory(songUploadPath);
                Directory.CreateDirectory(coverUploadPath);

                // Save audio file
                var audioFileName = Guid.NewGuid().ToString() + Path.GetExtension(audioFile.FileName);
                var audioPath = Path.Combine(songUploadPath, audioFileName);
                using (var stream = new FileStream(audioPath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                // Get audio duration using TagLib
                using (var tagFile = TagLib.File.Create(audioPath))
                {
                    song.Duration = tagFile.Properties.Duration;
                }

                // Save image file
                var imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var imagePath = Path.Combine(coverUploadPath, imageFileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Set song properties
                song.ArtistId = artist.ArtistId;
                song.FileUrl = "/uploads/songs/" + audioFileName;
                song.ImageUrl = "/uploads/covers/" + imageFileName;
                song.UploadDate = DateTime.Now;
                song.PlayCount = 0;
                song.LikeCount = 0;

                _context.Add(song);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm bài hát: " + ex.Message);
                return View(song);
            }
        }

        // GET: Songs/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Songs
                .Include(s => s.Artists)
                .FirstOrDefaultAsync(m => m.SongId == id);

            if (song == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu
            var user = await _userManager.GetUserAsync(User);
            if (user == null || song.Artists?.userId != user.Id)
            {
                return Forbid();
            }

            return View(song);
        }

        // POST: Songs/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SongId,Title,Genre,Duration,FileUrl,ImageUrl,VideoUrl,UploadDate,PlayCount,LikeCount,ArtistId")] Song song)
        {
            if (id != song.SongId)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu
            var existingSong = await _context.Songs
                .Include(s => s.Artists)
                .FirstOrDefaultAsync(s => s.SongId == id);

            if (existingSong == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || existingSong.Artists?.userId != user.Id)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(song);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SongExists(song.SongId))
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
            return View(song);
        }

        // GET: Songs/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Songs
                .Include(s => s.Artists)
                .FirstOrDefaultAsync(m => m.SongId == id);

            if (song == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu
            var user = await _userManager.GetUserAsync(User);
            if (user == null || song.Artists?.userId != user.Id)
            {
                return Forbid();
            }

            return View(song);
        }

        // POST: Songs/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var song = await _context.Songs
                .Include(s => s.Artists)
                .FirstOrDefaultAsync(m => m.SongId == id);

            if (song == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu
            var user = await _userManager.GetUserAsync(User);
            if (user == null || song.Artists?.userId != user.Id)
            {
                return Forbid();
            }

            // Xóa file nhạc và ảnh bìa
            if (!string.IsNullOrEmpty(song.FileUrl))
            {
                var audioPath = Path.Combine(_environment.WebRootPath, song.FileUrl.TrimStart('/'));
                if (System.IO.File.Exists(audioPath))
                {
                    System.IO.File.Delete(audioPath);
                }
            }

            if (!string.IsNullOrEmpty(song.ImageUrl))
            {
                var imagePath = Path.Combine(_environment.WebRootPath, song.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Songs/IncrementPlayCount/5
        [HttpPost]
        public async Task<IActionResult> IncrementPlayCount(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người nghe có phải là nghệ sĩ của bài hát không
            var user = await _userManager.GetUserAsync(User);
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.userId == user.Id);
            
            if (artist == null || artist.ArtistId != song.ArtistId)
            {
                song.PlayCount++;
                await _context.SaveChangesAsync();
            }

            return Ok(new { playCount = song.PlayCount });
        }

        private bool SongExists(int id)
        {
            return _context.Songs.Any(e => e.SongId == id);
        }
    }
}
