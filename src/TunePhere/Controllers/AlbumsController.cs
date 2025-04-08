using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TunePhere.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TunePhere.Controllers
{
    [Authorize]
    public class AlbumsController : Controller
    {
        private readonly AppDbContext _context;

        public AlbumsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Albums
        public async Task<IActionResult> Index(int? artistId = null)
{
    if (artistId.HasValue)
    {
        // Lấy thông tin nghệ sĩ
        var artist = await _context.Artists
            .Include(a => a.Albums)
                .ThenInclude(a => a.Songs)
            .FirstOrDefaultAsync(a => a.ArtistId == artistId);

        if (artist == null)
        {
            return NotFound();
        }

        // Lấy userId của người đang đăng nhập
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Kiểm tra xem người đang xem có phải là chủ sở hữu không
        ViewBag.IsOwner = (currentUserId == artist.userId);
        ViewBag.Artist = artist;
        
        var albums = artist.Albums.OrderByDescending(a => a.ReleaseDate).ToList();
        return View(albums);
    }

    // Logic cũ cho trang Albums tổng
    var popularArtists = await _context.Artists
        .Include(a => a.Songs)
        .OrderByDescending(a => a.Songs.Count)
        .Take(6)
        .ToListAsync();
    ViewBag.PopularArtists = popularArtists;

    var allAlbums = await _context.Albums
        .Include(a => a.Songs)
        .Include(a => a.Artists)
        .OrderByDescending(a => a.ReleaseDate)
        .Take(10)
        .ToListAsync();

    return View(allAlbums);
}
        // GET: Albums/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Artists)
                .Include(a => a.Songs)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.AlbumId == id);

            if (album == null)
            {
                return NotFound();
            }

            // Kiểm tra danh sách bài hát 
            if (album.Songs == null || !album.Songs.Any())
            {
                // Thử load lại bài hát từ DbSet
                var songs = await _context.Songs
                    .Where(s => s.AlbumId == id)
                    .ToListAsync();

                if (songs.Any())
                {
                    // Nếu có bài hát, gán vào album
                    album.Songs = songs;
                }
            }

            return View(album);
        }

        // GET: Albums/Create
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> Create()
        {
            // Lấy thông tin nghệ sĩ hiện tại
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.Artists
                .FirstOrDefaultAsync(a => a.userId == userId);

            if (artist == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Đưa thông tin nghệ sĩ vào ViewBag
            ViewBag.Artists = new List<Artists> { artist };

            return View();
        }

        // POST: Albums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> Create([Bind("AlbumName,AlbumDescription,ReleaseDate,ArtistId")] Album album,
    IFormFile ImageFile, List<IFormFile> SongFiles, List<string> SongTitles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra quyền sở hữu
                    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var artist = await _context.Artists.FirstOrDefaultAsync(a => a.userId == userId);

                    if (artist == null)
                    {
                        ModelState.AddModelError("", "Không tìm thấy thông tin nghệ sĩ");
                        return View(album);
                    }

                    // Đảm bảo ArtistId khớp với nghệ sĩ hiện tại
                    if (album.ArtistId != artist.ArtistId)
                    {
                        ModelState.AddModelError("", "Không có quyền tạo album cho nghệ sĩ khác");
                        return View(album);
                    }

                    // Xử lý upload ảnh album
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "albums");
                        Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        album.ImageUrl = $"/uploads/albums/{uniqueFileName}";
                    }

                    // Khởi tạo giá trị cho album
                    album.numberSongs = 0;
                    album.Time = TimeSpan.Zero;

                    // Tạo album trước để có AlbumId
                    _context.Albums.Add(album);
                    await _context.SaveChangesAsync();

                    // Tạo danh sách bài hát
                    var songs = new List<Song>();
                    var totalDuration = TimeSpan.Zero;

                    // Kiểm tra danh sách SongFiles
                    if (SongFiles != null && SongTitles != null)
                    {
                        for (int i = 0; i < SongFiles.Count; i++)
                        {
                            var songFile = SongFiles[i];
                            var songTitle = (i < SongTitles.Count) ? SongTitles[i] : "Unknown Title";

                            if (songFile != null && songFile.Length > 0)
                            {
                                // Kiểm tra định dạng file
                                var allowedAudioExtensions = new[] { ".mp3", ".m4a", ".wav", ".ogg", ".aac", ".flac" };
                                var audioExtension = Path.GetExtension(songFile.FileName).ToLowerInvariant();

                                if (!allowedAudioExtensions.Contains(audioExtension))
                                {
                                    ModelState.AddModelError("", $"File {songFile.FileName} có định dạng không được hỗ trợ. Hỗ trợ: mp3, m4a, wav, ogg, aac, flac");
                                    continue; // Bỏ qua file không hợp lệ
                                }

                                // Upload file bài hát
                                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "songs");
                                if (!Directory.Exists(uploadsFolder))
                                    Directory.CreateDirectory(uploadsFolder);

                                // Tạo tên file không có khoảng trắng hoặc ký tự đặc biệt
                                var audioFileName = Guid.NewGuid().ToString();
                                var fullAudioFileName = audioFileName + audioExtension;
                                var filePath = Path.Combine(uploadsFolder, fullAudioFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await songFile.CopyToAsync(fileStream);
                                }

                                // Đọc thời lượng từ file audio
                                TimeSpan duration = TimeSpan.Zero;
                                try
                                {
                                    using (var audioFile = TagLib.File.Create(filePath))
                                    {
                                        duration = audioFile.Properties.Duration;
                                        totalDuration = totalDuration.Add(duration);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log lỗi nếu không đọc được thời lượng
                                    Console.WriteLine($"Không thể đọc thời lượng file: {ex.Message}");
                                }

                                // Tạo đối tượng Song
                                var song = new Song
                                {
                                    Title = songTitle,
                                    FileUrl = $"/uploads/songs/{fullAudioFileName}",
                                    ImageUrl = album.ImageUrl ?? "/images/default-song.jpg", // Sử dụng ảnh album
                                    UploadDate = DateTime.Now,
                                    ArtistId = artist.ArtistId,
                                    AlbumId = album.AlbumId,
                                    Genre = "Unknown", // Giá trị mặc định cho Genre
                                    PlayCount = 0,
                                    LikeCount = 0,
                                    Duration = duration,
                                    FileType = audioExtension // Lưu định dạng file
                                };

                                songs.Add(song);
                                _context.Songs.Add(song);  // Thêm trực tiếp vào DbContext
                            }
                        }
                    }

                    // Cập nhật thông tin album
                    album.numberSongs = songs.Count;
                    album.Time = totalDuration;
                    _context.Update(album);

                    await _context.SaveChangesAsync();

                    // Đảm bảo load lại dữ liệu album và songs
                    _context.Entry(album).Reload();
                    foreach (var song in songs)
                    {
                        _context.Entry(song).Reload();
                    }

                    return RedirectToAction(nameof(Details), new { id = album.AlbumId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            // Nếu có lỗi, cần khởi tạo lại ViewBag.Artists
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentArtist = await _context.Artists.FirstOrDefaultAsync(a => a.userId == currentUserId);
            ViewBag.Artists = new List<Artists> { currentArtist };

            return View(album);
        }

        // GET: Albums/Edit/5
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Artists)
                .FirstOrDefaultAsync(a => a.AlbumId == id);

            if (album == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (album.Artists?.userId != userId)
            {
                return Unauthorized();
            }

            return View(album);
        }

        // POST: Albums/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> Edit(int id, [Bind("AlbumId,AlbumName,AlbumDescription,ReleaseDate,ArtistId,ImageUrl,numberSongs,Time")] Album album, IFormFile? AlbumImage)
        {
            if (id != album.AlbumId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAlbum = await _context.Albums
                        .Include(a => a.Artists)
                        .Include(a => a.Songs)
                        .FirstOrDefaultAsync(a => a.AlbumId == id);

                    if (existingAlbum == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra quyền sở hữu
                    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (existingAlbum.Artists?.userId != userId)
                    {
                        return Unauthorized();
                    }

                    // Cập nhật thông tin cơ bản
                    existingAlbum.AlbumName = album.AlbumName;
                    existingAlbum.AlbumDescription = album.AlbumDescription;
                    existingAlbum.ReleaseDate = album.ReleaseDate;

                    // Chỉ cập nhật ảnh nếu có ảnh mới được chọn
                    if (AlbumImage != null && AlbumImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "albums");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        // Xóa ảnh cũ nếu có và khác với ảnh mặc định
                        if (!string.IsNullOrEmpty(existingAlbum.ImageUrl) && !existingAlbum.ImageUrl.Contains("album-placeholder.jpg"))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingAlbum.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}_{AlbumImage.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await AlbumImage.CopyToAsync(fileStream);
                        }

                        existingAlbum.ImageUrl = $"/uploads/albums/{uniqueFileName}";

                        // Cập nhật ImageUrl cho tất cả các bài hát trong album
                        if (existingAlbum.Songs != null)
                        {
                            foreach (var song in existingAlbum.Songs)
                            {
                                song.ImageUrl = existingAlbum.ImageUrl;
                                _context.Update(song);
                            }
                        }
                    }
                    // Nếu không có ảnh mới, giữ nguyên ảnh cũ
                    else
                    {
                        existingAlbum.ImageUrl = album.ImageUrl;
                    }

                    _context.Update(existingAlbum);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index), new { artistId = existingAlbum.ArtistId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlbumExists(album.AlbumId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(album);
        }

        // GET: Albums/Delete/5
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var album = await _context.Albums
                .Include(a => a.Artists)
                .FirstOrDefaultAsync(m => m.AlbumId == id);

            if (album == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (album.Artists?.userId != userId)
            {
                return Unauthorized();
            }

            return View(album);
        }

        // POST: Albums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Sử dụng transaction để đảm bảo tính nhất quán dữ liệu
                using var transaction = await _context.Database.BeginTransactionAsync();

                var album = await _context.Albums
                    .Include(a => a.Artists)
                    .Include(a => a.Songs)
                    .FirstOrDefaultAsync(m => m.AlbumId == id);

                if (album == null)
                {
                    return NotFound();
                }

                // Lưu artistId trước khi xóa album
                var artistId = album.ArtistId;

                // Kiểm tra quyền sở hữu
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (album.Artists?.userId != userId)
                {
                    return Unauthorized();
                }

                // Xóa các bài hát và file bài hát
                if (album.Songs != null && album.Songs.Any())
                {
                    foreach (var song in album.Songs)
                    {
                        // Xóa các bài hát khỏi playlists
                        var playlistSongs = await _context.PlaylistSongs
                            .Where(ps => ps.SongId == song.SongId)
                            .ToListAsync();
                        if (playlistSongs.Any())
                        {
                            _context.PlaylistSongs.RemoveRange(playlistSongs);
                        }

                        // Xóa các remixes liên quan đến bài hát
                        var remixes = await _context.Remixes
                            .Where(r => r.OriginalSongId == song.SongId)
                            .ToListAsync();
                        if (remixes.Any())
                        {
                            _context.Remixes.RemoveRange(remixes);
                        }

                        // Xóa lyrics của bài hát
                        var lyrics = await _context.Lyrics
                            .Where(l => l.SongId == song.SongId)
                            .ToListAsync();
                        if (lyrics.Any())
                        {
                            _context.Lyrics.RemoveRange(lyrics);
                        }

                        // Xóa các bài hát khỏi danh sách yêu thích
                        var favoriteSongs = await _context.UserFavoriteSongs
                            .Where(fs => fs.SongId == song.SongId)
                            .ToListAsync();
                        if (favoriteSongs.Any())
                        {
                            _context.UserFavoriteSongs.RemoveRange(favoriteSongs);
                        }

                        // Xóa file vật lý
                        if (!string.IsNullOrEmpty(song.FileUrl))
                        {
                            var songPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", song.FileUrl.TrimStart('/'));
                            if (System.IO.File.Exists(songPath))
                            {
                                System.IO.File.Delete(songPath);
                            }
                        }
                    }

                    // Lưu các thay đổi về xóa các mối quan hệ trước
                    await _context.SaveChangesAsync();

                    // Sau đó mới xóa các bài hát
                    _context.Songs.RemoveRange(album.Songs);
                    await _context.SaveChangesAsync();
                }

                // Xóa ảnh album
                if (!string.IsNullOrEmpty(album.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", album.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Xóa album
                _context.Albums.Remove(album);
                await _context.SaveChangesAsync();

                // Hoàn thành transaction
                await transaction.CommitAsync();

                // Chuyển hướng về trang Index với artistId
                return RedirectToAction(nameof(Index), new { artistId = artistId });
            }
            catch (Exception ex)
            {
                // Log lỗi và hiển thị thông báo
                Console.WriteLine($"Lỗi khi xóa album: {ex.Message}");
                ModelState.AddModelError("", $"Có lỗi xảy ra khi xóa album: {ex.Message}");
                return View("Delete", await _context.Albums.FindAsync(id));
            }
        }

        // POST: Albums/AddSong
        [HttpPost]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> AddSong(int albumId, string Title, string Genre, IFormFile AudioFile)
        {
            // Kiểm tra nếu album tồn tại
            var album = await _context.Albums
                .Include(a => a.Artists)
                .FirstOrDefaultAsync(a => a.AlbumId == albumId);

            if (album == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (album.Artists?.userId != userId)
            {
                return Unauthorized();
            }

            // Kiểm tra file audio có tồn tại không
            if (AudioFile == null || AudioFile.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file nhạc hợp lệ";
                return RedirectToAction(nameof(Details), new { id = albumId });
            }

            // Mở rộng danh sách định dạng được hỗ trợ
            var allowedAudioExtensions = new[] { ".mp3", ".m4a", ".wav", ".ogg", ".aac", ".flac" };
            var audioExtension = Path.GetExtension(AudioFile.FileName).ToLowerInvariant();

            if (!allowedAudioExtensions.Contains(audioExtension))
            {
                TempData["Error"] = "Định dạng file không được hỗ trợ. Hỗ trợ: mp3, m4a, wav, ogg, aac, flac";
                return RedirectToAction(nameof(Details), new { id = albumId });
            }

            // Xử lý upload file nhạc
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "songs");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Tạo tên file không có khoảng trắng hoặc ký tự đặc biệt
            var audioFileName = Guid.NewGuid().ToString();
            var fullAudioFileName = audioFileName + audioExtension;
            var filePath = Path.Combine(uploadsFolder, fullAudioFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await AudioFile.CopyToAsync(fileStream);
            }

            // Đọc thời lượng từ file audio
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                using (var audioFile = TagLib.File.Create(filePath))
                {
                    duration = audioFile.Properties.Duration;
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nếu không đọc được thời lượng
                Console.WriteLine($"Không thể đọc thời lượng file: {ex.Message}");
                // Nếu không đọc được thời lượng, xóa file đã upload và báo lỗi
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                TempData["Error"] = "File âm thanh không hợp lệ hoặc bị hỏng";
                return RedirectToAction(nameof(Details), new { id = albumId });
            }

            // Tạo đối tượng Song
            var song = new Song
            {
                Title = Title,
                Genre = Genre,
                FileUrl = $"/uploads/songs/{fullAudioFileName}",
                // Sử dụng ảnh album làm ảnh bìa cho bài hát
                ImageUrl = album.ImageUrl ?? "/images/default-song.jpg",
                UploadDate = DateTime.Now,
                ArtistId = album.Artists.ArtistId,
                AlbumId = albumId,
                PlayCount = 0,
                LikeCount = 0,
                Duration = duration,
                FileType = audioExtension // Lưu định dạng file
            };

            _context.Songs.Add(song);

            // Cập nhật thông tin album
            album.numberSongs += 1;
            album.Time = album.Time.Add(duration);
            _context.Update(album);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = albumId });
        }

        // GET: Albums/AddSong
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> AddSong(int albumId)
        {
            // Kiểm tra nếu album tồn tại
            var album = await _context.Albums
                .Include(a => a.Artists)
                .FirstOrDefaultAsync(a => a.AlbumId == albumId);

            if (album == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền sở hữu
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (album.Artists?.userId != userId)
            {
                return Unauthorized();
            }

            return View(albumId);
        }

        // Xóa bài hát khỏi album
        [HttpPost]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> DeleteSong(int songId)
        {
            int? albumId = null;

            try
            {
                // Sử dụng transaction để đảm bảo tính nhất quán dữ liệu
                using var transaction = await _context.Database.BeginTransactionAsync();

                var song = await _context.Songs
                    .Include(s => s.Albums)
                    .ThenInclude(a => a.Artists)
                    .FirstOrDefaultAsync(s => s.SongId == songId);

                if (song == null)
                {
                    return NotFound();
                }

                // Kiểm tra quyền sở hữu
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (song.Albums?.Artists?.userId != userId)
                {
                    return Unauthorized();
                }

                // Lưu albumId và duration để sử dụng sau
                albumId = song.AlbumId;
                var duration = song.Duration;

                // 1. Xóa lịch sử nghe nhạc
                var playHistories = await _context.PlayHistories
                    .Where(ph => ph.SongId == songId)
                    .ToListAsync();
                if (playHistories.Any())
                {
                    _context.PlayHistories.RemoveRange(playHistories);
                    await _context.SaveChangesAsync();
                }

                // 2. Xóa các bài hát khỏi playlists
                var playlistSongs = await _context.PlaylistSongs
                    .Where(ps => ps.SongId == songId)
                    .ToListAsync();
                if (playlistSongs.Any())
                {
                    _context.PlaylistSongs.RemoveRange(playlistSongs);
                    await _context.SaveChangesAsync();
                }

                // 3. Xóa các remixes liên quan đến bài hát
                var remixes = await _context.Remixes
                    .Where(r => r.OriginalSongId == songId)
                    .ToListAsync();
                if (remixes.Any())
                {
                    _context.Remixes.RemoveRange(remixes);
                    await _context.SaveChangesAsync();
                }

                // 4. Xóa lyrics của bài hát
                var lyrics = await _context.Lyrics
                    .Where(l => l.SongId == songId)
                    .ToListAsync();
                if (lyrics.Any())
                {
                    _context.Lyrics.RemoveRange(lyrics);
                    await _context.SaveChangesAsync();
                }

                // 5. Xóa các bài hát khỏi danh sách yêu thích
                var favoriteSongs = await _context.UserFavoriteSongs
                    .Where(fs => fs.SongId == songId)
                    .ToListAsync();
                if (favoriteSongs.Any())
                {
                    _context.UserFavoriteSongs.RemoveRange(favoriteSongs);
                    await _context.SaveChangesAsync();
                }

                // Xóa file nhạc
                if (!string.IsNullOrEmpty(song.FileUrl))
                {
                    var audioPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", song.FileUrl.TrimStart('/'));
                    if (System.IO.File.Exists(audioPath))
                    {
                        System.IO.File.Delete(audioPath);
                    }
                }

                // Xóa bài hát
                _context.Songs.Remove(song);

                // Cập nhật thông tin album
                if (albumId.HasValue)
                {
                    var album = await _context.Albums.FindAsync(albumId.Value);
                    if (album != null)
                    {
                        album.numberSongs = Math.Max(0, album.numberSongs - 1);
                        album.Time = album.Time.Subtract(duration);
                        if (album.Time < TimeSpan.Zero)
                            album.Time = TimeSpan.Zero;

                        _context.Update(album);
                    }
                }

                await _context.SaveChangesAsync();

                // Hoàn thành transaction
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Details), new { id = albumId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa bài hát: {ex.Message}");
                TempData["Error"] = $"Có lỗi xảy ra khi xóa bài hát: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = albumId });
            }
        }

        // Thêm bài hát vào playlist
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddToPlaylist(int songId, int playlistId)
        {
            try
            {
                // Kiểm tra bài hát tồn tại
                var song = await _context.Songs.FindAsync(songId);
                if (song == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài hát" });
                }

                // Kiểm tra playlist tồn tại và thuộc về user hiện tại
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var playlist = await _context.Playlists
                    .FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == userId);

                if (playlist == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy playlist" });
                }

                // Kiểm tra bài hát đã có trong playlist chưa
                var existingEntry = await _context.PlaylistSongs
                    .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

                if (existingEntry != null)
                {
                    return Json(new { success = false, message = "Bài hát đã có trong playlist" });
                }

                // Thêm bài hát vào playlist
                var playlistSong = new PlaylistSong
                {
                    PlaylistId = playlistId,
                    SongId = songId,
                    AddedAt = DateTime.Now,
                    AddedByUserId = userId
                };

                _context.PlaylistSongs.Add(playlistSong);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã thêm bài hát vào playlist" });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Console.WriteLine($"Lỗi khi thêm bài hát vào playlist: {ex.Message}");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Lấy danh sách playlist của user
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPlaylists()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var playlists = await _context.Playlists
                .Where(p => p.UserId == userId)
                .Select(p => new { id = p.PlaylistId, name = p.Title })
                .ToListAsync();

            return Json(playlists);
        }

        private bool AlbumExists(int id)
        {
            return _context.Albums.Any(e => e.AlbumId == id);
        }
    }
}
