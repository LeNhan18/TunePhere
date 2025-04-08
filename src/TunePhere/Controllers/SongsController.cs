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
using Microsoft.AspNetCore.ResponseCaching;

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
        public async Task<IActionResult> Details(int? id, int? playlistId, int? index, int? albumId, bool? fromFavorites, int? artistId, bool? fromArtist, string context_type = "")
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

            // Đếm lại số lượt thích
            var likeCount = await _context.UserFavoriteSongs.CountAsync(f => f.SongId == id.Value);
            if (song.LikeCount != likeCount)
            {
                song.LikeCount = likeCount;
                await _context.SaveChangesAsync();
            }

            // Xác định nguồn điều hướng từ context_type hoặc tham số
            bool isFromArtist = fromArtist ?? (context_type == "artist");

            // LUÔN lấy dữ liệu bài hát của nghệ sĩ trong mọi trường hợp
            int targetArtistId = artistId ?? song.ArtistId;

            if (targetArtistId > 0)
            {
                try
                {
                    // Lấy danh sách bài hát của nghệ sĩ
                    var artistSongs = await _context.Songs
                        .AsNoTracking()
                        .Where(s => s.ArtistId == targetArtistId)
                        .OrderBy(s => s.SongId) // Sắp xếp cố định
                        .Select(s => new {
                            id = s.SongId,
                            title = s.Title,
                            artist = s.Artists.ArtistName,
                            fileUrl = s.FileUrl,
                            imageUrl = s.ImageUrl
                        })
                        .ToListAsync();

                    var artist = await _context.Artists
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.ArtistId == targetArtistId);

                    if (artistSongs.Any())
                    {
                        int currentIndex = artistSongs.FindIndex(s => s.id == id);
                        _logger.LogInformation($"Artist Songs: {artistSongs.Count}, Current Index: {currentIndex}");

                        ViewData["ArtistId"] = targetArtistId;
                        ViewData["ArtistName"] = artist?.ArtistName;
                        ViewData["ArtistSongs"] = System.Text.Json.JsonSerializer.Serialize(artistSongs);
                        ViewData["ArtistCurrentIndex"] = currentIndex;
                        ViewData["ArtistSongsCount"] = artistSongs.Count;
                        ViewData["FromArtist"] = isFromArtist; // Ghi nhớ nguồn điều hướng
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi lấy danh sách bài hát của nghệ sĩ ID={ArtistId}", targetArtistId);
                }
            }

            // Kiểm tra người dùng đã yêu thích bài hát chưa
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                ViewData["IsFavorited"] = await _context.UserFavoriteSongs
                    .AnyAsync(f => f.SongId == song.SongId && f.UserId == userId);

                // Nếu yêu cầu xem từ danh sách yêu thích HOẶC không có playlist/album, lấy danh sách bài hát yêu thích
                if (fromFavorites == true)
                {
                    var favoriteItems = await _context.UserFavoriteSongs
                        .Where(f => f.UserId == userId)
                        .Include(f => f.Song)
                        .ThenInclude(s => s.Artists)
                        .OrderByDescending(f => f.AddedDate)
                        .ToListAsync();

                    if (favoriteItems.Any())
                    {
                        var favoriteSongs = favoriteItems
                            .Select(f => new {
                                id = f.Song.SongId,
                                title = f.Song.Title,
                                artist = f.Song.Artists?.ArtistName ?? "Unknown Artist",
                                fileUrl = f.Song.FileUrl,
                                imageUrl = f.Song.ImageUrl
                            })
                            .ToList();

                        // Tìm vị trí của bài hát hiện tại trong danh sách yêu thích
                        var currentIndex = index.HasValue ? index.Value : favoriteSongs.FindIndex(s => s.id == id);

                        ViewData["FavoritesSongs"] = System.Text.Json.JsonSerializer.Serialize(favoriteSongs);
                        ViewData["FavoritesCurrentIndex"] = currentIndex;
                        ViewData["FromFavorites"] = true;
                    }
                }
            }

            // Nếu có playlistId, lấy thông tin các bài hát trong playlist
            if (playlistId.HasValue)
            {
                var playlist = await _context.Playlists
                    .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                    .FirstOrDefaultAsync(p => p.PlaylistId == playlistId);

                if (playlist != null)
                {
                    var playlistSongs = playlist.PlaylistSongs
                        .Where(ps => ps.Song != null)
                        .OrderBy(ps => ps.AddedAt)
                        .Select(ps => new {
                            id = ps.Song.SongId,
                            title = ps.Song.Title,
                            artist = ps.Song.Artists?.ArtistName ?? "Unknown Artist",
                            fileUrl = ps.Song.FileUrl,
                            imageUrl = ps.Song.ImageUrl
                        })
                        .ToList();

                    ViewData["PlaylistId"] = playlistId;
                    ViewData["PlaylistTitle"] = playlist.Title;
                    ViewData["PlaylistSongs"] = System.Text.Json.JsonSerializer.Serialize(playlistSongs);
                    ViewData["CurrentIndex"] = index ?? playlistSongs.FindIndex(s => s.id == id);
                }
            }
            // Nếu có albumId hoặc bài hát thuộc album, lấy thông tin các bài hát trong album
            else if (albumId.HasValue || song.AlbumId.HasValue)
            {
                int targetAlbumId = albumId ?? song.AlbumId.Value;
                var album = await _context.Albums
                    .Include(a => a.Songs)
                    .FirstOrDefaultAsync(a => a.AlbumId == targetAlbumId);

                if (album != null && album.Songs.Any())
                {
                    var albumSongs = album.Songs
                        .Where(s => s.IsActive)
                        .OrderBy(s => s.UploadDate)
                        .Select(s => new {
                            id = s.SongId,
                            title = s.Title,
                            artist = s.Artists?.ArtistName ?? "Unknown Artist",
                            fileUrl = s.FileUrl,
                            imageUrl = s.ImageUrl
                        })
                        .ToList();

                    ViewData["AlbumId"] = album.AlbumId;
                    ViewData["AlbumTitle"] = album.AlbumName;
                    ViewData["AlbumSongs"] = System.Text.Json.JsonSerializer.Serialize(albumSongs);
                    ViewData["CurrentIndex"] = albumSongs.FindIndex(s => s.id == id);
                }
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

                // Thêm kiểm tra file audio
                if (audioFile == null || audioFile.Length == 0)
                {
                    ModelState.AddModelError("audioFile", "File âm thanh không hợp lệ");
                    return View(song);
                }

                // Mở rộng danh sách định dạng được hỗ trợ
                var allowedAudioExtensions = new[] { ".mp3", ".m4a", ".wav", ".ogg", ".aac", ".flac" };
                var audioExtension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();

                if (!allowedAudioExtensions.Contains(audioExtension))
                {
                    ModelState.AddModelError("audioFile", "Định dạng file không được hỗ trợ. Hỗ trợ: mp3, m4a, wav, ogg, aac, flac");
                    return View(song);
                }

                // Create upload directories if they don't exist
                var songUploadPath = Path.Combine(_environment.WebRootPath, "uploads", "songs");
                var coverUploadPath = Path.Combine(_environment.WebRootPath, "uploads", "covers");
                Directory.CreateDirectory(songUploadPath);
                Directory.CreateDirectory(coverUploadPath);

                // Tạo tên file không có khoảng trắng, dấu tiếng Việt và ký tự đặc biệt
                var audioFileName = Guid.NewGuid().ToString();
                var fullAudioFileName = audioFileName + audioExtension;
                var audioPath = Path.Combine(songUploadPath, fullAudioFileName);

                using (var stream = new FileStream(audioPath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                // Lưu định dạng file để có thể kiểm tra sau này
                // Thêm thuộc tính mới vào bảng Songs nếu cần
                song.FileUrl = "/uploads/songs/" + fullAudioFileName;
                song.FileType = audioExtension; // Giả sử bạn đã thêm trường này vào model

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
                // Log lỗi chi tiết
                _logger.LogError(ex, "Lỗi upload: {Message}", ex.Message);
                ModelState.AddModelError("", "Lỗi khi tải lên: " + ex.Message);
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
        public async Task<IActionResult> Edit(int id, [Bind("SongId,Title,Genre,Duration,FileUrl,ImageUrl,VideoUrl,UploadDate,PlayCount,LikeCount,ArtistId")] Song song, IFormFile? imageFile)
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
                    // Xử lý upload ảnh mới nếu có
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Xóa ảnh cũ nếu tồn tại
                        if (!string.IsNullOrEmpty(existingSong.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_environment.WebRootPath, existingSong.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Lưu ảnh mới
                        var coverUploadPath = Path.Combine(_environment.WebRootPath, "uploads", "covers");
                        Directory.CreateDirectory(coverUploadPath);

                        var imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var imagePath = Path.Combine(coverUploadPath, imageFileName);

                        using (var stream = new FileStream(imagePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        // Cập nhật đường dẫn ảnh mới
                        song.ImageUrl = "/uploads/covers/" + imageFileName;
                    }
                    else
                    {
                        // Nếu không có ảnh mới, giữ nguyên ảnh cũ
                        song.ImageUrl = existingSong.ImageUrl;
                    }

                    // Cập nhật thông tin bài hát
                    _context.Entry(song).State = EntityState.Modified;

                    // Giữ nguyên các thông tin không được phép thay đổi
                    song.ArtistId = existingSong.ArtistId;
                    song.FileUrl = existingSong.FileUrl;
                    song.UploadDate = existingSong.UploadDate;
                    song.PlayCount = existingSong.PlayCount;
                    song.LikeCount = existingSong.LikeCount;
                    song.Duration = existingSong.Duration;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
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
            try
            {
                // Sử dụng transaction để đảm bảo tính nhất quán
                using var transaction = await _context.Database.BeginTransactionAsync();

                var song = await _context.Songs
                    .Include(s => s.Artists)
                    .Include(s => s.Lyrics)
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

                // 1. Xóa tất cả lịch sử nghe nhạc liên quan đến bài hát
                var playHistories = await _context.PlayHistories
                    .Where(ph => ph.SongId == id)
                    .ToListAsync();
                if (playHistories.Any())
                {
                    _context.PlayHistories.RemoveRange(playHistories);
                    await _context.SaveChangesAsync();
                }

                // 2. Xóa tất cả playlist songs liên quan đến bài hát
                var playlistSongs = await _context.PlaylistSongs
                    .Where(ps => ps.SongId == id)
                    .ToListAsync();
                if (playlistSongs.Any())
                {
                    _context.PlaylistSongs.RemoveRange(playlistSongs);
                    await _context.SaveChangesAsync();
                }

                // 3. Xóa tất cả remixes liên quan đến bài hát (nếu có)
                var remixes = await _context.Remixes
                    .Where(r => r.OriginalSongId == id)
                    .ToListAsync();
                if (remixes.Any())
                {
                    _context.Remixes.RemoveRange(remixes);
                    await _context.SaveChangesAsync();
                }

                // 4. Xóa tất cả lyrics của bài hát
                var lyrics = await _context.Lyrics
                    .Where(l => l.SongId == id)
                    .ToListAsync();
                if (lyrics.Any())
                {
                    _context.Lyrics.RemoveRange(lyrics);
                    await _context.SaveChangesAsync();
                }

                // 5. Xóa tất cả UserFavoriteSongs liên quan đến bài hát
                var userFavorites = await _context.UserFavoriteSongs
                    .Where(f => f.SongId == id)
                    .ToListAsync();
                if (userFavorites.Any())
                {
                    _context.UserFavoriteSongs.RemoveRange(userFavorites);
                    await _context.SaveChangesAsync();
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

                // Xóa bài hát
                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();

                // Hoàn thành transaction
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log lỗi và hiển thị thông báo
                _logger.LogError(ex, "Lỗi khi xóa bài hát: {Message}", ex.Message);
                TempData["Error"] = "Không thể xóa bài hát. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Index));
            }
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

            return Json(new
            {
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