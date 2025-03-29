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
using Microsoft.Extensions.Logging;

namespace TunePhere.Controllers
{
    public class SongsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<SongsController> _logger;

        public SongsController(
            AppDbContext context,
            IWebHostEnvironment environment,
            UserManager<AppUser> userManager,
            ILogger<SongsController> logger)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? artistId)
        {
            try
            {
                // Nếu có artistId, lấy bài hát của nghệ sĩ đó
                if (artistId.HasValue)
                {
                    var artist = await _context.Artists
                        .FirstOrDefaultAsync(a => a.ArtistId == artistId.Value);

                    if (artist == null)
                    {
                        return NotFound();
                    }

                    var songs = await _context.Songs
                        .Include(s => s.Artists)
                        .Where(s => s.ArtistId == artistId.Value)
                        .ToListAsync();

                    ViewBag.Artist = artist;
                    return View(songs);
                }

                // Nếu không có artistId, kiểm tra xem người dùng có đăng nhập không
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    // Kiểm tra xem người dùng có phải là nghệ sĩ không
                    var currentArtist = await _context.Artists
                        .FirstOrDefaultAsync(a => a.userId == user.Id);

                    if (currentArtist != null)
                    {
                        // Nếu người dùng là nghệ sĩ, hiển thị bài hát của họ
                        var artistSongs = await _context.Songs
                            .Include(s => s.Artists)
                            .Where(s => s.ArtistId == currentArtist.ArtistId)
                            .ToListAsync();

                        ViewBag.Artist = currentArtist;
                        return View(artistSongs);
                    }
                }
                
                // Nếu không phải nghệ sĩ hoặc không đăng nhập, hiển thị tất cả bài hát
                var allSongs = await _context.Songs
                    .Include(s => s.Artists)
                    .OrderByDescending(s => s.PlayCount)
                    .Take(50)  // Giới hạn số lượng bài hát hiển thị
                    .ToListAsync();
                    
                ViewBag.Title = "Bài hát nổi bật";
                return View(allSongs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching songs");
                return View(new List<Song>());
            }
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

            // Đếm lại số lượt thích từ bảng UserFavoriteSong
            var likeCount = await _context.UserFavoriteSongs.CountAsync(f => f.SongId == id.Value);
            
            // Cập nhật lại LikeCount nếu không khớp với số thực tế
            if (song.LikeCount != likeCount)
            {
                song.LikeCount = likeCount;
                await _context.SaveChangesAsync();
            }

            // Kiểm tra người dùng đã yêu thích bài hát chưa
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                ViewData["IsFavorited"] = await _context.UserFavoriteSongs
                    .AnyAsync(f => f.SongId == song.SongId && f.UserId == userId);
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
                .AsNoTracking()
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
                    // Cập nhật thông tin bài hát
                    _context.Entry(song).State = EntityState.Modified;

                    // Giữ nguyên các thông tin không được phép thay đổi
                    song.ArtistId = existingSong.ArtistId;
                    song.FileUrl = existingSong.FileUrl;
                    song.ImageUrl = existingSong.ImageUrl;
                    song.UploadDate = existingSong.UploadDate;
                    song.PlayCount = existingSong.PlayCount;
                    song.LikeCount = existingSong.LikeCount;

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

        // GET: Songs/Search
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Json(new List<object>());
            }

            var songs = await _context.Songs
                .Include(s => s.Artists)
                .Where(s => s.Title.ToLower().Contains(query.ToLower()) || 
                           s.Artists.ArtistName.ToLower().Contains(query.ToLower()))
                .Select(s => new
                {
                    songId = s.SongId,
                    title = s.Title,
                    artistName = s.Artists.ArtistName,
                    imageUrl = s.ImageUrl
                })
                .ToListAsync();

            return Json(songs);
        }

        // POST: Songs/ToggleFavorite/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            
            // Kiểm tra xem người dùng đã thích bài hát này chưa
            var favorite = await _context.UserFavoriteSongs
                .FirstOrDefaultAsync(f => f.UserId == userId && f.SongId == id);

            bool isNowLiked = false;
            
            if (favorite == null)
            {
                // Chưa thích - thêm vào danh sách yêu thích
                _context.UserFavoriteSongs.Add(new UserFavoriteSong
                {
                    UserId = userId,
                    SongId = id,
                    AddedDate = DateTime.Now
                });
                isNowLiked = true;
            }
            else
            {
                // Đã thích - xóa khỏi danh sách yêu thích
                _context.UserFavoriteSongs.Remove(favorite);
                isNowLiked = false;
            }
            
            await _context.SaveChangesAsync();
            
            // Đếm lại số lượt thích từ bảng UserFavoriteSong
            var likeCount = await _context.UserFavoriteSongs.CountAsync(f => f.SongId == id);
            
            // Cập nhật lại trường LikeCount trong bảng Song
            song.LikeCount = likeCount;
            await _context.SaveChangesAsync();
            
            return Json(new { 
                liked = isNowLiked,
                likeCount = likeCount 
            });
        }

        private bool SongExists(int id)
        {
            return _context.Songs.Any(e => e.SongId == id);
        }
    }
}
