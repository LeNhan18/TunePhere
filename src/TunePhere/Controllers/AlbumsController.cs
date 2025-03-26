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
        public async Task<IActionResult> Index()
        {
            // Lấy nghệ sĩ phổ biến (dựa trên số lượng bài hát)
            var popularArtists = await _context.Artists
                .Include(a => a.Songs)
                .OrderByDescending(a => a.Songs.Count)
                .Take(6)
                .ToListAsync();
            ViewBag.PopularArtists = popularArtists;

            // Lấy tất cả album, sắp xếp theo ngày phát hành mới nhất
            var albums = await _context.Albums
                .Include(a => a.Songs)
                .Include(a => a.Artists)
                .OrderByDescending(a => a.ReleaseDate)
                .Take(10) // Lấy 10 album mới nhất
                .ToListAsync();

            // Kiểm tra role của user hiện tại
            var isArtist = User.IsInRole("Artist");
            ViewBag.IsArtist = isArtist;

            // Nếu user đã đăng nhập và là nghệ sĩ, lấy thêm album của họ
            if (User.Identity.IsAuthenticated && isArtist)
            {
                string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var myAlbums = await _context.Albums
                    .Include(a => a.Songs)
                    .Include(a => a.Artists)
                    .Where(a => a.Artists.userId == currentUserId)
                    .OrderByDescending(a => a.ReleaseDate)
                    .ToListAsync();
                
                ViewBag.MyAlbums = myAlbums;
            }

            return View(albums);
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
                .FirstOrDefaultAsync(m => m.AlbumId == id);

            if (album == null)
            {
                return NotFound();
            }

            return View(album);
        }

        // GET: Albums/Create
        [Authorize(Roles = "Artist")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Albums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> Create([Bind("AlbumName,AlbumDescription,ReleaseDate")] Album album, 
            IFormFile ImageFile, List<IFormFile> SongFiles, List<string> SongTitles)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy ArtistId từ user hiện tại
                    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var artist = await _context.Artists.FirstOrDefaultAsync(a => a.userId == userId);
                    
                    if (artist == null)
                    {
                        ModelState.AddModelError("", "Không tìm thấy thông tin nghệ sĩ");
                        return View(album);
                    }

                    album.Artists = artist;

                    // Xử lý upload ảnh album
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "albums");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var uniqueFileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        album.ImageUrl = $"/uploads/albums/{uniqueFileName}";
                    }

                    // Tạo album trước để có AlbumId
                    _context.Add(album);
                    await _context.SaveChangesAsync();

                    // Tạo danh sách bài hát
                    var songs = new List<Song>();
                    var totalDuration = TimeSpan.Zero;
                    for (int i = 0; i < SongFiles?.Count; i++)
                    {
                        var songFile = SongFiles[i];
                        var songTitle = SongTitles[i];

                        if (songFile != null && songFile.Length > 0)
                        {
                            // Upload file bài hát
                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "songs");
                            if (!Directory.Exists(uploadsFolder))
                                Directory.CreateDirectory(uploadsFolder);

                            var uniqueFileName = $"{Guid.NewGuid()}_{songFile.FileName}";
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

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
                                FileUrl = $"/uploads/songs/{uniqueFileName}",
                                ImageUrl = album.ImageUrl ?? "/images/default-song.jpg", // Sử dụng ảnh album hoặc ảnh mặc định
                                UploadDate = DateTime.Now,
                                ArtistId = artist.ArtistId,
                                AlbumId = album.AlbumId,
                                Genre = "Unknown", // Giá trị mặc định cho Genre
                                PlayCount = 0,
                                LikeCount = 0,
                                Duration = duration
                            };

                            songs.Add(song);
                        }
                    }

                    if (songs.Any())
                    {
                        await _context.Songs.AddRangeAsync(songs);
                        await _context.SaveChangesAsync();

                        // Cập nhật thông tin album
                        album.Songs = songs;
                        album.numberSongs = songs.Count;
                        album.Time = totalDuration;
                        _context.Update(album);
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Details), new { id = album.AlbumId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo album: {ex.Message}");
                }
            }
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

            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (album.Artists.userId != currentUserId)
            {
                return Forbid();
            }

            return View(album);
        }

        // POST: Albums/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> Edit(int id, [Bind("AlbumId,AlbumName,AlbumDescription,ReleaseDate")] Album album, IFormFile AlbumImage)
        {
            if (id != album.AlbumId)
            {
                return NotFound();
            }

            // Kiểm tra quyền chỉnh sửa
            var existingAlbum = await _context.Albums
                .Include(a => a.Artists)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AlbumId == id);

            if (existingAlbum == null)
            {
                return NotFound();
            }

            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingAlbum.Artists.userId != currentUserId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (AlbumImage != null && AlbumImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "albums");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingAlbum.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingAlbum.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }

                        var uniqueFileName = $"{Guid.NewGuid()}_{AlbumImage.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await AlbumImage.CopyToAsync(fileStream);
                        }

                        album.ImageUrl = $"/uploads/albums/{uniqueFileName}";
                    }
                    else
                    {
                        album.ImageUrl = existingAlbum.ImageUrl;
                    }

                    _context.Update(album);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Details), new { id = album.AlbumId });
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

            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (album.Artists.userId != currentUserId)
            {
                return Forbid();
            }

            return View(album);
        }

        // POST: Albums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Artist")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Artists)
                .FirstOrDefaultAsync(a => a.AlbumId == id);

            if (album == null)
            {
                return NotFound();
            }

            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (album.Artists.userId != currentUserId)
            {
                return Forbid();
            }

            // Xóa ảnh album nếu có
            if (!string.IsNullOrEmpty(album.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", album.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Albums.Remove(album);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlbumExists(int id)
        {
            return _context.Albums.Any(e => e.AlbumId == id);
        }
    }
}

